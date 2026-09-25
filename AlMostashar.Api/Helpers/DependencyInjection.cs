using AlMostashar.Api.Services;
using AlMostashar.Application.Common.Interfaces;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

namespace AlMostashar.Api.Helpers
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApi(this IServiceCollection services)
        {

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AlMostashar API",
                    Version = "v1"
                });

                // Add JWT Bearer Authentication
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter your JWT token like this: **Bearer {your token}**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                options.AddSecurityDefinition("Bearer", securityScheme);

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { securityScheme, Array.Empty<string>() }
                });
            });
            services.AddSignalR(option => option.EnableDetailedErrors = true)
                .AddJsonProtocol(options =>
                {
                    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // ── Notification service (SignalR) ──
            services.AddScoped<INotificationService, SignalRNotificationService>();

            // ── Current User service ────────────
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }
    }
}
