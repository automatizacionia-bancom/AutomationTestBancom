using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Domain.Attributes;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    [Trait("Grupo", "CoreTest")]
    public class GetClientDataTypeTest : IClassFixture<TestFixture>
    {
        private readonly ITransactionDataResolver _transactionDataResolver;
        private readonly ITestConfigurationProvider _config;
        public GetClientDataTypeTest(TestFixture fixture)
        {
            _transactionDataResolver = fixture.ServiceProvider.GetRequiredService<ITransactionDataResolver>();
            _config = fixture.ServiceProvider.GetRequiredService<ITestConfigurationProvider>();
        }
        [Fact]
        public void TestTransactionDataResolver()
        {
            // 1) Obtiene el tipo de ClientData para PersonalBanking
            var txType = _config.TransactionType;
            var clientDataType = _transactionDataResolver.GetDataType<IClientData>(txType);

            // 2) Verifica que el tipo no sea nulo
            Assert.NotNull(clientDataType);

            // 3) Verifica que el tipo implementa IClientData
            Assert.True(typeof(IClientData).IsAssignableFrom(clientDataType), $"El tipo {clientDataType.FullName} no implementa IClientData");

            // 4) Opcional: Verifica que el tipo tiene un atributo TransactionTypeAttribute con el valor correcto
            var attr = clientDataType.GetCustomAttributes(typeof(TransactionTypeAttribute), false).FirstOrDefault() as TransactionTypeAttribute;
            Assert.NotNull(attr);
            Assert.Equal(txType, attr.Type);
        }
    }
}
