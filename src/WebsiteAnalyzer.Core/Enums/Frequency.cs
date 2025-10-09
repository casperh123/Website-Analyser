using System.ComponentModel;
using System.Reflection;

namespace WebsiteAnalyzer.Core.Enums;

public enum Frequency
{
    [Description("Every 6 Hours")] SixHourly,
    [Description("Every 12 Hours")] TwelveHourly,
    [Description("Daily")] Daily,
    [Description("Weekly")] Weekly
}

public static class FrequencyExtensions
{
    public static string ToFriendlyString(this Frequency frequency)
    {
        FieldInfo? fieldInfo = frequency.GetType().GetField(frequency.ToString());
        DescriptionAttribute[] descriptionAttributes =
            (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return descriptionAttributes.Length > 0
            ? descriptionAttributes[0].Description
            : frequency.ToString();
    }

    public static TimeSpan ToTimeSpan(Frequency frequency)
    {
        return frequency switch
        {
            Frequency.SixHourly => TimeSpan.FromHours(6),
            Frequency.TwelveHourly => TimeSpan.FromHours(12),
            Frequency.Daily => TimeSpan.FromDays(1),
            Frequency.Weekly => TimeSpan.FromDays(7),
            _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, null)
        };
    }
}