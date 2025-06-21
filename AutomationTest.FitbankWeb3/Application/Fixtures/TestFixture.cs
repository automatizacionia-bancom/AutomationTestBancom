using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Services;
using AutomationTest.FitbankWeb3.Application.Services.ActionCoordination;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalLoan;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApprovals.PersonalLoan;
using AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Configuration;
using AutomationTest.FitbankWeb3.Infrastructure.Persistence;
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
        public IServiceCollection Services { get; }
        public IServiceProvider ServiceProvider { get; private set; }

        public TestFixture()
        {
            // 1) Levanta la configuración
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var providerSettings = new TransactionUserProviderSettings();
            config.GetSection("TransactionUserProvider")
                  .Bind(providerSettings);

            // 2) Crea y llena el ServiceCollection
            Services = new ServiceCollection();

            // Registra la configuración de TransactionUserProviderSettings POCO
            Services.AddSingleton(providerSettings);
            switch (providerSettings.ProviderType)
            {
                case "Json":
                    Services.AddSingleton<ITransactionUsersProvider>(sp =>
                        new JsonFileTransactionUsersProvider(providerSettings.Connection));
                    break;

                // otros casos: “Api”, “InMemory” para tests, etc.
                default:
                    throw new InvalidOperationException(
                        $"ProviderType “{providerSettings.ProviderType}” no soportado.");
            }

            var connectionString = config.GetConnectionString("As400Db2")
                ?? throw new InvalidOperationException("ConnectionString no encontrado");

            // 3) Registra el executor DB genérico
            Services.AddScoped<IGenericQueryExecutor>(_ =>
                new Db2GenericQueryExecutor(connectionString));

            // 4) Registra PlaywrightFixture como singleton:
            //    queremos compartir la misma instancia para toda la colección de tests.
            Services.AddSingleton<PlaywrightFixture>();

            // 5) Registra ElementRepositoryFixture como singleton (si no guarda estado mutable)
            Services.AddSingleton<ElementRepositoryFixture>();

            // 6) Registra las demás dependencias (queries, servicios, etc.)
            Services.AddTransient<IStandardQuery<DeleteUserSesionModel>, DeleteUserSesionQuery>();
            Services.AddTransient<IStandardQuery<ForceLoanApprovalModel>, ForceLoanApprovalQuery>();
            Services.AddTransient<IStandardQuery<ForceLoanRejectionModel>, ForceLoanRejectionQuery>();
            Services.AddScoped<IStandardQueryService, StandardQueryService>();
            Services.AddScoped<ITransactionUsersSelectionService, TransactionUsersSelectionService>();
            Services.AddScoped<IPdfConverter, PdfConverter>();

            Services.AddTransient<IActionCoordinatorService, ActionCoordinatorService>();
            Services.AddSingleton<IActionCoordinatorFactory, ActionCoordinatorFactory>();
            Services.AddSingleton<ITransactionDataResolver, TransactionDataResolver>();

            Services.AddSingleton<IBranchSynchronizationService, DynamicBranchBarrier>();
            Services.AddSingleton<IUserTurnCoordinatorService,  UserTurnCoordinatorService>();
            Services.AddSingleton<ITestOutputAccessor, TestOutputAccessor>();

            Services.AddTransient<ILoanApplication<ClientDataT062900>, LoanApplicationT062900>();
            Services.AddTransient<ILoanApplication<ClientDataT062800>, LoanApplicationT062800>();
            Services.AddTransient<ILoanApplication<ClientDataT062700>, LoanApplicationT062700>();
            Services.AddTransient<ILoanApplication<ClientDataT062500>, LoanApplicationT062500>();
            Services.AddTransient<ILoanApplication<ClientDataT062400>, LoanApplicationT062400>();

            Services.AddTransient<ILoanApproval<ClientDataT062900>, LoanApproval>();
            Services.AddTransient<ILoanApproval<ClientDataT062800>, LoanApproval>();
            Services.AddTransient<ILoanApproval<ClientDataT062700>, LoanApproval>();
            Services.AddTransient<ILoanApproval<ClientDataT062500>, LoanApproval>();
            Services.AddTransient<ILoanApproval<ClientDataT062400>, LoanApproval>();

            // 7) Construye el ServiceProvider
            ServiceProvider = Services.BuildServiceProvider(); 
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
