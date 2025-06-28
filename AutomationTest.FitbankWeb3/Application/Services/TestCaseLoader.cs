using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AutomationTest.FitbankWeb3.Application.Services
{
    public class TestCaseLoader : ITestCaseLoader
    {
        private readonly ITestConfigurationProvider _config;
        private readonly ITestDataProvider _dataProvider;
        private readonly ITransactionDataResolver _resolver;
        private readonly IServiceProvider _serviceProvider;

        public TestCaseLoader(
            ITestConfigurationProvider config,
            ITestDataProvider dataProvider,
            ITransactionDataResolver resolver,
            IServiceProvider serviceProvider)
        {
            _config = config;
            _dataProvider = dataProvider;
            _resolver = resolver;
            _serviceProvider = serviceProvider;
        }
        public IEnumerable<FullLoanRequest<IClientData>> LoadCases()
        {
            // 1) Determina el tipo de ClientData a usar
            var txType = _config.TransactionType;
            var clientDataType = _resolver.GetDataType<IClientData>(txType);

            // 2) Invoca ITestDataProvider.GetTestCases<TClientData>
            var generic = typeof(ITestDataProvider)
                .GetMethod(nameof(ITestDataProvider.GetTestCases))!
                .MakeGenericMethod(clientDataType);

            var rawList = (IEnumerable)generic.Invoke(
                _dataProvider,
                new object[] { _config.ExcelPath, _config.SheetName }
            )!;

            // 3) Para cada elemento (que es un TClientData),
            //    conviértelo a FullLoanRequest<IClientData>
            var index = 1;
            foreach (var cd in rawList)
            {
                var client = (IClientData)cd;
                string caseFolder = Path.Combine(_config.EvidenceFolderBase, $"Caso {index}");

                yield return new FullLoanRequest<IClientData>
                {
                    ClientData = client,
                    EvidenceFoler = caseFolder,
                    IpPort = _config.IpPort,
                    Headless = _config.Headless,
                    KeepPdf = _config.KeepPdf,
                    MaxApprovalUser = _config.MaxApprovalUser
                };
                index++;
            }
        }
    }
}
