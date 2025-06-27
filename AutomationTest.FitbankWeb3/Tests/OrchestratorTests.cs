using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public class OrchestratorTests : IClassFixture<TestFixture>
    {

        // 1) Lista estática de casos ya inicializados
        private static List<FullLoanRequest<ClientDataT062900>> _cases = null!;
        static OrchestratorTests()
        {
            // Obtenemos el ServiceProvider de TestFixture
            var sp = TestFixture.Configure();

            // Traemos los servicios de configuracion y proveedor de datos
            var config = sp.GetRequiredService<ITestConfigurationProvider>();
            var dataProv = sp.GetRequiredService<ITestDataProvider>();

            // Leemos y adaptamos los casos
            _cases = dataProv.GetTestCases<ClientDataT062900>(config.ExcelPath, config.SheetName)
                .Select(cd => new FullLoanRequest<ClientDataT062900>
                {
                    ClientData = cd,
                    EvidenceFoler = Path.Combine(config.EvidenceFolderBase, cd.Identification),
                    IpPort = config.IpPort,
                    Headless = config.Headless,
                    KeepPdf = config.KeepPdf,
                    MaxApprovalUser = config.MaxApprovalUser
                }).ToList();
        }
        public static TheoryData<int> GetData()
        {
            var data = new TheoryData<int>();
            for (int i = 0; i < _cases.Count; i++)
                data.Add(i);
            return data;
        }
        private readonly ITransactionOrchestrator _orchestrator;

        public OrchestratorTests(TestFixture fixture, ITestOutputHelper output)
        {
            var accessor = fixture.ServiceProvider
               .GetRequiredService<ITestOutputAccessor>();
            accessor.Set(output);

            // Resuelve tu orquestador (no genérico) de DI
            _orchestrator = fixture.ServiceProvider.GetRequiredService<ITransactionOrchestrator>();
        }
        //[Theory]
        //[MemberData(nameof(GetData))]
        public async Task OrchestratorTest(int clientDataIndex)
        {
            // Recupera el FullLoanRequest usando el índice
            var request = _cases[clientDataIndex];

            // Ejecuta tu orquestador
            await _orchestrator.TransactionAsync(request);

        }
    }
}
