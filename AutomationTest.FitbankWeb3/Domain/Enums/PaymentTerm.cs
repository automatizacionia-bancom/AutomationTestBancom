using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Domain.Enums
{
    public enum PaymentTerm
    {
        [Description("30 días")] Monthly,
        [Description("60 días")] Bimonthly,
        [Description("90 días")] TriMonthly,
    }
}
