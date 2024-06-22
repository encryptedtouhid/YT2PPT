using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace YT2PP.Web.Models
{
    public class UserDetails
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
        public string ClickedAt { get; set; }
        public string YoutubeLink { get; set; }
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserDetails> UserDetails { get; set; }
    }
}
