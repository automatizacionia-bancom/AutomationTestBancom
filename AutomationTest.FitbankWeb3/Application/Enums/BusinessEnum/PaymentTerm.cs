using System.ComponentModel;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum PaymentTerm
    {
        [Description("30 días")] Monthly,
        [Description("60 días")] Bimonthly,
        [Description("90 días")] TriMonthly,
    }
}
