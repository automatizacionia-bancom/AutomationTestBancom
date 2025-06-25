using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Application.Models.TransactionModels
{
    public class FullLoanRequest <TClientData> where TClientData : IClientData
    {
        public required TClientData ClientData { get; set; }
        public required string EvidenceFoler { get; set; }
        public required string IpPort { get; set; }
        public required bool Headless { get; set; }
        public required bool KeepPdf { get; set; }
        public required int MaxApprovalUser { get; set; }
    }
}
