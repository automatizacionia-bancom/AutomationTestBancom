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
    public class LoanApplicationT062700 : LoanApplicationPersonalBanking<ClientDataT062700>
    {
        public LoanApplicationT062700(
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        : base(locators, pdfConverter, standardQueryService, actionCoordinatorFactory, output)
        { }
        public override async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT062700> loanApplication)
        {
            ClientDataT062700 clientData = loanApplication.ClientData;

            await SearchProduct(page, clientData.Product); // Buscar el producto de forma rapida en la lista de productos

            // Ingresar a la página de inicio de sesión de Fitbank
            await page.GotoAsync($"{loanApplication.IpPort}/WEB3/ingreso.html");

            // Ingresar las credenciales del usuario
            await page.Locator(_locators.LocatorsLogin.UsernameInput).FillAsync(clientData.UserRequest);
            await page.Locator(_locators.LocatorsLogin.PasswordInput).FillAsync("fitbank123");
            await page.Locator(_locators.LocatorsLogin.SubmitButton).ClickAsync();

            // Ingresamos a la transacción T062800 para convenios
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("062700");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingresamos la identificación del cliente y una dirección válida
            await page.Locator(_locators.LocatorsT062700.Identification).FillAsync(clientData.Identification);
            await page.Locator(_locators.LocatorsT062700.Identification).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsT062700.AdressList).ClickAsync();
            await page.Locator(_locators.LocatorsT062700.AddressElement).ClickAsync();
            //await page.WaitForTimeoutAsync(10000); // Agregar conyugue manual

            // Seleccionamos el producto, gestor y tipo de préstamo
            await page.Locator(_locators.LocatorsT062700.ProductList).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(clientData.Product)).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });
            await page.Locator(_locators.LocatorsT062700.ManagerList).ClickAsync();
            await page.Locator(_locators.LocatorsT062700.ManagerElement).ClickAsync();

            // Ingresamos el monto de la linea
            await page.Locator(_locators.LocatorsT062700.CreditLine).FillSimulatedAsync(clientData.CreditLine.ToString(), "Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Espera intermedia en un elemento externo
            await page.Locator(_locators.LocatorsT062700.CreditDataLabel).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsT062700.ExternalEvaluation).SelectOptionAsync("APROBADO");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Verificar que se haya generado correctamente la tasa de interés
            string loanRate = await page.Locator(_locators.LocatorsT062700.LoanRate).InputValueAsync();
            if (string.IsNullOrWhiteSpace(loanRate))
                Assert.Fail("No se ha generado correctamente la tasa de la solicitud");

            // ingreso de entrega TCR y EECC
            await page.Locator(_locators.LocatorsT062700.TcrList).ClickAsync();
            await page.Locator(_locators.LocatorsT062700.TcrElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062700.EeccList).ClickAsync();
            await page.Locator(_locators.LocatorsT062700.EcccElement).ClickAsync();

            // Ingreso de emboss y ciclo de facturación
            await page.Locator(_locators.LocatorsT062700.Emboss).FillAsync(FlowExtensions.GenerateRandomString(20));
            await page.Locator(_locators.LocatorsT062700.Emboss).PressAsync("Enter");

            await page.Locator(_locators.LocatorsT062700.BillingCycleList).ClickAsync(new LocatorClickOptions
            {
                ClickCount = 2 // Doble click para seleccionar el ciclo de facturación
            });
            int billinCycle = (int)clientData.BillingCycle;
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(billinCycle.ToString())).ClickAsync(new LocatorClickOptions
            {
                ClickCount = 2 // Doble click para seleccionar el ciclo de facturación
            });

            await page.WaitForTimeoutAsync(500); // Esperar un segundo para asegurar que los cambios se reflejen
            // Crear la solicitud de préstamo y esperar a que se genere el número de solicitud
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApplicationCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 90000, // 90 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output, maxRetries: 20);
            }

            string applicationNumber = await page.Locator(_locators.LocatorsT062700.ApplicationNumber).InputValueAsync();
            _outputAccessor.Output.WriteLine($"Solicitud: {applicationNumber}");

            // Ingresar los datos de ingresos del cliente si es necesario
            await AssingAdditonalIncomeAsync(page, clientData);

            // Asignar la garantía según el tipo de préstamo si requiere
            await AssingGuaranteeAsync(page, loanApplication);

            // Evaluar la solicitud de préstamo y esperamos el resultado de la evaluación, tomando capturas de pantalla en el proceso
            // Consideramos el error en el ambiente de QA, donde el botón de evaluación puede dar 'Error al procesar la consulta'
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsT062700.EvaluateButton),
                page.Locator(_locators.LocatorsT062700.OK_ApprovalError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            bool approvalError = await page.Locator(_locators.LocatorsT062700.ApprovalError).IsVisibleAsync();
            if (approvalError)
            {
                await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsGeneralDashboard.F7Button),
                    page.Locator(_locators.LocatorsGeneralDashboard.OK),
                    new LocatorWaitForOptions
                    {
                        Timeout = 60000, // 90 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
            }

            // Se modificara el resultado si es necesario
            await ModifyApplicationResultAsync(page, clientData.ModifyLoanApplication, applicationNumber);

            if (!Enum.TryParse(await page.Locator(_locators.LocatorsT062400.EvaluateResult).InputValueAsync(), out EvaluationResult evaluationResult))
                throw new Exception("No se ha podido obtener el resultado de la evaluación correctamente.");

            _outputAccessor.Output.WriteLine($"Resultado de la evaluación: {evaluationResult}");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFolder, "1. Solicitud.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            // Tomar Capturas de pantalla de los resultados de la evaluación
            await GetCalificationResultAsync(page, loanApplication.EvidenceFolder);

            // Tomar capturas de pantalla a los criterios de evaluación de riesgo la solicitud
            await GetCarsResultAsync(page, loanApplication.EvidenceFolder);

            await ApproveAndGetPdfAsync(
                page,
                evaluationResult,
                clientData.RequestState,
                clientData.RequestType,
                loanApplication.EvidenceFolder,
                loanApplication.Headless,
                clientData.RequestObservation1,
                clientData.RequestObservation2);

            List<string> approvingUsers = await GetApprovingUsersAsync(page, applicationNumber, loanApplication.EvidenceFolder);

            // Convertir el PDF a PNG solo si es headless
            if (loanApplication.Headless)
            {
                await GetImgFromPdfDocument(loanApplication, "4. PRT"); // Convertir el PDF del PRT a JPEG
            }

            // Realizamos las aprobacion de verificacion domiciliaria y laboral
            await ValidateDocumentationAsync(page, applicationNumber); // Verificacion 1
            await ValidateDocumentationAsync(page, applicationNumber); // Verificacion 2 para Maxiprestamos


            return new LoanApplicationResultT062700
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

            await page.Locator(_locators.LocatorsT062700.ValidateDocumentationVerification).CheckAsync();
            await page.Locator(_locators.LocatorsT062700.ValidateDocumentationType).ClickAsync();
            await page.Locator(_locators.LocatorsT062700.ValidateDocumentationTypeElement).ClickAsync();
            await page.Locator(_locators.LocatorsT062700.ValidateDocumentationDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            await page.Locator(_locators.LocatorsT062700.ValidateDocumentationObservation).FillAsync("QA");
            await page.Locator(_locators.LocatorsT062700.ValidateDocumentationResult).SelectOptionAsync("CONFORME");

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
        private async Task AssingAdditonalIncomeAsync(IPage page, ClientDataT062700 clientData)
        {
            if (Convert.ToInt32(clientData.Income) == 0)
                return;

            await page.Locator(_locators.LocatorsT062700.IncomeButtton).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsT062700.IncomeDate).FillIfEditableAsync(DateTime.Now.AddMonths(-1).ToString("MMyyyy"));
            await page.Locator(_locators.LocatorsT062700.IncomeDate).PressAsync("Enter");

            await page.Locator(_locators.LocatorsT062700.IncomeAssets).FillAsync(clientData.Income.ToString());

            await page.Locator(_locators.LocatorsT062700.IncomeOther1).FillIfEditableAsync(clientData.Income.ToString());
            await page.Locator(_locators.LocatorsT062700.IncomeOther2).FillIfEditableAsync(clientData.Income.ToString());

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
                page.Locator(_locators.LocatorsT062700.IncomeReturn),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }
        private async Task AssingGuaranteeAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT062700> loanApplication)
        {
            if (loanApplication.ClientData.GuaranteeType == GuaranteeType.SinGarantia)
                return;

            await page.Locator(_locators.LocatorsT062700.GuaranteeButton).ClickAsync();
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

            await page.Locator(_locators.LocatorsT062700.GuaranteeType).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(guaranteeType)).ClickAsync();

            await page.Locator(_locators.LocatorsT062700.GuaranteeGoodsType).ClickAsync();
            await page.Locator(_locators.LocatorsT062700.GuaranteeGoodsTypeElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062700.GuaranteeCoinType).SelectOptionAsync(CoinType.Soles.ToString());

            double guaranteeValue = loanApplication.ClientData.CreditLine * 1.2;
            await page.Locator(_locators.LocatorsT062700.GuaranteeTaxAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.LocatorsT062700.GuaranteeAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.LocatorsT062700.GuaranteeDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            await page.Locator(_locators.LocatorsT062700.GuaranteeDescription).FillAsync("QA");

            await page.WaitForTimeoutAsync(500);
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
                Path = Path.Combine(loanApplication.EvidenceFolder, "7. Garantia.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsT062700.GuaranteeReturn),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }

    }
}
