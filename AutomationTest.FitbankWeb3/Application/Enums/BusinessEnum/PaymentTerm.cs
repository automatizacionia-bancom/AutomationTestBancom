using System.ComponentModel;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum PaymentTerm
    {
        [Description("30 dias")] Monthly,
        [Description("60 dias")] Bimonthly,
        [Description("90 dias")] TriMonthly,
    }
}
