using System.ComponentModel.DataAnnotations;
using YT2PP.Web.Attributes;

namespace YT2PP.Web.Models
{

    public class DataInputViewModel
    {
        [Required(ErrorMessage = "YouTube URL is required.")]
        [StringLength(45, ErrorMessage = "YouTube URL cannot be longer than 45 characters.")]
        [YouTubeUrl(ErrorMessage = "Invalid YouTube URL.")]
        public string DataInput { get; set; }
    }

}
