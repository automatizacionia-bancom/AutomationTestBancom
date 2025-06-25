using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Services.ActionCoordination;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications;
using AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApprovals.PersonalLoan
{
    public class LoanApproval
        :
        ILoanApproval<ClientDataT062900>,
        ILoanApproval<ClientDataT062800>,
        ILoanApproval<ClientDataT062700>,
        ILoanApproval<ClientDataT062500>,
        ILoanApproval<ClientDataT062400>
    {
        private readonly ElementRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly ITestOutputAccessor _outputAccessor;

        public LoanApproval(ElementRepositoryFixture locators, IPdfConverter pdfConverter, IStandardQueryService standardQueryService, IActionCoordinatorFactory actionCoordinatorService, ITestOutputAccessor output)
        {
            _locators = locators;
            _pdfConverter = pdfConverter;
            _standardQueryService = standardQueryService;
            _actionCoordinatorFactory = actionCoordinatorService;
            _outputAccessor = output;
        }
        public async Task<LoanApprovalResultModel> ApproveLoanAsync(IPage page, LoanApprovalModel loanAppproval)
        {
            await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
            {
                User = loanAppproval.ApprovingUser
            });

            await page.GotoAsync($"{loanAppproval.IpPort}/WEB3/ingreso.html");

            // Forzar cierre de sesión anterior
            Task forcelogin = page.Locator(_locators.Login.ForceLogin).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30000 });
            Task fillUser = page.Locator(_locators.Login.UsernameInput).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30000 });
            Task loginStatus = await Task.WhenAny(forcelogin, fillUser);

            if (loginStatus == forcelogin)
            {
                _outputAccessor.Output.WriteLine("Forzando cierre de sesión anterior.");
                await page.ClickAndWaitAsync(
                    page.Locator(_locators.Login.ForceLogin),
                    page.Locator(_locators.Login.UsernameInput),
                    new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 30000 // 30 seconds timeout for the force login to be processed
                    }, _outputAccessor.Output);
            }

            await page.Locator(_locators.Login.UsernameInput).FillAsync(loanAppproval.ApprovingUser);
            await page.Locator(_locators.Login.PasswordInput).FillAsync("fitbank123");
            await page.Locator(_locators.Login.SubmitButton).ClickAsync();

            await page.Locator(_locators.DashboardPage.TransactionInput).FillAsync("064000");
            await page.Locator(_locators.DashboardPage.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApprovalPage.ApplicationNumber).FillAsync(loanAppproval.ApplicationNumber);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F7Button),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            string applicationNumberResult = await page.Locator(_locators.ApprovalPage.ApplicationNumberResult).InputValueAsync();

            if (applicationNumberResult != loanAppproval.ApplicationNumber)
            {
                throw new Exception("El número de aplicación no coincide con el esperado. " +
                                    $"Esperado: {loanAppproval.ApplicationNumber}, " +
                                    $"Obtenido: {applicationNumberResult}");
            }

            await page.ClickAndWaitAsync(
                page.Locator(_locators.ApprovalPage.ApplicationNumberResult),
                page.Locator(_locators.DashboardPage.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.ApprovalSection),
                page.Locator(_locators.DashboardPage.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            await page.Locator(_locators.DashboardPage.RequestState).SelectOptionAsync(RequestStatus.APROBAR.ToString());
            await page.Locator(_locators.DashboardPage.RequestComment).FillAsync("QA");

            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApprovalCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F12Button),
                page.Locator(_locators.DashboardPage.OK_TransactionCorrect),
                page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 90000 // 90 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output, maxRetries: 10);
                await Task.Delay(1000); // Wait for the transaction to be processed
                await page.Locator(_locators.DashboardPage.OK_TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                });
            }

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanAppproval.EvidenceFoler, $"Aprobacion {loanAppproval.ApprovalNumber} - Estado.jpg"),
                FullPage = true
            });

            await page.Locator(_locators.DashboardPage.TransactionInput).FillAsync("064060");
            await page.Locator(_locators.DashboardPage.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.DashboardPage.ApplicationNumberSearch).FillAsync(loanAppproval.ApplicationNumber);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F7Button),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            string approvalStatusResult = await page.Locator(_locators.DashboardPage.ApprovalStatus).InputValueAsync();
            if (!Enum.TryParse(approvalStatusResult, true, out ApprovalStatus approvalStatus))
                throw new Exception($"El estado de aprobación '{approvalStatusResult}' no es válido.");

            List<string> approvingUsers = await GetApprovingUsersAsync(page, approvalStatus);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanAppproval.EvidenceFoler, $"Aprobacion {loanAppproval.ApprovalNumber} - Usuarios.jpg"),
                FullPage = true
            });

            _outputAccessor.Output.WriteLine($"Estado de aprobación: {approvalStatus}");

            return new LoanApprovalResultModel
            {
                RecognizedApprovingUsers = approvingUsers,
                ApprovalStatus = approvalStatus
            };
        }
        private async Task<List<string>> GetApprovingUsersAsync(IPage page, ApprovalStatus approvalStatus)
        {
            // Si el estado de aprobación es APROBADO, se espera que no haya usuarios autorizadores
            // Ejecutamos en bucle F7 para verificar los cambios en el fitbank, estos pueden tardar en reflejarse
            if (approvalStatus == ApprovalStatus.APROBADO)
            {
                for (int i = 0; i < 15; i++)
                {
                    string approvalProcess = await page.Locator(_locators.DashboardPage.ApprovalProcess).InputValueAsync();

                    if (approvalProcess == "DESEMBOLSADO" || approvalProcess == "EMISION")
                        return new List<string>();

                    await page.ClickAndWaitAsync(
                            page.Locator(_locators.DashboardPage.F7Button),
                            page.Locator(_locators.DashboardPage.TransactionCorrect),
                            new LocatorWaitForOptions
                            {
                                State = WaitForSelectorState.Visible,
                                Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                            }, _outputAccessor.Output);
                }
            }

            await page.Locator(_locators.DashboardPage.ApprovalUsers).ClickAsync();
            await Task.Delay(500); // Wait for the UI to stabilize
            Task hasApprovingUsersTask = page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
            Task hasNoApprovingUsersTask = page.Locator(_locators.DashboardPage.NonAuthorizingUsers).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });

            Task usersResultTask = await Task.WhenAny(hasApprovingUsersTask, hasNoApprovingUsersTask);

            List<string> approvingUsers = new();
            if (usersResultTask == hasApprovingUsersTask)
            {
                ILocator usersTable = page.Locator(_locators.DashboardPage.AprovalUsersList);
                var rows = usersTable.Locator("tbody > tr");
                int rowCount = await rows.CountAsync();

                for (int i = 0; i < rowCount; i++)
                {
                    var row = rows.Nth(i);
                    var cell = row.Locator("td").First;
                    string user = await cell.InnerTextAsync();
                    // Verifica si no está vacío y empieza con una letra
                    if (!string.IsNullOrWhiteSpace(user) && Regex.IsMatch(user[0].ToString(), @"^[a-zA-Z]"))
                    {
                        approvingUsers.Add(user);
                        _outputAccessor.Output.WriteLine($"Usuario {i + 1}: {user}");
                    }
                }
            }
            else
            {
                _outputAccessor.Output.WriteLine("No hay usuarios autorizadores disponibles para esta solicitud.");
            }

            return approvingUsers;
        }
    }
}
