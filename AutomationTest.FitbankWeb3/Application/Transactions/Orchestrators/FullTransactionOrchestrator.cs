using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Application.Services;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApprovals;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Orchestrators
{
    public class FullTransactionOrchestrator : ITransactionOrchestrator
    {
        private readonly IServiceProvider _provider;
        private readonly PlaywrightFixture _playwright;
        private readonly ElementRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly ITransactionUsersSelectionService _transactionUsersSelectionService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly IBranchSynchronizationService _branchSynchronizationService;
        private readonly IUserTurnCoordinatorService _userTurnCoordinatorService;
        private readonly ITestOutputAccessor _outputAccessor;
        private readonly TransactionSettings _transactionSettings;

        public FullTransactionOrchestrator(
            IServiceProvider provider,
            PlaywrightFixture playwright,
            ElementRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            ITransactionUsersSelectionService transactionUsersSelectionService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            IBranchSynchronizationService branchSynchronizationService,
            IUserTurnCoordinatorService userTurnCoordinatorService,
            ITestOutputAccessor output,
            TransactionSettings transactionSettings)
        {
            _provider = provider;
            _playwright = playwright;
            _locators = locators;
            _pdfConverter = pdfConverter;
            _standardQueryService = standardQueryService;
            _transactionUsersSelectionService = transactionUsersSelectionService;
            _actionCoordinatorFactory = actionCoordinatorFactory;
            _branchSynchronizationService = branchSynchronizationService;
            _userTurnCoordinatorService = userTurnCoordinatorService;
            _outputAccessor = output;
            _transactionSettings = transactionSettings;
        }
        public async Task TransactionAsync<TClientData>(FullLoanRequest<TClientData> loanRequest)
            where TClientData : IClientData
        {
            await using var browser = await LaunchBrowserAsync<TClientData>(loanRequest);
            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { AcceptDownloads = true });
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(_transactionSettings.GeneralTimout);
            page.SetDefaultNavigationTimeout(_transactionSettings.GeneralTimout);

            TransactionType transactionType = _provider.GetRequiredService<ITransactionDataResolver>().GetTransactionType<TClientData>();

            //using var userTurnSession = _userTurnCoordinatorService.RegisterBranch();
            // Preparamos el watcher (esperará indefinidamente hasta que aparezca)
            var watcherTask = WatcherAsync(page, _locators.DashboardPage.TransactionNotAllowed);
            try
            {
                // Verificar que el usuario no tenga una sesión activa
                var mainFlowTask = Task.Run(async () =>
                {
                    await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
                    {
                        User = loanRequest.ClientData.UserRequest
                    });

                    var applicationResult = await RunLoanApplicationAsync<TClientData>(page, loanRequest);
                    _outputAccessor.Output.WriteLine("Flujo de solicitud de préstamo completado con éxito.");

                    await RunApprovalLoopAsync<TClientData>(page, loanRequest, applicationResult, transactionType);//, userTurnSession);
                });

                // Race: cualquiera que termine primero…
                var winner = await Task.WhenAny(mainFlowTask, watcherTask);

                if (winner == watcherTask)
                {
                    // El watcher detectó el elemento: re-lanza su excepción y muere aquí
                    await watcherTask;
                }
                else
                {
                    // El flujo principal acabó sin que apareciera → limpias el watcher
                    // (no hay forma de cancelar WaitForSelectorAsync, así que al menos lo observas)
                    _ = watcherTask.ContinueWith(_ => { }, TaskScheduler.Default);
                    await mainFlowTask;
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, loanRequest, page);
            }
        }
        async Task WatcherAsync(IPage page, string locator)
        {
            // Espera sin límite a que aparezca
            await page.Locator(locator).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 0
            });
            // En cuanto aparezca: excepción
            throw new Exception($"¡Elemento inesperado detectado: {locator}!");
        }
        private async Task<IBrowser> LaunchBrowserAsync<TClientData>(FullLoanRequest<TClientData> loanRequest) where TClientData : IClientData =>
            await _playwright.PlaywrightVar.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = loanRequest.Headless,
                DownloadsPath = loanRequest.EvidenceFoler,
            });
        private async Task<ILoanApplicationResult> RunLoanApplicationAsync<TClientData>(
            IPage page,
            FullLoanRequest<TClientData> loanRequest)
            where TClientData : IClientData
        {
            var approvalFlow = _provider.GetRequiredService<ILoanApplication<TClientData>>();

            var model = new LoanApplicationModel<TClientData>
            {
                ClientData = loanRequest.ClientData,
                EvidenceFoler = loanRequest.EvidenceFoler,
                IpPort = loanRequest.IpPort,
                Headless = loanRequest.Headless,
                KeepPdf = loanRequest.KeepPdf
            };

            return await approvalFlow.ApplyForLoanAsync(page, model);
        }
        private async Task RunApprovalLoopAsync<TClientData>(
            IPage page,
            FullLoanRequest<TClientData> loanRequest,
            ILoanApplicationResult result, TransactionType transactionType, IBranchSession? userTurnSession = null)
            where TClientData : IClientData
        {
            var approvalFlow = _provider.GetRequiredService<ILoanApproval<TClientData>>();

            List<string> recognizedUser = result.RecognizedApprovingUsers;

            for (int attempt = 1; attempt <= loanRequest.MaxApprovalUser; attempt++)
            {
                string nextUser = await _transactionUsersSelectionService
                    .SelectOptimalUserAsync(transactionType, recognizedUser);

                var model = new LoanApprovalModel
                {
                    ApprovingUser = nextUser,
                    ApprovalNumber = attempt,
                    EvidenceFoler = loanRequest.EvidenceFoler,
                    ApplicationNumber = result.ApplicationNumber,
                    IpPort = loanRequest.IpPort
                };

                await (userTurnSession?.ArriveUntilTurnAsync(nextUser) ?? Task.CompletedTask);

                // Verificar que el usuario no tenga una sesión activa
                await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
                {
                    User = loanRequest.ClientData.UserRequest
                });

                var approvalResult = await approvalFlow.ApproveLoanAsync(page, model);
                _outputAccessor.Output.WriteLine("Flujo de aprobación ejecutado.");

                if (approvalResult.RecognizedApprovingUsers.Count == 0)
                {
                    HandleNoRecognizedUsers(approvalResult);
                    return;
                }

                if (approvalResult.ApprovalStatus == ApprovalStatus.APROBADO)
                    throw new InvalidOperationException("Bucle detectado: préstamo ya aprobado con usuarios pendientes.");

                recognizedUser = approvalResult.RecognizedApprovingUsers;
            }

            throw new InvalidOperationException($"Se alcanzó el máximo ({loanRequest.MaxApprovalUser}) de intentos de aprobación.");
        }
        private void HandleNoRecognizedUsers(LoanApprovalResultModel res)
        {
            if (res.ApprovalStatus == ApprovalStatus.APROBADO)
            {
                _outputAccessor.Output.WriteLine("Aprobación completada con éxito.");
            }
            else
            {
                throw new InvalidOperationException(
                    $"No hay usuarios reconocidos. Estado: {res.ApprovalStatus}");
            }
        }
        private async Task HandleErrorAsync<TClientData>(Exception ex, FullLoanRequest<TClientData> loanRequest, IPage page) where TClientData : IClientData
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var screenshotPath = Path.Combine(loanRequest.EvidenceFoler, $"Incidencia_{timestamp}.jpg");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            _outputAccessor.Output.WriteLine($"Error en flujo: {ex.Message}, {ex.StackTrace}");
            Assert.Fail($"Error en flujo: {ex.Message}");
        }
    }
}
