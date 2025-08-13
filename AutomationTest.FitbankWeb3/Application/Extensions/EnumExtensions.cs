using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AutomationTest.FitbankWeb3.Application.Extensions
{
    public static class EnumExtensions
    {
        public static string ToSpacedString(this Enum value)
        {
            return Regex.Replace(value.ToString(), "(\\B[A-Z])", " $1");
        }
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                             .FirstOrDefault() as DescriptionAttribute;
            return attr?.Description ?? value.ToString();
        }
    }
}
