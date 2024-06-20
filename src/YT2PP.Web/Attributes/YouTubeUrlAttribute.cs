using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace YT2PP.Web.Attributes
{
    public class YouTubeUrlAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("YouTube URL is required.");
            }

            var url = value.ToString();
            var regex = new Regex(@"^(https?\:\/\/)?(www\.youtube\.com|youtu\.?be)\/.+$");

            if (regex.IsMatch(url))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Invalid YouTube URL.");
            }
        }
    }
}
