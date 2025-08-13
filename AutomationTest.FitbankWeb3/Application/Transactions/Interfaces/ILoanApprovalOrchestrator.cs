using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Interfaces
{
    public interface ILoanApprovalOrchestrator
    {
        Task TransactionAsync<TClientData>(LoanApprovalWorkflowModel<TClientData> loanRequest) where TClientData : IClientData;
    }
}