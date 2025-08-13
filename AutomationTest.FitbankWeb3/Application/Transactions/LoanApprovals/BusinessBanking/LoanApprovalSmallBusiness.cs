using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Output;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApprovals.BusinessBanking
{
    public class LoanApprovalSmallBusiness : ILoanApproval<ClientDataT072100Pe>
    {
        private readonly LocatorRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly ITestOutputAccessor _outputAccessor;

        public LoanApprovalSmallBusiness(LocatorRepositoryFixture locators, IPdfConverter pdfConverter, IStandardQueryService standardQueryService, IActionCoordinatorFactory actionCoordinatorService, ITestOutputAccessor output)
        {
            _locators = locators;
            _pdfConverter = pdfConverter;
            _standardQueryService = standardQueryService;
            _actionCoordinatorFactory = actionCoordinatorService;
            _outputAccessor = output;
        }
        public async Task<LoanApprovalResultModel> ApproveLoanAsync(IPage page, LoanApprovalModel loanAppproval)
        {
            await page.GotoAsync($"{loanAppproval.IpPort}/WEB3/ingreso.html");

            // Forzar cierre de sesión anterior
            Task forcelogin = page.Locator(_locators.LocatorsLogin.ForceLogin).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30000 });
            Task fillUser = page.Locator(_locators.LocatorsLogin.UsernameInput).WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 30000 });
            Task loginStatus = await Task.WhenAny(forcelogin, fillUser);

            if (loginStatus == forcelogin)
            {
                _outputAccessor.Output.WriteLine("Forzando cierre de sesión anterior.");
                await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsLogin.ForceLogin),
                    page.Locator(_locators.LocatorsLogin.UsernameInput),
                    new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 30000 // 30 seconds timeout for the force login to be processed
                    }, _outputAccessor.Output);
            }

            await page.Locator(_locators.LocatorsLogin.UsernameInput).FillAsync(loanAppproval.ApprovingUser);
            await page.Locator(_locators.LocatorsLogin.PasswordInput).FillAsync("fitbank123");
            await page.Locator(_locators.LocatorsLogin.SubmitButton).ClickAsync();

            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("074113");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.FormProcessing).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden
            });

            // Buscar la transaccion en la 074113
            await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransaction).FillAsync(loanAppproval.ApplicationNumber);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F7Button),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            string applicationNumberResult = await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionResult).InnerTextAsync();

            if (applicationNumberResult != loanAppproval.ApplicationNumber)
            {
                //throw new Exception("El número de aplicación no coincide con el esperado. " +
                //                    $"Esperado: {loanAppproval.ApplicationNumber}, " +
                //                    $"Obtenido: {applicationNumberResult}");
                await EconomicEvaluationRecord(page, loanAppproval.ApplicationNumber, loanAppproval.ApprovingUser);

                await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("074113");
                await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
                await page.Locator(_locators.LocatorsGeneralDashboard.FormCorrect).WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible
                });

                await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransaction).FillAsync(loanAppproval.ApplicationNumber);
                await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsGeneralDashboard.F7Button),
                    page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                    new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                    }, _outputAccessor.Output);
            }

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionResult),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            // Ir a la seccion de aprobacion
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.ApprovalSection),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.ApprovalStatusList),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            List<string> approvalStatusElements = await GetFirstColumnAsync(page, _locators.LocatorsBusinessBankingDashboard.ApprovalStatusElementsPe);

            // Define las opciones en orden de prioridad
            string[] options = new[] { "APROBADO", "POR CONFIRMAR", "OBSERVADO" };

            //bool hasReconsidered = await AnyInputInTableContainsAsync(page, "RECONSIDERADO");

            //if (!hasReconsidered) // Si no hay "RECONSIDERADO" en la tabla, lo agrega al inicio de las opciones como prioridad
            //{
            //    var list = options.ToList();
            //    list.Insert(0, "RECONSIDERADO");      // inserta al inicio
            //    options = list.ToArray();
            //}

            // Elige la primera que esté presente en approvalStatusElements o, de lo contrario, la última
            string selected = options.FirstOrDefault(opt => approvalStatusElements.Contains(opt))
                           ?? options.Last();

            // Haz click en el elemento correspondiente
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(selected)).ClickAsync();

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApprovalComment).FillAsync("QA");

            await page.WaitForTimeoutAsync(500);

            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApprovalCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                    page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                    page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                    new LocatorWaitForOptions
                    {
                        Timeout = 90000, // 90 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output, maxRetries: 15);

                // Contiene doble flag
                await page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect).WaitForAsync(delayBefore: 1000, new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible
                });
            }

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.ApprovalUsersButton),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanAppproval.EvidenceFoler, $"Aprobacion {loanAppproval.ApprovalNumber}.jpeg"),
                FullPage = true
            });

            string users = await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApprovalUsersListPe).InnerTextAsync();

            List<string> usersList = new List<string>();
            if (!string.IsNullOrWhiteSpace(users))
            {
                usersList = users
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(u => u.Trim())
                .Where(u => !string.IsNullOrEmpty(u))
                .ToList();
            }

            for (int i = 0; i < usersList.Count(); i++)
            {
                string user = usersList[i];
                _outputAccessor.Output?.WriteLine($"Usuario {i + 1}: {user}");
            }

            ApprovalStatus approvalStatus = usersList.Count > 0 ? ApprovalStatus.PENDIENTE : ApprovalStatus.APROBADO;

            return new LoanApprovalResultModel
            {
                RecognizedApprovingUsers = usersList,
                ApprovalStatus = approvalStatus
            };
        }
        private async Task<bool> AnyInputInTableContainsAsync(IPage page, string needle)
        {
            if (string.IsNullOrWhiteSpace(needle)) return false;
            needle = needle.Trim().ToLowerInvariant();

            var inputs = page.Locator("table.tabla.table-group tbody tr.clonada input.record.input.none[type='text']");
            var count = await inputs.CountAsync();

            for (int i = 0; i < count; i++)
            {
                var val = await inputs.Nth(i).InputValueAsync(); // lee la propiedad value actual
                if (!string.IsNullOrEmpty(val) && val.ToLowerInvariant().Contains(needle))
                    return true;
            }
            return false;
        }
        private async Task EconomicEvaluationRecord(IPage page, string applicationNumber, string currentUser)
        {
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("074119");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.FormCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionAssing).FillAsync(applicationNumber);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F7Button),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            string applicationNumberResult = await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionAssingResult).InputValueAsync();

            if (applicationNumberResult != applicationNumber)
            {
                throw new Exception("El número de aplicación no coincide con el esperado para la asginacion de prppuesta de credito. " +
                                $"Esperado: {applicationNumber}, " +
                                $"Obtenido: {applicationNumberResult}");
            }

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionAssingList).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(currentUser)).ClickAsync();


            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionAssingButton),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });
        }
        private async Task<List<string>> GetFirstColumnAsync(IPage page, string tbodySelector)
        {
            var results = new List<string>();

            // Localiza todas las filas
            var rows = page.Locator($"{tbodySelector} > tr");
            int rowCount = await rows.CountAsync();

            for (int i = 0; i < rowCount; i++)
            {
                // Para cada fila, toma la primera <td> y lee su texto
                var firstCell = rows.Nth(i).Locator("td").First;
                string text = await firstCell.InnerTextAsync();
                results.Add(text.Trim());
            }

            return results;
        }
    }
}
