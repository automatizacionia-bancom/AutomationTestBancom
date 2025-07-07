using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Adapters;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Services;
using AutomationTest.FitbankWeb3.Application.Services.ActionCoordination;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalBanking;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApprovals.PersonalLoan;
using AutomationTest.FitbankWeb3.Application.Transactions.Orchestrators;
using AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.Database;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.DataProvider;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.FileProcessing;
using AutomationTest.FitbankWeb3.Infrastructure.Configuration;
using AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.ClientDataAdapters;
using AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.Interfaces;
using Meziantou.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Fixtures
{
    public class TestFixture : IAsyncLifetime, IDisposable
    {
        //public IServiceCollection services { get; }
        public IServiceProvider ServiceProvider { get; private set; }
        public TestFixture()
        {
            ServiceProvider = Configure();
        }
        public static IServiceProvider Configure()
        {
            // 1) Levanta la configuración
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables(prefix: "MYTEST_")
                .Build();

            var usersProviderSettings = new TransactionUserProviderSettings();
            config.GetSection("TransactionUserProvider")
                  .Bind(usersProviderSettings);

            var transactionSettings = new TransactionSettings();
            config.GetSection("TransactionSettings")
                  .Bind(transactionSettings);

            // 2) Crea y llena el ServiceCollection
            IServiceCollection services = new ServiceCollection();

            // Registra los settings de configuración
            services.AddSingleton<IConfiguration>(config);
            services.AddSingleton<ITestConfigurationProvider, ConfigurationTestProvider>();

            services.AddSingleton(usersProviderSettings);
            services.AddSingleton(transactionSettings);
            switch (usersProviderSettings.ProviderType)
            {
                case "Json":
                    services.AddSingleton<ITransactionUsersProvider>(sp =>
                        new JsonFileTransactionUsersProvider(usersProviderSettings.Connection));
                    break;

                // otros casos: “Api”, “InMemory” para tests, etc.
                default:
                    throw new InvalidOperationException(
                        $"ProviderType “{usersProviderSettings.ProviderType}” no soportado.");
            }

            var connectionString = config.GetConnectionString("As400Db2")
                ?? throw new InvalidOperationException("ConnectionString no encontrado");

            // 3) Registra el executor DB genérico
            services.AddScoped<IGenericQueryExecutor>(_ =>
                new Db2GenericQueryExecutor(connectionString));

            // 4) Registra PlaywrightFixture como singleton:
            //    queremos compartir la misma instancia para toda la colección de tests.
            services.AddSingleton<PlaywrightFixture>();

            // 5) Registra ElementRepositoryFixture como singleton (si no guarda estado mutable)
            services.AddSingleton<ElementRepositoryFixture>();

            // 6) Registra las demás dependencias (queries, servicios, etc.)
            services.AddTransient<IStandardQuery<DeleteUserSesionModel>, DeleteUserSesionQuery>();
            services.AddTransient<IStandardQuery<ForceLoanApprovalModel>, ForceLoanApprovalQuery>();
            services.AddTransient<IStandardQuery<ForceOnlyCarsEssentialModel>, ForceOnlyCarsEssential>();
            services.AddScoped<IStandardQueryService, StandardQueryService>();
            services.AddScoped<ITransactionUsersSelectionService, TransactionUsersSelectionService>();
            services.AddScoped<IPdfConverter, CrossPlatformPdfConverter>();
            services.AddSingleton<ITestDataProvider, SpireTestDataProvider>();

            services.AddSingleton<IFullWorkflowExecutor, FullWorkflowExecutor>();
            services.AddSingleton<ILoanApplicationExecutor, LoanApplicationExecutor>();

            services.AddTransient<IActionCoordinatorService, ActionCoordinatorService>();
            services.AddSingleton<IActionCoordinatorFactory, ActionCoordinatorFactory>();
            services.AddSingleton<ITransactionDataResolver, TransactionDataResolver>();

            services.AddSingleton<IBranchSynchronizationService, DynamicBranchBarrier>();
            services.AddSingleton<IUserTurnCoordinatorService, UserTurnCoordinatorService>();
            services.AddSingleton<ITestOutputAccessor, TestOutputAccessor>();

            services.AddSingleton<IFullWorkflowOrchestrator, FullWorkflowOrchestrator>();
            services.AddSingleton<ILoanApplicationOrchestrator, LoanApplicationOrchestrator>();

            services.AddTransient<ILoanApplication<ClientDataT062900>, LoanApplicationT062900>();
            services.AddTransient<ILoanApplication<ClientDataT062800>, LoanApplicationT062800>();
            services.AddTransient<ILoanApplication<ClientDataT062700>, LoanApplicationT062700>();
            services.AddTransient<ILoanApplication<ClientDataT062500>, LoanApplicationT062500>();
            services.AddTransient<ILoanApplication<ClientDataT062400>, LoanApplicationT062400>();

            services.AddTransient<ILoanApproval<ClientDataT062900>, LoanApproval>();
            services.AddTransient<ILoanApproval<ClientDataT062800>, LoanApproval>();
            services.AddTransient<ILoanApproval<ClientDataT062700>, LoanApproval>();
            services.AddTransient<ILoanApproval<ClientDataT062500>, LoanApproval>();
            services.AddTransient<ILoanApproval<ClientDataT062400>, LoanApproval>();

            // Adaptadores por cada TClientData
            services.AddTransient<IClientDataAdapter<ClientDataT062900>, ClientDataT062900Adapter>();
            services.AddTransient<IClientDataAdapter<ClientDataT062800>, ClientDataT062800Adapter>();
            services.AddTransient<IClientDataAdapter<ClientDataT062700>, ClientDataT062700Adapter>();
            services.AddTransient<IClientDataAdapter<ClientDataT062500>, ClientDataT062500Adapter>();
            services.AddTransient<IClientDataAdapter<ClientDataT062400>, ClientDataT062400Adapter>();

            // 7) Construye el ServiceProvider
            var ServiceProvider = services.BuildServiceProvider();

            // *** Nuevo bloque: inicializamos PlaywrightFixture ahora mismo ***
            var pw = ServiceProvider.GetRequiredService<PlaywrightFixture>();
            // Como estamos en contexto sincrónico:
            pw.InitializeAsync().GetAwaiter().GetResult();

            return ServiceProvider;
        }

        /// <summary>
        /// Este método será llamado por xUnit **antes** de ejecutar cualquier test en la colección.
        /// Aquí invocamos InitializeAsync de PlaywrightFixture.
        /// </summary>
        public async Task InitializeAsync()
        {
            // Recuperamos la instancia compartida de PlaywrightFixture
            var playwrightFixture = ServiceProvider.GetRequiredService<PlaywrightFixture>();
            await playwrightFixture.InitializeAsync();
        }

        ///// <summary>
        ///// Este método será llamado por xUnit **después** de que terminen todos los tests de la colección.
        ///// Aquí invocamos DisposeAsync de PlaywrightFixture para cerrar el navegador.
        ///// </summary>
        public async Task DisposeAsync()
        {
            var playwrightFixture = ServiceProvider.GetService<PlaywrightFixture>();
            if (playwrightFixture != null)
                await playwrightFixture.DisposeAsync();
        }

        ///// <summary>
        ///// Liberamos el ServiceProvider y cualquier otro IDisposable.
        ///// </summary>
        public void Dispose()
        {
            if (ServiceProvider is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
