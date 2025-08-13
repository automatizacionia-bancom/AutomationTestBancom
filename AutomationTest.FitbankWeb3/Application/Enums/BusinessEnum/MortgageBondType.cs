using System.ComponentModel;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum MortgageBondType
    {
        [Description("SIN BONO")] SinBono,
        [Description("BBP: BONO BUEN PAGADOR NCMV")] BBP,
        [Description("BMS: BONO MI VIV. SOSTENIBLE")] BMS
    }
}
