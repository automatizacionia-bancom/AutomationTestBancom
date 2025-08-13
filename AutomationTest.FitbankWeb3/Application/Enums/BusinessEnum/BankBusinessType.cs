using System.ComponentModel;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum BankBusinessType
    {
        [Description("Prop. Mayor")] PropMayor,
        [Description("Prop. Menor")] PropMenor,
        [Description("Garantía Líquida")] GarantiaLiquida,
        [Description("Mid Market Regular")] MidMarketRegular,
        Default
    }
}
