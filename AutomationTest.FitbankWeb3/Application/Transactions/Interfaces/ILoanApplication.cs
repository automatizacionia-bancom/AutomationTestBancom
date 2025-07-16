using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Interfaces
{
    public interface ILoanApplication<TClientData> where TClientData : IClientData
    {
        Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<TClientData> loanRequest);
    }
}
