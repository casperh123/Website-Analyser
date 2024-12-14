using System.ComponentModel.DataAnnotations;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Web.Models
{
    public class ScheduleTaskModel
    {
        [Required(ErrorMessage = "Website URL is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [RegularExpression(@"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)", ErrorMessage = "URL must start with http:// or https://")]
        public string WebsiteUrl { get; set; } = "https://";

        [Required(ErrorMessage = "Frequency is required")]
        public Frequency Frequency { get; set; }
    }
}