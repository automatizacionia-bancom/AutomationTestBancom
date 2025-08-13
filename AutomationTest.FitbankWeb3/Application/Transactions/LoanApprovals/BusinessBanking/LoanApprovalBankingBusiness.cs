using System.Globalization;
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
    public class LoanApprovalBankingBusiness : ILoanApproval<ClientDataT072100Be>
    {
        private readonly LocatorRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly ITestOutputAccessor _outputAccessor;

        public LoanApprovalBankingBusiness(LocatorRepositoryFixture locators, IPdfConverter pdfConverter, IStandardQueryService standardQueryService, IActionCoordinatorFactory actionCoordinatorService, ITestOutputAccessor output)
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

            string currentActivity = await page.Locator(_locators.LocatorsBusinessBankingDashboard.TransactionCurrentActivity).InputValueAsync();

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionResult),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            if (currentActivity == "RECEPCIÓN DE AUTONOMÍA")
            {
                await AssingGuaranteeAndBusinessPlanAsync(page, loanAppproval);
            }

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

            List<string> approvalStatusElements = await GetFirstColumnAsync(page, _locators.LocatorsBusinessBankingDashboard.ApprovalStatusElements);

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

            string users = await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApprovalUsersList).InnerTextAsync();

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
        private async Task AssingGuaranteeAndBusinessPlanAsync(IPage page, LoanApprovalModel loanAppproval)
        {
            CoinType coinType = CoinType.Soles;

            // Ingreso de garantia
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeSection),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeCoverage).SelectOptionAsync("GLOBAL"); // Selecionar tipo de cobertura

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeType).ClickAsync(); // Lista de tipo de garantia

            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement("HIPOTECARIA")).ClickAsync();

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeGoodsType).ClickAsync(); // Lista de bien

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeGoodsTypeElement).ClickAsync(); // Selecionar primer bien

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeCondition).SelectOptionAsync("A POSTERIORI");

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeCoinType).SelectOptionFuzzyAsync(coinType.ToString());

            double rmgValue = double.Parse(await page.Locator(_locators.LocatorsBusinessBankingDashboard.RmgValue).InputValueAsync(), CultureInfo.InvariantCulture);

            double guaranteeValue = rmgValue * 1.5;

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeTaxAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeComertialValue).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeFinalValue).FillAsync(guaranteeValue.ToString());

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.GuaranteeDate).FillAsync(DateTime.Now.ToString(DateTime.Now.ToString("dd-MM-yyyy")));

            await page.WaitForTimeoutAsync(500);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            await page.WaitForTimeoutAsync(500);
            await page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanAppproval.EvidenceFoler, $"Aprobacion {loanAppproval.ApprovalNumber} - Garantia.jpeg"),
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            // Ingreso de plan de negocio
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.BusinessPlanSection),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.EspecificLinesSection).ClickAsync();

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.LineProductList).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElementPattern("FACTORING")).ClickAsync();

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.LineProductCoinType).SelectOptionFuzzyAsync(coinType.ToString());

            double lineProductValue = rmgValue * 0.2;

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.LineProductAmount).FillAsync(lineProductValue.ToString());

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.LineProductExpirationDay).FillAsync(DateTime.Now.AddYears(1).ToString("dd-MM-yyyy"));

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.LineProductRate).FillAsync("12.00");

            await page.WaitForTimeoutAsync(500);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            await page.WaitForTimeoutAsync(500);
            await page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanAppproval.EvidenceFoler, $"Aprobacion {loanAppproval.ApprovalNumber} - Plan de negocio.jpeg"),
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            // Asingnar la garantia
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.LineProductAssingGuaranteeButton),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            await page.Locator(_locators.LocatorsBusinessBankingDashboard.LineProductAssingGuaranteeBox).CheckAsync();

            await page.WaitForTimeoutAsync(500);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            await page.WaitForTimeoutAsync(500);
            await page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Asingnar regresamos a la ventana
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsBusinessBankingDashboard.LineProductAssingGuaranteeReturn),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);
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
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30_000
            });

            // Varifivicar si el usuario esta visible, caso contrario buscarlo
            bool isUserVisible = await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(currentUser)).IsVisibleAsync();

            if (!isUserVisible)
            {
                await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionAssingInput).FillAsync(currentUser);
                await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApplicationNumberSearchTransactionAssingInput).PressAsync("Enter");
                await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30_000
                });
            }

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
