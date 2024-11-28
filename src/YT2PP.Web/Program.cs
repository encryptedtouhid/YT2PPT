using ElectronNET.API.Entities;
using ElectronNET.API;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Principal;
using Serilog.Exceptions;
using YT2PP.Services.Interfaces;
using YT2PP.Services.Implementations;
using YT2PP.Models; // Add this namespace

namespace YT2PP.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Check for administrator privileges
           
            var builder = WebApplication.CreateBuilder(args);

            // Configure data protection
            builder.Services.AddDataProtection()
                .SetDefaultKeyLifetime(TimeSpan.FromDays(90)) // Set the key lifetime here
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "Keys"))); // Optional: Persist keys to a file system

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();
            // Getting Configuration data from appsetting.json
            builder.Services.Configure<AppSettings>(builder.Configuration);

            builder.Services.AddScoped<IYTService, YTService>();
            builder.Services.AddScoped<IPPTService, PPTService>();
            builder.Services.AddMvc().AddNToastNotifyToastr();
            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            builder.WebHost.UseElectron(args);
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseStaticFiles();
            app.UseDefaultFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseNToastNotify();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            if (HybridSupport.IsElectronActive)
            {
                CreateElectronWindow();
            }
            app.Run();

            async void CreateElectronWindow()
            {
                if (!IsAdministrator())
                {
                    // Display Electron dialog box
                    await Electron.Dialog.ShowMessageBoxAsync(new MessageBoxOptions("The application requires administrative privileges. Please restart as an administrator.")
                    {
                        Type = MessageBoxType.error
                    });
                    Electron.App.Quit();
                    return;
                }

                // Get the client's primary screen dimensions
                var screen = await Electron.Screen.GetPrimaryDisplayAsync();
                var screenSize = screen.Size;

                // Customize the window options based on screen size
                var options = new BrowserWindowOptions
                {
                    Width = screenSize.Width,
                    Height = screenSize.Height,
                    Icon = Path.Combine(AppContext.BaseDirectory, "wwwroot/img/yt2ppt.ico"),
                    WebPreferences = new WebPreferences
                    {
                        NodeIntegration = false,
                        ContextIsolation = true
                    }
                };

                // Create the Electron window with dynamic size
                var window = await Electron.WindowManager.CreateWindowAsync(options);
                window.OnClosed += () => Electron.App.Quit();             }
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
