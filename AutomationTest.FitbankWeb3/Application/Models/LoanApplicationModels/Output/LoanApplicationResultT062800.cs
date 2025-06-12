using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output
{
    public class LoanApplicationResultT062800 : ILoanApplicationResult
    {
        public string ApplicationNumber { get; set; } = string.Empty;
        public Transacion062800Type Transacion062800Type { get; set; } = Transacion062800Type.Unspecified;
        public EvaluationResult EvaluationResult { get; set; } = EvaluationResult.Failed;
        public List<string> RecognizedApprovingUsers { get; set; } = new List<string>();
    }
}
