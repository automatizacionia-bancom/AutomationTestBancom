using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
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
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Application.Services;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApprovals;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Orchestrators
{
    public class TransactionOrchestrator
    {
        private readonly IServiceProvider _provider;
        private readonly PlaywrightFixture _playwright;
        private readonly ElementRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly ITransactionUsersSelectionService _transactionUsersSelectionService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly IBranchSynchronizationService _branchSynchronizationService;
        private readonly ITestOutputHelper _output;
        private readonly string BranchId = Guid.NewGuid().ToString();

        public TransactionOrchestrator(
            IServiceProvider provider,
            PlaywrightFixture playwright,
            ElementRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            ITransactionUsersSelectionService transactionUsersSelectionService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            IBranchSynchronizationService branchSynchronizationService,
            ITestOutputHelper output)
        {
            _provider = provider;
            _playwright = playwright;
            _locators = locators;
            _pdfConverter = pdfConverter;
            _standardQueryService = standardQueryService;
            _transactionUsersSelectionService = transactionUsersSelectionService;
            _actionCoordinatorFactory = actionCoordinatorFactory;
            _branchSynchronizationService = branchSynchronizationService;
            _output = output;
        }
        public async Task TransactionAsync<TClientData>(FullLoanRequest<TClientData> loanRequest)
            where TClientData : IClientData
        {
            await using var browser = await LaunchBrowserAsync<TClientData>(loanRequest);
            await using var context = await browser.NewContextAsync(new BrowserNewContextOptions { AcceptDownloads = true });
            var page = await context.NewPageAsync();

            TransactionType transactionType = _provider.GetRequiredService<ITransactionDataResolver>().GetTransactionType<TClientData>();

            using var registration = _branchSynchronizationService.RegisterBranch(BranchId);
            try
            {
                var applicationResult = await RunLoanApplicationAsync<TClientData>(page, loanRequest);
                _output.WriteLine("Flujo de solicitud de préstamo completado con éxito.");

                await _branchSynchronizationService.ArriveAndWaitAsync(BranchId);

                await RunApprovalLoopAsync<TClientData>(page, loanRequest, applicationResult, transactionType);
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(ex, loanRequest, page);
            }
        }
        private async Task<IBrowser> LaunchBrowserAsync<TClientData>(FullLoanRequest<TClientData> req) where TClientData : IClientData =>
            await _playwright.PlaywrightVar.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = req.Headless,
                DownloadsPath = req.EvidenceFoler
            });
        private async Task<ILoanApplicationResult> RunLoanApplicationAsync<TClientData>(
            IPage page,
            FullLoanRequest<TClientData> req)
            where TClientData : IClientData
        {
            var approvalFlow = _provider.GetRequiredService<ILoanApplication<TClientData>>();

            var model = new LoanApplicationModel<TClientData>
            {
                ClientData = req.ClientData,
                EvidenceFoler = req.EvidenceFoler,
                IpPort = req.IpPort,
                Headless = req.Headless,
                KeepPdf = req.KeepPdf
            };

            return await approvalFlow.ApplyForLoanAsync(page, model);
        }
        private async Task RunApprovalLoopAsync<TClientData>(
            IPage page,
            FullLoanRequest<TClientData> req,
            ILoanApplicationResult result, TransactionType transactionType)
            where TClientData : IClientData
        {
            var approvalFlow = _provider.GetRequiredService<ILoanApproval>();

            var nextUser = await _transactionUsersSelectionService
                                     .SelectOptimalUserAsync(transactionType, result.RecognizedApprovingUsers);

            for (int attempt = 1; attempt <= req.MaxApprovalUser; attempt++)
            {
                var model = new LoanApprovalModel
                {
                    ApprovingUser = nextUser,
                    ApprovalNumber = attempt,
                    EvidenceFoler = req.EvidenceFoler,
                    ApplicationNumber = result.ApplicationNumber,
                    IpPort = req.IpPort
                };

                var approvalResult = await approvalFlow.ApproveLoanAsync(page, model);
                _output.WriteLine("Flujo de aprobación ejecutado.");

                if (!approvalResult.RecognizedApprovingUsers.Any())
                {
                    HandleNoRecognizedUsers(approvalResult);
                    return;
                }

                if (approvalResult.ApprovalStatus == ApprovalStatus.APROBADO)
                    throw new InvalidOperationException("Bucle detectado: préstamo ya aprobado con usuarios pendientes.");

                if (attempt >= req.MaxApprovalUser)
                    throw new InvalidOperationException($"Se alcanzó el máximo ({req.MaxApprovalUser}) de intentos de aprobación.");

                nextUser = await _transactionUsersSelectionService
                                 .SelectOptimalUserAsync(transactionType, approvalResult.RecognizedApprovingUsers);

                await _branchSynchronizationService.ArriveAndWaitAsync(BranchId);
            }
        }
        private void HandleNoRecognizedUsers(LoanApprovalResultModel res)
        {
            if (res.ApprovalStatus == ApprovalStatus.APROBADO)
            {
                _output.WriteLine("Aprobación completada con éxito.");
            }
            else
            {
                throw new InvalidOperationException(
                    $"No hay usuarios reconocidos. Estado: {res.ApprovalStatus}");
            }
        }
        private async Task HandleErrorAsync<TClientData>(Exception ex, FullLoanRequest<TClientData> req, IPage page) where TClientData : IClientData
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var screenshotPath = Path.Combine(req.EvidenceFoler, $"Incidencia_{timestamp}.jpg");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = screenshotPath,
                FullPage = true
            });

            _output.WriteLine($"Error en flujo: {ex.Message}, {ex.StackTrace}");
            Assert.Fail($"Error en flujo: {ex.Message}");
        }
    }
}
