using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Inbound
{
    public interface IFullWorkflowExecutor
    {
        Task ExecuteWorkflow(FullLoanRequest<IClientData> fullLoanRequest);
    }
}
