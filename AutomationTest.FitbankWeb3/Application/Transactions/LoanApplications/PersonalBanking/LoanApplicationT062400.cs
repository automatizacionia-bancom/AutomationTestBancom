using System.Globalization;
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
    public class LoanApplicationT062400 : LoanApplicationPersonalBanking<ClientDataT062400>
    {
        public LoanApplicationT062400(
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        : base(locators, pdfConverter, standardQueryService, actionCoordinatorFactory, output)
        { }
        public override async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT062400> loanApplication)
        {
            ClientDataT062400 clientData = loanApplication.ClientData;

            // Verificar que el usuario no tenga una sesión activa
            await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
            {
                User = clientData.UserRequest
            });
            await _standardQueryService.ExecuteStandardQueryAsync<UpdatePasswordUserModel>(new UpdatePasswordUserModel
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
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("062400");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingresamos la identificación del cliente y una dirección válida
            await page.Locator(_locators.LocatorsT062400.Identification).FillAsync(clientData.Identification);
            await page.Locator(_locators.LocatorsT062400.Identification).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsT062400.AdressList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.AddressElement).ClickAsync();

            // Seleccionamos el producto y gestor
            await page.Locator(_locators.LocatorsT062400.ProductList).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(clientData.Product)).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsT062400.ManagerList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.ManagerElement).ClickAsync();

            // Ingresar numero de lote
            await page.Locator(_locators.LocatorsT062400.LotNumber).FillAsync(FlowExtensions.GenerateRandomString(10));
            await page.Locator(_locators.LocatorsT062400.LotNumber).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // ingreso de datos de la joya
            await page.Locator(_locators.LocatorsT062400.JewelTypeList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.JewelTypeElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062400.JewelSubTypeList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.JewelSubTypeElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062400.JewelCaratList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.JewelCaratElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062400.JewelConditionList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.JewelConditionElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062400.JewelDescription).FillAsync("QA");

            await page.Locator(_locators.LocatorsT062400.JewelGrossWeight).FillAsync(clientData.JewelGrossWeight.ToString());
            await page.Locator(_locators.LocatorsT062400.JewelGrossWeight).PressAsync("Enter");

            await page.Locator(_locators.LocatorsT062400.JewelEmbeddedWeight).FillAsync(clientData.JewelEmbeddedWeight.ToString());
            await page.Locator(_locators.LocatorsT062400.JewelEmbeddedWeight).PressAsync("Enter");

            // Regresamos los datos de la joya
            await page.Locator(_locators.LocatorsT062400.JewelData).ClickAsync();

            await page.WaitForTimeoutAsync(500);

            // Consulta en sentinel
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.ConsultSentinel).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsT062400.ConsultSentinel),
                    page.Locator(_locators.LocatorsT062400.OK_RangeError_GestorError),
                    
                    new LocatorWaitForOptions
                    {
                        Timeout = 60000, // 60 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
                await page.WaitForTimeoutAsync(500); // Esperar un segundo para asegurar que los cambios se reflejen
                await page.Locator(_locators.LocatorsT062400.OK_RangeError_GestorError).WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible
                });
            }

            // Rectificar Gestor 
            await page.Locator(_locators.LocatorsT062400.ManagerList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.ManagerElement).ClickAsync();

            // Ingresamos los datos del crédito
            await page.Locator(_locators.LocatorsT062400.CreditData).ClickAsync();
            await page.WaitForTimeoutAsync(500);

            // Ingresamos el monto solicitado y el plazo
            await page.Locator(_locators.LocatorsT062400.PaymentTerm).SelectOptionAsync(clientData.PaymentTerm.GetDescription());

            await page.Locator(_locators.LocatorsT062400.RequestedAmount).FillSimulatedAsync(clientData.RequestedAmount.ToString(CultureInfo.InvariantCulture));
            await page.Locator(_locators.LocatorsT062400.RequestedAmount).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            //await page.PauseAsync();

            // Verificar que se haya generado correctamente la tasa de interés
            string loanRate = await page.Locator(_locators.LocatorsT062400.LoanRate).InputValueAsync();
            if (string.IsNullOrWhiteSpace(loanRate))
                Assert.Fail("No se ha generado correctamente la tasa de la solicitud");

            if (clientData.DisbursementType == DisbursementType.AbonoACuenta)
            {
                await page.Locator(_locators.LocatorsT062400.DisbursementAcType).CheckAsync();
            }
            else
            {
                await page.Locator(_locators.LocatorsT062400.DisbursementOpType).CheckAsync();
            }

            await page.WaitForTimeoutAsync(500); // Esperar un segundo para asegurar que los cambios se reflejen

            // Crear la solicitud de préstamo y esperar a que se genere el número de solicitud
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApplicationCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.Locator(_locators.LocatorsGeneralDashboard.F12Button).ClickAsync();
                await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible
                });

                await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 130000, // 130 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output, maxRetries: 20);
            }

            string applicationNumber = await page.Locator(_locators.LocatorsT062400.ApplicationNumber).InputValueAsync();
            _outputAccessor.Output.WriteLine($"Solicitud: {applicationNumber}");

            // Ingresar los datos de ingresos del cliente si es necesario
            await AssingAdditonalIncomeAsync(page, clientData);

            // Evaluar la solicitud de préstamo y esperamos el resultado de la evaluación, tomando capturas de pantalla en el proceso
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsT062400.EvaluateButton),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

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
                await GetImgFromPdfDocument(loanApplication, "4. Documento"); // Convertir el PDF del PRT a JPEG
            }

            return new LoanApplicationResultT062400
            {
                ApplicationNumber = applicationNumber,
                EvaluationResult = evaluationResult,
                RecognizedApprovingUsers = approvingUsers
            };
        }
        private async Task AssingAdditonalIncomeAsync(IPage page, ClientDataT062400 clientData)
        {
            if (Convert.ToInt32(clientData.Income) == 0)
                return;

            await page.Locator(_locators.LocatorsT062400.IncomeButtton).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // ingresar el rubro de ingresos, descripcion y contacto
            await page.Locator(_locators.LocatorsT062400.IncomeCategoryList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.IncomeCategoryElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062400.IncomeDescription).FillAsync("QA");
            await page.Locator(_locators.LocatorsT062400.IncomeDescription).PressAsync("Enter");

            await page.Locator(_locators.LocatorsT062400.IncomeContactNumberList).ClickAsync();
            await page.Locator(_locators.LocatorsT062400.IncomeContactNumberElement).ClickAsync();

            await page.Locator(_locators.LocatorsT062400.IncomeDate).FillIfEditableAsync(DateTime.Now.AddMonths(-1).ToString("MMyyyy"));
            await page.Locator(_locators.LocatorsT062400.IncomeDate).PressAsync("Enter");

            await page.Locator(_locators.LocatorsT062400.IncomeAssets).FillIfEditableAsync(clientData.Income.ToString());
            await page.Locator(_locators.LocatorsT062400.IncomeOther).FillIfEditableAsync(clientData.Income.ToString());

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
                page.Locator(_locators.LocatorsT062400.IncomeReturn),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.Locator(_locators.LocatorsT062400.CreditData).ClickAsync(); // Volver a la sección de datos del crédito para continuar con la solicitud
        }
    }
}
