using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum MortgageProjectType
    {
        [Description("PROYECTO PROPIO")] ProyectoPropio,
        [Description("PROYECTO EXTERNO")]  ProyectoExterno
    }
}
