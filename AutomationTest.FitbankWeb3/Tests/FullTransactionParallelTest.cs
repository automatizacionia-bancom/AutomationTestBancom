using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.Orchestrators;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.ClientDataAdapters;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.Interfaces;
using AutomationTest.FitbankWeb3.Infrastructure.Persistence;
using HarfBuzzSharp;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests
{
    public class FullTransactionParallelTest : IClassFixture<TestFixture>
    {
        private static List<FullLoanRequest<IClientData>> _cases = null!;
        static FullTransactionParallelTest()
        {
            var serviceProvider = TestFixture.Configure();

            var loader = serviceProvider.GetRequiredService<ITestCaseLoader>();
            _cases = loader.LoadCases().ToList();
        }
        public static TheoryData<int> GetData()
        {
            var data = new TheoryData<int>();
            for (int i = 0; i < _cases.Count; i++)
                data.Add(i);
            return data;
        }

        private readonly IFullWorkflowExecutor _workflowExecutor;
        public FullTransactionParallelTest(TestFixture fixture, ITestOutputHelper output)
        {
            var accessor = fixture.ServiceProvider
               .GetRequiredService<ITestOutputAccessor>();
            accessor.Set(output);

            // Resuelve tu orquestador de DI
            _workflowExecutor = fixture.ServiceProvider.GetRequiredService<IFullWorkflowExecutor>();
        }
        [Theory]
        [MemberData(nameof(GetData))]
        public async Task OrchestratorTest(int clientDataIndex)
        {
            // 1) Recupera el request “homogéneo”
            var untypedReq = _cases[clientDataIndex]; // es FullLoanRequest<IClientData>

            await _workflowExecutor.ExecuteWorkflow(untypedReq);
        }
    }
}
