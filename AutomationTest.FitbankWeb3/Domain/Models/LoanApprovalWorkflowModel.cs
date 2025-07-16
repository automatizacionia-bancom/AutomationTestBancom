using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Domain.Models
{
    public class LoanApprovalWorkflowModel<TClientData> : IOrchestratorModel<TClientData> where TClientData : IClientData
    {
        public required string ApplicationNumber { get; set; }
        public required List<string> RecognizedApprovingUsers { get; set; }
        public required string EvidenceFolder { get; set; }
        public required int Attempt { get; set; }
        public required string IpPort { get; set; }
        public required bool Headless { get; set; }
        public required int MaxApprovalUser { get; set; }
    }
}
