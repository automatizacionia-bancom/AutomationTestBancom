using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Domain.Models
{
    public class LoanApplicationWorkflowModel<TClientData> : IOrchestratorModel<TClientData> where TClientData : IClientData
    {
        public required TClientData ClientData { get; set; }
        public required string EvidenceFolder { get; set; }
        public required string IpPort { get; set; }
        public required bool Headless { get; set; }
        public required bool KeepPdf { get; set; }
    }
}
