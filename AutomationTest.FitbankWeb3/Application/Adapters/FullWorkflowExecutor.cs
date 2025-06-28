using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
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
        private readonly ITransactionOrchestrator _orchestrator;

        public FullWorkflowExecutor(
            ITestConfigurationProvider config,
            ITestDataProvider dataProvider,
            ITransactionDataResolver resolver,
            ITransactionOrchestrator orchestrator)
        {
            _config = config;
            _dataProvider = dataProvider;
            _resolver = resolver;
            _orchestrator = orchestrator;
        }
        public async Task ExecuteWorkflow(FullLoanRequest<IClientData> fullLoanRequest)
        {
            if(!Directory.Exists(fullLoanRequest.EvidenceFoler))
            {
                Directory.CreateDirectory(fullLoanRequest.EvidenceFoler);
            }

            await ExecuteTypedTransactionAsync(fullLoanRequest);
        }
        // Replaces the previous unnamed method with a descriptive name
        private async Task ExecuteTypedTransactionAsync(FullLoanRequest<IClientData> fullLoanRequest)
        {
            // 2) Descubre el tipo real de TClientData
            var clientData = fullLoanRequest.ClientData!;
            var clientType = clientData.GetType(); // e.g. typeof(ClientDataT062900)

            // 3) Crea un FullLoanRequest<TClientData> dinámicamente
            var fullReqType = typeof(FullLoanRequest<>).MakeGenericType(clientType);
            var typedReq = Activator.CreateInstance(fullReqType)!;

            // 4) Copia cada propiedad de untypedReq a typedReq
            foreach (var prop in fullReqType.GetProperties().Where(p => p.CanWrite))
            {
                // obtiene el valor de la propiedad genérica IClientData
                var value = typeof(FullLoanRequest<IClientData>)
                    .GetProperty(prop.Name)!.GetValue(fullLoanRequest);
                prop.SetValue(typedReq, value);
            }

            // 5) Invoca TransactionAsync<TClientData>(typedReq) por reflexión
            var method = _orchestrator
               .GetType()
               .GetMethod(nameof(ITransactionOrchestrator.TransactionAsync))!
               .MakeGenericMethod(clientType);

            var task = (Task)method.Invoke(_orchestrator, new object[] { typedReq })!;

            // 6) Await al task
            await task;
        }
    }
}
