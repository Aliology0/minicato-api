using AlMostashar.Application.Common.Interfaces;
using AlMostashar.Application.Common.Locations;
using AlMostashar.Infrastructure.Data;
using AlMostashar.Infrastructure.Options;
using AlMostashar.Infrastructure.Services;
using Amazon.S3;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AlMostashar.Infrastructure.Helpers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDataProtection()
                .SetApplicationName("AlMostashar.Api")
                .PersistKeysToDbContext<AlmostasharDbContext>();
            services.AddSingleton<ISensitiveDataProtector, DataProtectionSensitiveDataProtector>();

            // Database
            services.AddDbContext<AlmostasharDbContext>((options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("AlmostasharSqlServer"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null));
            });
            services.AddScoped<IAppDbContext, AlmostasharDbContext>();
            services.AddSingleton<ILocationCatalog, JsonLocationCatalog>();

            // Auth service
            services.AddScoped<IAuthService, AuthService>();

            // Email service
            services.AddScoped<IEmailService, EmailService>();

            // Admin seed settings + hosted seeder
            services.Configure<AdminSettings>(configuration.GetSection(AlMostashar.Infrastructure.Options.AdminSettings.SectionName));
            services.AddScoped<AdminSeedService>();
            services.AddHostedService<AdminSeedHostedService>();

            // Legal AI gateway client
            services.AddMemoryCache();
            services.Configure<LegalAiOptions>(configuration.GetSection(LegalAiOptions.SectionName));
            services.AddHttpClient<ILegalAiClient, LegalAiClient>((serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<LegalAiOptions>>()
                    .Value;

                if (string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    throw new InvalidOperationException("LegalAi:BaseUrl is not configured.");
                }

                client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 120);
            });
            services.AddHostedService<LegalAiWarmupHostedService>();
            services.AddHostedService<MissedCallCleanupService>();
            services.AddHostedService<UnlinkedDocumentCleanupHostedService>();
            services.Configure<DocumentCleanupOptions>(configuration.GetSection(DocumentCleanupOptions.SectionName));

            // Agora real time communication service
            services.AddScoped<ICommunicationService, AgoraService>();

            // JWT Bearer authentication
            var jwtSettings = configuration.GetSection("Jwt");
            var secret = jwtSettings["Secret"]
                ?? throw new InvalidOperationException("JWT Secret is not configured in appsettings.");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };

                });

            var isDevelopmentEnvironment = string.Equals(
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                "Development",
                StringComparison.OrdinalIgnoreCase);
            //if (isDevelopmentEnvironment)
            //{
            //    services.AddSingleton<DevelopmentLocalStorageService>();
            //    services.AddSingleton<IStorageService>(provider =>
            //        provider.GetRequiredService<DevelopmentLocalStorageService>());
            //}
            //else
            //{
                var s3Settings = configuration.GetSection("Storage:S3").Get<S3Settings>();
                if (s3Settings == null)
                    throw new InvalidOperationException("Storage:S3 section is missing or invalid in appsettings.");

                var s3Config = new AmazonS3Config
                {
                    ServiceURL = s3Settings.Endpoint,
                    AuthenticationRegion = s3Settings.Region,
                    ForcePathStyle = true
                };
                var credentials = new Amazon.Runtime.BasicAWSCredentials(
                    s3Settings.AccessKey,
                    s3Settings.SecretKey);
                services.AddSingleton<IAmazonS3>(new AmazonS3Client(credentials, s3Config));
                services.AddSingleton(s3Settings);
                services.AddScoped<IStorageService, S3StorageService>();
            //}

            // Paymob Registration
            services.AddHttpClient();
            services.AddScoped<IPaymentService, PaymobService>();

            // Connection Tracker Registration
            services.AddSingleton<IConnectionTracker, InMemoryConnectionTracker>();

            return services;
        }
    }
}
