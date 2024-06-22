using Microsoft.AspNetCore.Http;
using System;
using YT2PP.Web.Models;

namespace YT2PP.Web.Middlewares
{
    public class UserDetailsLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public UserDetailsLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
        {
          
            // Check if the request path matches the specific method you want to log
            if (context.Request.Path.Value.Contains("/Home/Extract"))
            {
                var userDetails = new UserDetails
                {
                    UserId = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Anonymous",
                    Browser = context.Request.Headers["User-Agent"].ToString(),
                    OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                    ClickedAt = DateTime.UtcNow.ToString("o"),
                    YoutubeLink =  context.Request.Form["DataInput"]
                };

                dbContext.UserDetails.Add(userDetails);
                await dbContext.SaveChangesAsync();
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
