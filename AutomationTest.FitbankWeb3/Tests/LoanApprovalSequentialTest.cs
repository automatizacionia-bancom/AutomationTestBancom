using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests
{
    public class LoanApprovalSequentialTest : IClassFixture<TestFixture>
    {
        private static List<LoanApprovalWorkflowModel<IClientData>> _cases = null!;
        private static ILoanApprovalExecutor _workflowExecutor;
        private readonly ITestOutputAccessor _outputAccessor;
        static LoanApprovalSequentialTest()
        {
            var serviceProvider = TestFixture.Configure();

            _workflowExecutor = serviceProvider.GetRequiredService<ILoanApprovalExecutor>();

            _cases = _workflowExecutor.LoadCases().ToList();
        }
        public static TheoryData<int> GetData()
        {
            var data = new TheoryData<int>();
            for (int i = 0; i < _cases.Count; i++)
                data.Add(i);
            return data;
        }
        public LoanApprovalSequentialTest(TestFixture fixture, ITestOutputHelper output)
        {
            var accessor = fixture.ServiceProvider
               .GetRequiredService<ITestOutputAccessor>();
            accessor.Set(output);
            _outputAccessor = accessor;
        }
        [Theory]
        [MemberData(nameof(GetData), DisableDiscoveryEnumeration = true)]
        public async Task OrchestratorTest(int clientDataIndex)
        {
            // 1) Recupera el request “homogéneo”
            var untypedReq = _cases[clientDataIndex]; // es FullLoanRequest<IClientData>

            await _workflowExecutor.ExecuteWorkflow(untypedReq);
        }
    }
}
