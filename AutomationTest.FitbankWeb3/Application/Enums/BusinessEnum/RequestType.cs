using System.ComponentModel;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum RequestType
    {
        [Description("EXCEPCION")] Excepcion,
        [Description("INGRESO A RIESGOS")] IngresoARiesgos
    }
}
