using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalBanking;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications
{
    public class LoanApplicationT062500 : LoanApplication<ClientDataT062500>
    {
        public LoanApplicationT062500(
            ElementRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        : base(locators, pdfConverter, standardQueryService, actionCoordinatorFactory, output)
        { }
        public override async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationModel<ClientDataT062500> loanApplication)
        {
            ClientDataT062500 clientData = loanApplication.ClientData;

            // Verificar que el usuario no tenga una sesión activa
            await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
            {
                User = clientData.UserRequest
            });

            await SearchProduct(page, clientData.Product); // Buscar el producto de forma rapida en la lista de productos

            // Ingresar a la página de inicio de sesión de Fitbank
            await page.GotoAsync($"{loanApplication.IpPort}/WEB3/ingreso.html");

            // Ingresar las credenciales del usuario
            await page.Locator(_locators.Login.UsernameInput).FillAsync(clientData.UserRequest);
            await page.Locator(_locators.Login.PasswordInput).FillAsync("fitbank123");
            await page.Locator(_locators.Login.SubmitButton).ClickAsync();

            // Ingresamos a la transacción T062500 para convenios PNP
            await page.Locator(_locators.DashboardPage.TransactionInput).FillAsync("062500");
            await page.Locator(_locators.DashboardPage.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingresamos la identificación del cliente y una dirección válida
            await page.Locator(_locators.ApplicationPageT062500.Identification).FillAsync(clientData.Identification);
            await page.Locator(_locators.ApplicationPageT062500.Identification).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062500.AdressList).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062500.AddressElement).ClickAsync();

            // Seleccionamos el producto, gestor y tipo de préstamo
            await page.Locator(_locators.ApplicationPageT062500.ProductList).ClickAsync();
            await page.Locator(_locators.DashboardPage.ListElement(clientData.Product)).ClickAsync();
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062500.ManagerList).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062500.ManagerElement).ClickAsync();

            await page.Locator(_locators.ApplicationPageT062500.LoanTypeList).ClickAsync();
            await page.Locator(_locators.DashboardPage.ListElement(clientData.LoanType.ToString())).ClickAsync();
            await page.WaitForTimeoutAsync(200);

            // Ingresamos el monto del préstamo y las cuotas
            await page.Locator(_locators.ApplicationPageT062500.LoanAmount).FillSimulatedAsync(clientData.LoanAmount.ToString(), "Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(delayBefore: 500 ,new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062500.CreditDataLabel).ClickAsync();
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062500.LoanInstallments).FillSimulatedAsync(clientData.LoanInstallments.ToString(), "Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Verificar que se haya generado correctamente la tasa de interés
            string loanRate = await page.Locator(_locators.ApplicationPageT062500.LoanRate).InputValueAsync();
            if (string.IsNullOrWhiteSpace(loanRate))
                Assert.Fail("No se ha generado correctamente la tasa de la solicitud");

            // Ingresamos la fehca de aprobacion y el número de remesa si es necesario
            if (await page.Locator(_locators.ApplicationPageT062500.ApprovalDate).IsVisibleAsync())
            {
                await page.Locator(_locators.ApplicationPageT062500.ApprovalDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            }

            if (await page.Locator(_locators.ApplicationPageT062500.RemittanceNumber).IsVisibleAsync())
            {
                await page.Locator(_locators.ApplicationPageT062500.RemittanceNumber).FillAsync(FlowExtensions.GenerateRandomString(15));
            }

            // Seleccionar la fuente de planilla
            await page.Locator(_locators.ApplicationPageT062500.PayrollSource).SelectOptionAsync(clientData.PayrollSource.GetDescription());

            if (clientData.DisbursementType == DisbursementType.OrdenDePago)
            {
                // Seleccionar el tipo de operación de desembolso
                await page.Locator(_locators.ApplicationPageT062500.DisbursementOpType).CheckAsync();
            }
            else  // Si es abono a cuenta o indefinido seleccionarmos la cuenta bancaria
            {
                await page.Locator(_locators.ApplicationPageT062500.BankAccountList).ClickAsync();
                await page.Locator(_locators.ApplicationPageT062500.BankAccountElement).ClickAsync();
            }

            await page.WaitForTimeoutAsync(500); // Esperar medio segundo para asegurar que la cuenta se haya seleccionado correctamente

            // Crear la solicitud de préstamo y esperar a que se genere el número de solicitud
            using (var handle = _actionCoordinatorFactory.GetCoordinator(Enums.ActionCoordinatorType.LoanApplicationCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.ClickAndWaitAsync(
                    page.Locator(_locators.DashboardPage.F12Button),
                    page.Locator(_locators.DashboardPage.OK),
                    page.Locator(_locators.DashboardPage.TransactionError),
                    new LocatorWaitForOptions
                    {
                        Timeout = 90000, // 90 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
            }

            string applicationNumber = await page.Locator(_locators.ApplicationPageT062500.ApplicationNumber).InputValueAsync();

            _outputAccessor.Output.WriteLine($"Solicitud: {applicationNumber}");

            // Ingresar los datos de ingresos del cliente si es necesario
            await AssingAdditonalIncomeAsync(page, clientData);

            // Evaluar la solicitud de préstamo y esperamos el resultado de la evaluación, tomando capturas de pantalla en el proceso
            await page.ClickAndWaitAsync(
                page.Locator(_locators.ApplicationPageT062500.EvaluateButton),
                page.Locator(_locators.DashboardPage.OK),
                page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 90000, // 90 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            if (!Enum.TryParse(await page.Locator(_locators.ApplicationPageT062500.EvaluateResult).InputValueAsync(), out EvaluationResult evaluationResult))
                throw new Exception("No se ha podido obtener el resultado de la evaluación correctamente.");

            // Se modificara el resultado si es necesario
            evaluationResult = await ModifyApplicationResultAsync(page, clientData.ModifyLoanApplication, applicationNumber, evaluationResult);
            _outputAccessor.Output.WriteLine($"Resultado de la evaluación: {evaluationResult}");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "1. Solicitud.jpg"),         // Ruta donde se guarda la imagen
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

            return new LoanApplicationResultT062500
            {
                ApplicationNumber = applicationNumber,
                EvaluationResult = evaluationResult,
                RecognizedApprovingUsers = approvingUsers
            };
        }
        private async Task AssingAdditonalIncomeAsync(IPage page, ClientDataT062500 clientData)
        {
            if (Convert.ToInt32(clientData.Income) == 0)
                return;

            await page.Locator(_locators.ApplicationPageT062500.IncomeButtton).ClickAsync();
            await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062500.IncomeDate).FillIfEditableAsync(DateTime.Now.AddMonths(-1).ToString("MMyyyy"));
            await page.Locator(_locators.ApplicationPageT062500.IncomeDate).PressAsync("Enter");

            await page.Locator(_locators.ApplicationPageT062500.IncomeAssets).FillIfEditableAsync(clientData.Income.ToString());
            await page.Locator(_locators.ApplicationPageT062500.IncomeAssets).PressAsync("Enter");

            await page.Locator(_locators.ApplicationPageT062500.IncomeOther).FillAsync(clientData.Income.ToString());
            await page.Locator(_locators.ApplicationPageT062500.IncomeOther).PressAsync("Enter");

            // Presionar F12 para guardar los cambios
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F12Button),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            // Presionar regresar para guardar los cambios
            await page.ClickAndWaitAsync(
                page.Locator(_locators.ApplicationPageT062500.IncomeReturn),
                page.Locator(_locators.DashboardPage.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }
    }
}
