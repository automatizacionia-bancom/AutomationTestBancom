using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Attributes;

namespace AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum
{
    public enum CoinType
    {
        [Description("S/.")] Soles = 0,
        [Description("USD")]  Dolares = 1,
    }
}
