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
            // 2.a) Cargar configuración de ruta o hardcodear
            string excelPath = @"C:\Users\HASANCHEZ\Desktop\Fitbank RPA\Formato RPA Fitbank WEB3 06 2900.xlsx";
            string sheetName = "QA";

            // 2.b) Crear temporalmente el provider para leer Excel
            var services = new ServiceCollection();
            // sólo necesitas ITestDataProvider e IClientDataAdapter<T>
            services.AddSingleton<ITestDataProvider, SpireTestDataProvider>();
            services.AddTransient<IClientDataAdapter<ClientDataT062900>, ClientDataT062900Adapter>();
            var sp = services.BuildServiceProvider();

            var provider = sp.GetRequiredService<ITestDataProvider>();

            // 2.c) Carga y mapea
            _cases = provider
                .GetTestCases<ClientDataT062900>(excelPath, sheetName)
                .Select(cd => new FullLoanRequest<ClientDataT062900>
                {
                    ClientData = cd,
                    EvidenceFoler = @"C:\Users\HASANCHEZ\Desktop\Fitbank RPA\Evidencias\cajamarca\Caso1",
                    IpPort = "http://10.0.2.54:8380",
                    Headless = true,
                    KeepPdf = false,
                    MaxApprovalUser = 10
                })
                .ToList();
        }
        // 3) Ahora GetData ya encuentra _cases no nulo
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
            _orchestrator = fixture.ServiceProvider
                .GetRequiredService<ITransactionOrchestrator>();
        }
        // 3) Este método debe ser estático para MemberData.

        //[Theory]
        //[MemberData(nameof(GetData))]
        public async Task OrchestratorTest(int clientDataIndex)
        {
            // 4) Recupera el FullLoanRequest usando el índice
            var request = _cases[clientDataIndex];

            // Ejecuta tu orquestador
            await _orchestrator.TransactionAsync(request);

            //Assert.True(result.IsSuccess);
        }
    }
}
