using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Services;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications;
using AutomationTest.FitbankWeb3.Application.Transactions.PersonalBanking;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

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
                        loanRequest.EvidenceFoler,
                        loanRequest.IpPort,
                        loanRequest.MaxApprovalUser);//, userTurnSession);
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
