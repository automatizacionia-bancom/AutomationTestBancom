using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.OrchestratorsModels
{
    public class ApprovalRequestModel<TClientData> where TClientData : IClientData
    {
        public required string ApplicationNumber { get; set; }
        public required List<string> RecognizedApprovingUsers { get; set; }
        public required string EvidenceFoler { get; set; }
        public required int Attempt { get; set; }
        public required string IpPort { get; set; }
        public required bool Headless { get; set; }
        public required int MaxApprovalUser { get; set; }
    }
}
