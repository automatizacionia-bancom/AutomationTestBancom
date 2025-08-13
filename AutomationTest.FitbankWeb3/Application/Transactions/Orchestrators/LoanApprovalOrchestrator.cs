using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Configuration;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Orchestrators
{
    public class LoanApprovalOrchestrator : TransactionOrchestratorBase, ILoanApprovalOrchestrator
    {
        public LoanApprovalOrchestrator(
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
        public async Task TransactionAsync<TClientData>(LoanApprovalWorkflowModel<TClientData> loanRequest) where TClientData : IClientData
        {
            await using var browser = await LaunchBrowserAsync<TClientData>(loanRequest);
            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { AcceptDownloads = true });
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(_transactionSettings.GeneralTimout);
            page.SetDefaultNavigationTimeout(_transactionSettings.GeneralTimout);

            //using var userTurnSession = _userTurnCoordinatorService.RegisterBranch();
            // Preparamos el watcher (esperará indefinidamente hasta que aparezca)
            var watcherTask = WatcherAsync(page, _locators.LocatorsGeneralDashboard.TransactionNotAllowed);
            try
            {
                var mainFlowTask = Task.Run(async () =>
                {
                    await RunApprovalLoopAsync<TClientData>(
                        page,
                        loanRequest.ApplicationNumber,
                        loanRequest.RecognizedApprovingUsers,
                        loanRequest.EvidenceFolder,
                        loanRequest.IpPort,
                        loanRequest.MaxApprovalUser,
                        loanRequest.Attempt);//, userTurnSession);
                });

                // Obtenemos la terea que termina primero, ya sea el flujo principal o el watcher
                var winner = await Task.WhenAny(mainFlowTask, watcherTask);

                // Si el watcher es el que terminó primero, significa que detectó el elemento inesperado
                if (winner == watcherTask)
                {
                    // El watcher detectó el elemento: re-lanza su excepción y muere aquí
                    await watcherTask;
                }
                else
                {
                    // El flujo principal acabó sin que apareciera → limpiamos el watcher
                    // (no hay forma de cancelar WaitForSelectorAsync, así que lo observamos)
                    _ = watcherTask.ContinueWith(_ => { }, TaskScheduler.Default);
                    await mainFlowTask;
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, loanRequest, page);
            }
        }
    }
}
