using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels
{
    public class ForceLoanApprovalModel : IStandardQueryModel
    {
        public required string ApplicationNumber { get; set; }
        public string? TIPOSOLICITUDCREDITO { get; set; } = null;
    }
}
