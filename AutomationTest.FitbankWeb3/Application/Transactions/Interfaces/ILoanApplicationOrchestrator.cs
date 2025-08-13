using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Interfaces
{
    public interface ILoanApplicationOrchestrator
    {
        Task TransactionAsync<TClientData>(LoanApplicationWorkflowModel<TClientData> loanRequest) where TClientData : IClientData;
    }
}
