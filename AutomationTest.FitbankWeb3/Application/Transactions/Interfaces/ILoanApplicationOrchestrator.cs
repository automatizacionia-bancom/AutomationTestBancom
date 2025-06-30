using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Interfaces
{
    public interface ILoanApplicationOrchestrator
    {
        Task TransactionAsync<TClientData>(LoanApplicationModel<TClientData> loanRequest) where TClientData : IClientData;
    }
}
