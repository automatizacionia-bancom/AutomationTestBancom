using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;
using Spire.Presentation;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    [Trait("Grupo", "CoreTest")]
    public class DataProviderTest : IClassFixture<TestFixture>
    {
        private readonly ITestDataProvider _dataProvider;
        private readonly ITestOutputHelper _output;
        private readonly ITestConfigurationProvider _config;

        public DataProviderTest(TestFixture fixture, ITestOutputHelper output)
        {
            _dataProvider = fixture.ServiceProvider.GetRequiredService<ITestDataProvider>();
            _output = output;
            _config = fixture.ServiceProvider.GetRequiredService<ITestConfigurationProvider>();
        }

        [Fact]
        public void GetDataTest()
        {
            // Act
            var data = _dataProvider.GetTestCases<ClientDataT062400>(_config.ExcelPath, _config.SheetName);

            // Assert/Output
            foreach (var item in data)
            {
                var properties = typeof(ClientDataT062400).GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    _output.WriteLine($"{prop.Name}: {value}");
                }
                _output.WriteLine("-----");
            }

            foreach (int caseTest in _config.TestCaseList)
            {
                _output.WriteLine($"Caso de prueba: {caseTest}");
            }
        }
    }
}
