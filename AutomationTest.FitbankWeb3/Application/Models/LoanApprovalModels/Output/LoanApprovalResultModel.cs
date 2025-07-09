using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;

namespace AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Output
{
    public class LoanApprovalResultModel
    {
        public required List<string> RecognizedApprovingUsers { get; set; } = new List<string>();
        public required ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Failed;    
    }
}
