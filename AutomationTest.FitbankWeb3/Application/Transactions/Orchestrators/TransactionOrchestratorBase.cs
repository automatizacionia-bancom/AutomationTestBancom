using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Orchestrators
{
    public abstract class TransactionOrchestratorBase
    {
        protected readonly IServiceProvider _provider;
        protected readonly PlaywrightFixture _playwright;
        protected readonly LocatorRepositoryFixture _locators;
        protected readonly IPdfConverter _pdfConverter;
        protected readonly IStandardQueryService _standardQueryService;
        protected readonly ITransactionUsersSelectionService _transactionUsersSelectionService;
        protected readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        protected readonly IBranchSynchronizationService _branchSynchronizationService;
        protected readonly IUserTurnCoordinatorService _userTurnCoordinatorService;
        protected readonly ITestOutputAccessor _outputAccessor;
        protected readonly TransactionSettings _transactionSettings;

        protected TransactionOrchestratorBase(
            IServiceProvider provider,
            PlaywrightFixture playwright,
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            ITransactionUsersSelectionService transactionUsersSelectionService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            IBranchSynchronizationService branchSynchronizationService,
            IUserTurnCoordinatorService userTurnCoordinatorService,
            ITestOutputAccessor outputAccessor,
            TransactionSettings transactionSettings)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _playwright = playwright ?? throw new ArgumentNullException(nameof(playwright));
            _locators = locators ?? throw new ArgumentNullException(nameof(locators));
            _pdfConverter = pdfConverter ?? throw new ArgumentNullException(nameof(pdfConverter));
            _standardQueryService = standardQueryService ?? throw new ArgumentNullException(nameof(standardQueryService));
            _transactionUsersSelectionService = transactionUsersSelectionService ?? throw new ArgumentNullException(nameof(transactionUsersSelectionService));
            _actionCoordinatorFactory = actionCoordinatorFactory ?? throw new ArgumentNullException(nameof(actionCoordinatorFactory));
            _branchSynchronizationService = branchSynchronizationService ?? throw new ArgumentNullException(nameof(branchSynchronizationService));
            _userTurnCoordinatorService = userTurnCoordinatorService ?? throw new ArgumentNullException(nameof(userTurnCoordinatorService));
            _outputAccessor = outputAccessor ?? throw new ArgumentNullException(nameof(outputAccessor));
            _transactionSettings = transactionSettings ?? throw new ArgumentNullException(nameof(transactionSettings));
        }

        protected async Task WatcherAsync(IPage page, string locator)
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

        protected async Task<IBrowser> LaunchBrowserAsync<TClientData>(IOrchestratorModel<TClientData> loanRequest) where TClientData : IClientData
        {
            if (_playwright.PlaywrightVar is null)
                throw new InvalidOperationException("Playwright no ha sido inicializado.");

            return await _playwright.PlaywrightVar.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = loanRequest.Headless,
                DownloadsPath = loanRequest.EvidenceFolder,
            });
        }
        protected async Task<ILoanApplicationResult> RunLoanApplicationAsync<TClientData>(
            IPage page, TClientData clientData, string evidenceFolder, string ipPort, bool headless, bool keepPdf) where TClientData : IClientData
        {
            var approvalFlow = _provider.GetRequiredService<ILoanApplication<TClientData>>();

            var model = new LoanApplicationWorkflowModel<TClientData>
            {
                ClientData = clientData,
                EvidenceFolder = evidenceFolder,
                IpPort = ipPort,
                Headless = headless,
                KeepPdf = keepPdf
            };

            return await approvalFlow.ApplyForLoanAsync(page, model);
        }
        protected async Task RunApprovalLoopAsync<TClientData>(
            IPage page,
            string applicationNumber,
            List<string> recognizedUsers,
            string evidenceFolder,
            string ipPort,
            int maxApprovalUser,
            IBranchSession? userTurnSession = null) where TClientData : IClientData
        {
            var approvalFlow = _provider.GetRequiredService<ILoanApproval<TClientData>>();
            TransactionType transactionType = _provider.GetRequiredService<ITransactionDataResolver>().GetTransactionType<TClientData>();

            List<string> recognizedNewUsers = recognizedUsers;

            for (int attempt = 1; attempt <= maxApprovalUser; attempt++)
            {
                string nextUser = await _transactionUsersSelectionService
                    .SelectOptimalUserAsync(transactionType, recognizedNewUsers);

                var model = new LoanApprovalModel
                {
                    ApprovingUser = nextUser,
                    ApprovalNumber = attempt,
                    EvidenceFoler = evidenceFolder,
                    ApplicationNumber = applicationNumber,
                    IpPort = ipPort
                };

                await (userTurnSession?.ArriveUntilTurnAsync(nextUser) ?? Task.CompletedTask);

                // Verificar que el usuario no tenga una sesión activa
                await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
                {
                    User = nextUser
                });

                var approvalResult = await approvalFlow.ApproveLoanAsync(page, model);
                _outputAccessor.Output.WriteLine($"Flujo de aprobación ({attempt}) ejecutado.");

                if (approvalResult.RecognizedApprovingUsers.Count == 0)
                {
                    HandleNoRecognizedUsers(approvalResult);
                    return;
                }

                if (approvalResult.ApprovalStatus == ApprovalStatus.APROBADO)
                    throw new InvalidOperationException("Bucle detectado: préstamo ya aprobado con usuarios pendientes.");

                recognizedNewUsers = approvalResult.RecognizedApprovingUsers;
            }

            throw new InvalidOperationException($"Se alcanzó el máximo ({maxApprovalUser}) de intentos de aprobación.");
        }
        protected void HandleNoRecognizedUsers(LoanApprovalResultModel approvalResult)
        {
            if (approvalResult.ApprovalStatus == ApprovalStatus.APROBADO)
            {
                _outputAccessor.Output.WriteLine("Aprobación completada con éxito.");
            }
            else
            {
                throw new InvalidOperationException(
                    $"No hay usuarios reconocidos. Estado: {approvalResult.ApprovalStatus}");
            }
        }
        protected async Task HandleErrorAsync<TClientData>(Exception ex, IOrchestratorModel<TClientData> loanRequest, IPage page) where TClientData : IClientData
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var screenshotPath = Path.Combine(loanRequest.EvidenceFolder, $"Incidencia_{timestamp}.jpeg");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            Assert.Fail($"{ex.Message}\n{ex.StackTrace}");

            _outputAccessor.Output.WriteLine($"Error en flujo: {ex.Message}, {ex.StackTrace}");
        }
    }
}
