using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Configuration;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Orchestrators
{
    public class FullWorkflowOrchestrator : TransactionOrchestratorBase, IFullWorkflowOrchestrator
    {
        public FullWorkflowOrchestrator(
            IServiceProvider provider,
            PlaywrightFixture playwright,
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            ITransactionUsersSelectionService transactionUsersSelectionService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            IBranchSynchronizationService branchSynchronizationService,
            IUserTurnCoordinatorService userTurnCoordinatorService,
            ITestOutputAccessor output,
            TransactionSettings transactionSettings
            ) : base(
                provider,
                playwright,
                locators,
                pdfConverter,
                standardQueryService,
                transactionUsersSelectionService,
                actionCoordinatorFactory,
                branchSynchronizationService,
                userTurnCoordinatorService,
                output,
                transactionSettings
            )
        {
        }
        public async Task TransactionAsync<TClientData>(FullWorkflowModel<TClientData> loanRequest) where TClientData : IClientData
        {
            await using var browser = await LaunchBrowserAsync<TClientData>(loanRequest);
            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { AcceptDownloads = true });
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(_transactionSettings.GeneralTimout);
            page.SetDefaultNavigationTimeout(_transactionSettings.GeneralTimout);

            //using var userTurnSession = _userTurnCoordinatorService.RegisterBranch();

            // Preparamos el watcher (esperará indefinidamente hasta que aparezca)
            IEnumerable<Task> watchers = new List<Task>
            {
                WatcherAsync(page, _locators.LocatorsGeneralDashboard.TransactionNotAllowed),
                WatcherAsync(page, _locators.LocatorsGeneralDashboard.PageErrorMessage)
            };

            try
            {
                var mainFlowTask = Task.Run(async () =>
                {
                    // Liberamos el usuario
                    await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel { User = loanRequest.ClientData.UserRequest });

                    // Ejecutamos el lujo de aprobacion
                    var applicationResult = await RunLoanApplicationAsync<TClientData>(page, loanRequest.ClientData, loanRequest.EvidenceFolder, loanRequest.IpPort, loanRequest.Headless, loanRequest.KeepPdf);

                    _outputAccessor.Output.WriteLine("Flujo de solicitud de préstamo completado con éxito.");

                    await RunApprovalLoopAsync<TClientData>(
                        page,
                        applicationResult.ApplicationNumber,
                        applicationResult.RecognizedApprovingUsers,
                        loanRequest.EvidenceFolder,
                        loanRequest.IpPort,
                        loanRequest.MaxApprovalUser);//, userTurnSession);
                });

                // Combinamos todas las tareas en una sola colección
                IEnumerable<Task> taks = Enumerable.Concat(watchers, new[] { mainFlowTask });

                // Obtenemos la terea que termina primero, ya sea el flujo principal o un watcher
                var winner = await Task.WhenAny(taks);

                if (winner == mainFlowTask)
                {
                    // El flujo principal acabó sin que apareciera
                    await mainFlowTask;
                }
                else
                {
                    // Si el watcher es el que terminó primero, significa que detectó el elemento inesperado
                    //  re-lanza su excepción y muere aquí
                    await winner;
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, loanRequest, page);
            }
        }
    }
}
