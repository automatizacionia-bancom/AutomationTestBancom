using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Application.Models.Interfaces
{
    public interface ILoanApplicationResult
    {
        string ApplicationNumber { get; }
        EvaluationResult EvaluationResult { get; }
        List<string> RecognizedApprovingUsers { get; }
    }
}
