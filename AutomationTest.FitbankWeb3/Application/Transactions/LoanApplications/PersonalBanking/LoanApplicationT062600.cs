using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalBanking
{
    public class LoanApplicationT062600 : LoanApplication<ClientDataT062600>
    {
        public LoanApplicationT062600(
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        : base(locators, pdfConverter, standardQueryService, actionCoordinatorFactory, output)
        { }
        public override async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT062600> loanApplication)
        {
            ClientDataT062600 clientData = loanApplication.ClientData;

            // Verificar que el usuario no tenga una sesión activa
            await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
            {
                User = clientData.UserRequest
            });

            await SearchProduct(page, clientData.Product); // Buscar el producto de forma rapida en la lista de productos

            // Ingresar a la página de inicio de sesión de Fitbank
            await page.GotoAsync($"{loanApplication.IpPort}/WEB3/ingreso.html");

            // Ingresar las credenciales del usuario
            await page.Locator(_locators.LocatorsLogin.UsernameInput).FillAsync(clientData.UserRequest);
            await page.Locator(_locators.LocatorsLogin.PasswordInput).FillAsync("fitbank123");
            await page.Locator(_locators.LocatorsLogin.SubmitButton).ClickAsync();

            // Ingresamos a la transacción T062800 para convenios
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("062600");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingresamos la identificación del cliente y una dirección válida
            await page.Locator(_locators.LocatorsT062600.Identification).FillAsync(clientData.Identification);
            await page.Locator(_locators.LocatorsT062600.Identification).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsT062600.AdressList).ClickAsync();
            await page.Locator(_locators.LocatorsT062600.AddressElement).ClickAsync();

            // Seleccionamos el producto, gestor y tipo de préstamo
            await page.Locator(_locators.LocatorsT062600.ProductList).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(clientData.Product)).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });
            await page.Locator(_locators.LocatorsT062600.ManagerList).ClickAsync();
            await page.Locator(_locators.LocatorsT062600.ManagerElement).ClickAsync();

            // Seleccionar Tipo Seguro
            await page.Locator(_locators.LocatorsT062600.MortgageInsurance1).SelectOptionAsync(clientData.MortgageInsurance1.GetDescription());
            await page.Locator(_locators.LocatorsT062600.MortgageInsurance2).SelectOptionAsync(clientData.MortgageInsurance2.GetDescription());

            // Seleccionar Tipo Bien
            await page.Locator(_locators.LocatorsT062600.MortgageGood).SelectOptionAsync(clientData.MortgageGood.GetDescription());

            // Selecionar Tipo Proyecto
            await page.Locator(_locators.LocatorsT062600.MortgageProject).SelectOptionAsync(clientData.MortgageProject.GetDescription());

            // Selecionar Grado si lo requiere
            if(await page.Locator(_locators.LocatorsT062600.MortgageGrade).IsEnabledAsync())
                await page.Locator(_locators.LocatorsT062600.MortgageGrade).SelectOptionAsync("Grado 1");

            // Ingresamos el valor estimado, cuota inicial y numero de cuotas
            await page.Locator(_locators.LocatorsT062600.EstimatedValue).FillSimulatedAsync(clientData.EstimatedValue.ToString(), "Enter");
            await page.Locator(_locators.LocatorsT062600.DownPayment).FillSimulatedAsync(clientData.DownPayment.ToString(), "Enter");
            await page.Locator(_locators.LocatorsT062600.LoanInstallments).FillSimulatedAsync(clientData.LoanInstallments.ToString(), "Enter");

            // Ingresamos el importe a financiar
            double financedAmount = clientData.EstimatedValue - clientData.DownPayment;
            await page.Locator(_locators.LocatorsT062600.FinancedAmount).FillSimulatedAsync(financedAmount.ToString(), "Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Espera intermedia en un elemento externo
            await page.Locator(_locators.LocatorsT062600.CreditDataLabel).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingresar la tasa de interés
            await page.Locator(_locators.LocatorsT062600.LoanRate).FillAsync(clientData.LoanRate.ToString());

            // Evaluacion Externa
            await page.Locator(_locators.LocatorsT062600.ExternalEvaluation).SelectOptionAsync("APROBADO");

            // Ingresar Bono
            if (clientData.MortgageBond != MortgageBondType.SinBono)
            {
                await page.Locator(_locators.LocatorsT062600.MortgageBondList).ClickAsync();
                await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(clientData.MortgageBond.GetDescription())).ClickAsync();
            }

            await page.WaitForTimeoutAsync(500); // Esperar un segundo para asegurar que los cambios se reflejen
            // Crear la solicitud de préstamo y esperar a que se genere el número de solicitud
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApplicationCoordinator).CreateHandle())
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
                }, _outputAccessor.Output, maxRetries: 20);
            }

            string applicationNumber = await page.Locator(_locators.LocatorsT062600.ApplicationNumber).InputValueAsync();
            _outputAccessor.Output.WriteLine($"Solicitud: {applicationNumber}");

            // Ingresar los datos de ingresos del cliente si es necesario
            await AssingAdditonalIncomeAsync(page, clientData);

            // Ingresar la garantia
            await AssingGuaranteeAsync(page, loanApplication);

            // Evaluar la solicitud de préstamo y esperamos el resultado de la evaluación, tomando capturas de pantalla en el proceso
            // Consideramos el error en el ambiente de QA, donde el botón de evaluación puede dar 'Error al procesar la consulta'
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsT062600.EvaluateButton),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            if (!Enum.TryParse(await page.Locator(_locators.LocatorsT062600.EvaluateResult).InputValueAsync(), out EvaluationResult evaluationResult))
                throw new Exception("No se ha podido obtener el resultado de la evaluación correctamente.");

            // Se modificara el resultado si es necesario
            evaluationResult = await ModifyApplicationResultAsync(page, clientData.ModifyLoanApplication, applicationNumber, evaluationResult);
            _outputAccessor.Output.WriteLine($"Resultado de la evaluación: {evaluationResult}");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "1. Solicitud.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            // Tomar Capturas de pantalla de los resultados de la evaluación
            await GetCalificationResultAsync(page, loanApplication.EvidenceFoler);

            // Tomar capturas de pantalla a los criterios de evaluación de riesgo la solicitud
            await GetCarsResultAsync(page, loanApplication.EvidenceFoler);

            await ApproveAndGetPdfAsync(
                page,
                evaluationResult,
                clientData.RequestState,
                clientData.RequestType,
                loanApplication.EvidenceFoler,
                loanApplication.Headless,
                clientData.RequestObservation1,
                clientData.RequestObservation2);

            List<string> approvingUsers = await GetApprovingUsersAsync(page, applicationNumber, loanApplication.EvidenceFoler);

            // Convertir el PDF a PNG solo si es headless
            if (loanApplication.Headless)
            {
                await GetImgFromPdfDocument(loanApplication.EvidenceFoler, loanApplication.KeepPdf); // Convertir el PDF del PRT a PNG
            }

            // Realizamos las aprobacion de verificacion domiciliaria y laboral
            await ValidateDocumentationAsync(page, applicationNumber); 
            await ValidateDocumentationAsync(page, applicationNumber);


            return new LoanApplicationResultT062600
            {
                ApplicationNumber = applicationNumber,
                EvaluationResult = evaluationResult,
                RecognizedApprovingUsers = approvingUsers
            };
        }
        private async Task ValidateDocumentationAsync(IPage page, string applicationNumber)
        {
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("064000");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsPersonalBankingDashboard.ApplicationNumberSearchTransaction).FillAsync(applicationNumber);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F7Button),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 60000 // 60 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            string applicationNumberResult = await page.Locator(_locators.LocatorsPersonalBankingDashboard.ApplicationNumberSearchTransactionResult).InputValueAsync();

            if (applicationNumberResult != applicationNumber)
            {
                throw new Exception("El número de aplicación no coincide con el esperado. " +
                                    $"Esperado: {applicationNumber}, " +
                                    $"Obtenido: {applicationNumberResult}");
            }

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsPersonalBankingDashboard.ApplicationNumberSearchTransactionResult),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 60000 // 60 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            await page.Locator(_locators.LocatorsT062600.ValidateDocumentationVerification).CheckAsync();
            await page.Locator(_locators.LocatorsT062600.ValidateDocumentationType).ClickAsync();
            await page.Locator(_locators.LocatorsT062600.ValidateDocumentationTypeElement).ClickAsync();
            await page.Locator(_locators.LocatorsT062600.ValidateDocumentationDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            await page.Locator(_locators.LocatorsT062600.ValidateDocumentationObservation).FillAsync("QA");
            await page.Locator(_locators.LocatorsT062600.ValidateDocumentationResult).SelectOptionAsync("CONFORME");

            await page.WaitForTimeoutAsync(500); // Esperar para asegurar que los cambios se reflejen

            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApprovalCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();
                // Presionar F12 para guardar los cambios
                await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            }
        }
        private async Task AssingAdditonalIncomeAsync(IPage page, ClientDataT062600 clientData)
        {
            if (Convert.ToInt32(clientData.Income) == 0)
                return;

            await page.Locator(_locators.LocatorsT062600.IncomeButtton).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsT062600.IncomeDate).FillIfEditableAsync(DateTime.Now.AddMonths(-1).ToString("MMyyyy"));
            await page.Locator(_locators.LocatorsT062600.IncomeDate).PressAsync("Enter");

            await page.Locator(_locators.LocatorsT062600.IncomeAssets).FillAsync(clientData.Income.ToString());

            await page.Locator(_locators.LocatorsT062600.IncomeOther1).FillIfEditableAsync(clientData.Income.ToString());

            // Presionar F12 para guardar los cambios
            await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                    page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                    new LocatorWaitForOptions
                    {
                        Timeout = 60000, // 60 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);

            // Presionar regresar para guardar los cambios
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsT062600.IncomeReturn),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }
        private async Task AssingGuaranteeAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT062600> loanApplication)
        {
            if (loanApplication.ClientData.GuaranteeType == GuaranteeType.SinGarantia)
                return;

            await page.Locator(_locators.LocatorsT062600.GuaranteeButton).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            string guaranteeType = loanApplication.ClientData.GuaranteeType switch
            {
                GuaranteeType.GarantiaLiquida => "DEPÓSITOS",
                GuaranteeType.GarantiaNoLiquida => "HIPOTECARIA",
                _ => throw new ArgumentOutOfRangeException(nameof(loanApplication.ClientData.GuaranteeType), "Tipo de garantía no soportado.")
            };

            await page.Locator(_locators.LocatorsT062600.GuaranteeType).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(guaranteeType)).ClickAsync();

            await page.Locator(_locators.LocatorsT062600.GuaranteeGoodsType).ClickAsync();
            await page.Locator(_locators.LocatorsT062600.GuaranteeGoodsTypeElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062600.GuaranteeCoinType).SelectOptionAsync(CoinType.Soles.ToString());

            double guaranteeValue = loanApplication.ClientData.EstimatedValue * 1.2;
            await page.Locator(_locators.LocatorsT062600.GuaranteeTaxAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.LocatorsT062600.GuaranteeAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.LocatorsT062600.GuaranteeDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            await page.Locator(_locators.LocatorsT062600.GuaranteeDescription).FillAsync("QA");

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output, maxRetries: 10);
            await page.WaitForTimeoutAsync(500);
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "7. Garantia.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsT062600.GuaranteeReturn),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }
    }
}
