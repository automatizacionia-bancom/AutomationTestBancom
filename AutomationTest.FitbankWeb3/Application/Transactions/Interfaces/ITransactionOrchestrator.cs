using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Interfaces
{
    public interface ITransactionOrchestrator
    {
        Task TransactionAsync<TClientData>(FullLoanRequest<TClientData> loanRequest) where TClientData : IClientData;
    }
}
