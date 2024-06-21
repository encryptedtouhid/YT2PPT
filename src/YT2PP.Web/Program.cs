using Microsoft.AspNetCore.Mvc;
using YT2PP.Models;
using YT2PP.Services.Implementations;
using YT2PP.Services.Interfaces;
using NToastNotify;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;


namespace YT2PP.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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
           

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseNToastNotify();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
