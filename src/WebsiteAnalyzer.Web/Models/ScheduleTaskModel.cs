using System.ComponentModel.DataAnnotations;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Web.Models
{
    public class ScheduleTaskModel
    {
        public string WebsiteUrl { get; set; } = "https://";

        [Required(ErrorMessage = "Frequency is required")]
        public Frequency Frequency { get; set; }
    }
}