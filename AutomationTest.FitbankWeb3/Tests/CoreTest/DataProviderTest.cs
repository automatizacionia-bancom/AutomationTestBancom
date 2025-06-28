using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    [Trait("Grupo", "CoreTest")]
    public class DataProviderTest : IClassFixture<TestFixture>
    {
        private readonly ITestDataProvider _dataProvider;
        private readonly ITestOutputHelper _output;

        public DataProviderTest(TestFixture fixture, ITestOutputHelper output)
        {
            _dataProvider = fixture.ServiceProvider.GetRequiredService<ITestDataProvider>();
            _output = output;
        }

        [Fact]
        public void GetDataTest()
        {
            // Arrange
            string path = @"C:\Users\HASANCHEZ\Desktop\Fitbank RPA\Formato RPA Fitbank WEB3 06 2900.xlsx";
            string sheetName = "QA";

            // Act
            var data = _dataProvider.GetTestCases<ClientDataT062900>(path, sheetName);

            // Assert/Output
            foreach (var item in data)
            {
                var properties = typeof(ClientDataT062900).GetProperties();
                foreach (var prop in properties)
                {
                    var value = prop.GetValue(item);
                    _output.WriteLine($"{prop.Name}: {value}");
                }
                _output.WriteLine("-----");
            }
        }
    }
}
