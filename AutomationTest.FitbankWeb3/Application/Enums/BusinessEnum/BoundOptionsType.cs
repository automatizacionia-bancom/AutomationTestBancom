using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum BoundOptionsType
    {
        [Description("Unspecified")] Unspecified,
        [Description("Menores Ingresos")] MenoresIngresos,
        [Description("Adultos Mayores")] AdultosMayores,
        [Description("Con Discapacidad")] ConDiscapacidad,
        [Description("Desplazados")] Desplazados,
        [Description("Migrantes retornados")] MigrantesRetornados,
    }
}
