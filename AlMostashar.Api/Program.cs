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

            // Global diagnostics for unexpected crashes and unobserved task exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                try
                {
                    var exObj = eventArgs.ExceptionObject as Exception;
                    Serilog.Log.Fatal(exObj, "Unhandled exception (AppDomain.CurrentDomain.UnhandledException). IsTerminating={IsTerminating}", eventArgs.IsTerminating);
                }
                catch { /* swallow to avoid secondary failures during fatal logging */ }
            };

            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                try
                {
                    Serilog.Log.Error(eventArgs.Exception, "UnobservedTaskException: an unobserved task exception occurred.");
                    eventArgs.SetObserved();
                }
                catch { }
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                try
                {
                    Serilog.Log.Information("ProcessExit called. Process is exiting.");
                    Serilog.Log.CloseAndFlush();
                }
                catch { }
            };

            // API health/status endpoint
            app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

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

            // Serve static files from wwwroot and ensure uploads/avatars is available
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<AlMostasharHub>("/hubs/almostashar");

            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                // Final catch-all to ensure fatal startup exceptions are recorded
                Serilog.Log.Fatal(ex, "Host terminated unexpectedly.");
                throw;
            }
        }
    }
}