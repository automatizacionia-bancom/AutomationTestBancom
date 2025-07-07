using System.Data;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels;
using AutomationTest.FitbankWeb3.Application.Services;
using AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    [Trait("Grupo", "CoreTest")]
    public class GetTransactionUsersTest: IClassFixture<TestFixture>
    {
        private readonly ITransactionUsersProvider _usersProvider;
        private readonly ITestOutputHelper _output;

        public GetTransactionUsersTest(TestFixture fixture, ITestOutputHelper output)
        {
            _usersProvider = fixture.ServiceProvider.GetRequiredService<ITransactionUsersProvider>();
            _output = output;
        }
        [Fact]
        public async Task GetTransactionUsersForR062900()
        {
            List<string> users = await _usersProvider.GetUsersForTransactionAsync(TransactionType.T062900);

            foreach (var user in users)
            {
                _output.WriteLine($"Usuario: {user}");
            }

            var selector = new TransactionUsersSelectionService(_usersProvider);

            List<string> recognizedUsers = new List<string> { "NGONZALES","MOLORTEGUIA","EMONTERO"};
            string selectedUser = await selector.SelectOptimalUserAsync(TransactionType.T062900, recognizedUsers);

            _output.WriteLine($"Usuario seleccionado: {selectedUser}");
        }
    }
}
