using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;

namespace AutomationTest.FitbankWeb3.Application.Adapters
{
    public class FullWorkflowExecutor : IFullWorkflowExecutor
    {
        private readonly ITestConfigurationProvider _config;
        private readonly ITestDataProvider _dataProvider;
        private readonly ITransactionDataResolver _resolver;
        private readonly IFullWorkflowOrchestrator _orchestrator;
        private readonly ITestOutputAccessor _outputAccessor;

        public FullWorkflowExecutor(
            ITestConfigurationProvider config,
            ITestDataProvider dataProvider,
            ITransactionDataResolver resolver,
            IFullWorkflowOrchestrator orchestrator,
            ITestOutputAccessor outputAccessor)
        {
            _config = config;
            _dataProvider = dataProvider;
            _resolver = resolver;
            _orchestrator = orchestrator;
            _outputAccessor = outputAccessor;
        }
        public async Task ExecuteWorkflow(FullWorkflowModel<IClientData> fullLoanRequest)
        {
            if (!Directory.Exists(fullLoanRequest.EvidenceFolder))
            {
                Directory.CreateDirectory(fullLoanRequest.EvidenceFolder);
            }

            await ExecuteTypedTransactionAsync(fullLoanRequest);
        }
        public IEnumerable<FullWorkflowModel<IClientData>> LoadCases()
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

                yield return new FullWorkflowModel<IClientData>
                {
                    ClientData = client,
                    EvidenceFolder = caseFolder,
                    IpPort = _config.IpPort,
                    Headless = _config.Headless,
                    KeepPdf = _config.KeepPdf,
                    MaxApprovalUser = _config.MaxApprovalUser
                };
                index++;
            }
        }
        private async Task ExecuteTypedTransactionAsync(FullWorkflowModel<IClientData> fullLoanRequest)
        {
            // 2) Descubre el tipo real de TClientData
            var clientData = fullLoanRequest.ClientData!;
            var clientType = clientData.GetType(); // e.g. typeof(ClientDataT062900)

            // 3) Crea un FullLoanRequest<TClientData> dinámicamente
            var fullReqType = typeof(FullWorkflowModel<>).MakeGenericType(clientType);
            var typedReq = Activator.CreateInstance(fullReqType)!;

            // 4) Copia cada propiedad de untypedReq a typedReq
            foreach (var prop in fullReqType.GetProperties().Where(p => p.CanWrite))
            {
                // obtiene el valor de la propiedad genérica IClientData
                var value = typeof(FullWorkflowModel<IClientData>)
                    .GetProperty(prop.Name)!.GetValue(fullLoanRequest);
                prop.SetValue(typedReq, value);
            }

            // 5) Invoca TransactionAsync<TClientData>(typedReq) por reflexión
            var method = _orchestrator
               .GetType()
               .GetMethod(nameof(IFullWorkflowOrchestrator.TransactionAsync))!
               .MakeGenericMethod(clientType);

            var task = (Task)method.Invoke(_orchestrator, new object[] { typedReq })!;

            // 6) Await al task
            await task;
        }
    }
}
