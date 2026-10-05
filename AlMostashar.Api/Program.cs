using AlMostashar.Api.Helpers;
using AlMostashar.Api.Middlewares;
using AlMostashar.Api.SignalR;
using AlMostashar.Application.Helpers;
using AlMostashar.Infrastructure.Helpers;
using System.Globalization;
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
                .AddApi(builder.Configuration)
                .AddApplication()
                .AddInfrastructure(builder.Configuration);

            // Disable Microsoft's default JWT claim type mapping globally
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler
                .DefaultInboundClaimTypeMap.Clear();

            // ── Localization ──
            builder.Services.AddLocalization();

            var app = builder.Build();

            // API health/status endpoint
            app.MapGet("/", () => Results.Ok(new
            {
                message = "AlMostashar API is running",
                status = "OK"
            })).AllowAnonymous();

            // Set Arabic as default culture
            var supportedCultures = new[]
            {
                new CultureInfo("ar"),
                new CultureInfo("en")
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture =
                    new Microsoft.AspNetCore.Localization.RequestCulture("ar"),

                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures,
            });

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowAll");

            // Do not force HTTPS redirection to preserve HTTP frontend development workflow
            // app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<AlMostasharHub>("/hubs/almostashar");

            app.Run();
        }
    }
}