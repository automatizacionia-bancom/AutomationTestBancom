using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AutomationTest.FitbankWeb3.Application.Adapters
{
    public class TestCaseLoader
    {
        private readonly IServiceProvider _serviceProvider;
        public TestCaseLoader(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public List<FullLoanRequest<TClientData>> LoadCases<TClientData>() where TClientData : IClientData
        {
            // 2) Resuelvo lo que necesito
            var cfg = _serviceProvider.GetRequiredService<ITestConfigurationProvider>();
            var dataProv = _serviceProvider.GetRequiredService<ITestDataProvider>();
            var transactionResolver = _serviceProvider.GetRequiredService<ITransactionDataResolver>();

            transactionResolver.GetDataType<IClientData>(cfg.Transaction);

            // 3) Leo y adapto
            var clientDatas = dataProv
                .GetTestCases<TClientData>(cfg.ExcelPath, cfg.SheetName);

            return clientDatas
                .Select(cd => new FullLoanRequest<TClientData>
                {
                    ClientData = cd,
                    EvidenceFoler = Path.Combine(cfg.EvidenceFolderBase, cd.Identification),
                    IpPort = cfg.IpPort,
                    Headless = cfg.Headless,
                    KeepPdf = cfg.KeepPdf,
                    MaxApprovalUser = cfg.MaxApprovalUser
                })
                .ToList();
        }
    }
}
