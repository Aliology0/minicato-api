using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using AlMostashar.Api.Helpers;
using AlMostashar.Api.Middlewares;
using AlMostashar.Api.SignalR;
using AlMostashar.Application.Helpers;
using AlMostashar.Infrastructure.Helpers;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Serilog;

namespace AlMostashar.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Serilog: structured logging to Console + rolling Files ──
            builder.Host.UseSerilog((context, config) => config
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "Logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            );

            builder.Services
                .AddApi()                                        // controllers, swagger, cors, signalr
                .AddApplication()                               // mediatr handlers
                .AddInfrastructure(builder.Configuration);      // db, auth service, jwt

            // Disable Microsoft's default JWT claim type mapping globally
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // ── Localization: Arabic default, English fallback ──
            builder.Services.AddLocalization();
          
            var app = builder.Build();
            app.MapGet("/", () => Results.Redirect("https://minicato-web.vercel.app")).AllowAnonymous();

            // Set Arabic as default culture; Flutter can override via Accept-Language header
            var supportedCultures = new[] { new CultureInfo("ar"), new CultureInfo("en") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ar"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
            });

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseAuthentication();   
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<AlMostasharHub>("/hubs/minicato");

            app.Run();
        }
    }
}
