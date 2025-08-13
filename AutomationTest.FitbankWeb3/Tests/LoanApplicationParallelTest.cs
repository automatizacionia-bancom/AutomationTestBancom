using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests
{
    public class LoanApplicationParallelTest : IClassFixture<TestFixture>
    {
        private static List<LoanApplicationWorkflowModel<IClientData>> _cases = null!;
        private static ILoanApplicationExecutor _workflowExecutor;
        static LoanApplicationParallelTest()
        {
            var serviceProvider = TestFixture.Configure();

            _workflowExecutor = serviceProvider.GetRequiredService<ILoanApplicationExecutor>();

            _cases = _workflowExecutor.LoadCases().ToList();
        }
        public static TheoryData<int> GetData()
        {
            var data = new TheoryData<int>();
            for (int i = 0; i < _cases.Count; i++)
                data.Add(i);
            return data;
        }
        public LoanApplicationParallelTest(TestFixture fixture, ITestOutputHelper output)
        {
            var accessor = fixture.ServiceProvider
               .GetRequiredService<ITestOutputAccessor>();
            accessor.Set(output);
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
