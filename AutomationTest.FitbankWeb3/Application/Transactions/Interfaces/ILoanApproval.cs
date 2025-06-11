using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Output;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Interfaces
{
    public interface ILoanApproval
    {
        Task<LoanApprovalResultModel> ApproveLoanAsync(IPage page, LoanApprovalModel loanAppproval);
    }
}
