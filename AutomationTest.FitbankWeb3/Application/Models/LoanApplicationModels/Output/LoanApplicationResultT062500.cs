using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output
{
    public class LoanApplicationResultT062500 : ILoanApplicationResult
    {
        public string ApplicationNumber { get; set; } = string.Empty;
        public InstitutionFFAAType InstitutionFFAAType { get; set; } = InstitutionFFAAType.Undefined;
        public EvaluationResult EvaluationResult { get; set; } = EvaluationResult.Failed;
        public List<string> RecognizedApprovingUsers { get; set; } = new List<string>();
    }
}
