using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Services.ActionCoordination;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApprovals;
using AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalLoan
{
    public class LoanApplicationT062800 : LoanApplication<ClientDataT062800>
    {
        private readonly ElementRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly ITestOutputAccessor _outputAccessor;

        public LoanApplicationT062800(ElementRepositoryFixture locators, IPdfConverter pdfConverter, IStandardQueryService standardQueryService, IActionCoordinatorFactory actionCoordinatorFactory, ITestOutputAccessor output)
        {
            _locators = locators;
            _pdfConverter = pdfConverter;
            _standardQueryService = standardQueryService;
            _actionCoordinatorFactory = actionCoordinatorFactory;
            _outputAccessor = output;
        }
        public override async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationModel<ClientDataT062800> loanApplication)
        {
            ClientDataT062800 clientData = loanApplication.ClientData;

            // Verificar que el usuario no tenga una sesión activa
            await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
            {
                User = clientData.UserRequest
            });

            await SearchProduct(page, clientData.Product, clientData.ProductGroup, clientData.CoinType, _outputAccessor.Output); // Buscar el producto de forma rapida en la lista de productos

            // Ingresar a la página de inicio de sesión de Fitbank
            await page.GotoAsync($"{loanApplication.IpPort}/WEB3/ingreso.html");

            // Ingresar las credenciales del usuario
            await page.Locator(_locators.Login.UsernameInput).FillAsync(clientData.UserRequest);
            await page.Locator(_locators.Login.PasswordInput).FillAsync("fitbank123");
            await page.Locator(_locators.Login.SubmitButton).ClickAsync();

            // Ingresamos a la transacción T062900 para convenios PNP
            await page.Locator(_locators.DashboardPage.TransactionInput).FillAsync("062800");
            await page.Locator(_locators.DashboardPage.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingresamos la identificación del cliente y una dirección válida
            await page.Locator(_locators.ApplicationPageT062800.Identification).FillAsync(clientData.Identification);
            await page.Locator(_locators.ApplicationPageT062800.Identification).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062800.AdressList).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062800.AddressElement).ClickAsync();

            // Seleccionamos el producto, gestor y tipo de préstamo
            await page.Locator(_locators.ApplicationPageT062800.ProductList).ClickAsync();
            await page.Locator(_locators.DashboardPage.ListElement(clientData.Product)).ClickAsync();
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            Transacion062800Type transacion062800Type = clientData.ProductGroup switch
            {
                "05" => Transacion062800Type.Maxiprestamos,
                "11" => Transacion062800Type.Convenios,
                _ => Transacion062800Type.Unspecified
            };

            await page.Locator(_locators.ApplicationPageT062800.ManagerList).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062800.ManagerElement).ClickAsync();

            await page.Locator(_locators.ApplicationPageT062800.LoanTypeList).ClickAsync();
            await page.Locator(_locators.DashboardPage.ListElement(clientData.LoanType.ToString())).ClickAsync();

            // Ingresamos el monto del préstamo y las cuotas
            await page.Locator(_locators.ApplicationPageT062800.LoanAmount).FillAsync(clientData.LoanAmount.ToString());
            await page.Locator(_locators.ApplicationPageT062800.LoanAmount).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062800.LoanInstallments).FillAsync(clientData.LoanInstallments.ToString());
            await page.Locator(_locators.ApplicationPageT062800.LoanInstallments).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Verificar que se haya generado correctamente la tasa de interés
            string loanRate = await page.Locator(_locators.ApplicationPageT062800.LoanRate).InputValueAsync();
            if (string.IsNullOrWhiteSpace(loanRate))
                Assert.Fail("No se ha generado correctamente la tasa de la solicitud");

            // Seleccionar forma de desembolso
            if (clientData.DisbursementType != DisbursementType.Unspecified)
            {
                await page.Locator(_locators.ApplicationPageT062800.DisbursementType).SelectOptionAsync(clientData.DisbursementType.GetDescription());

                if (clientData.DisbursementType == DisbursementType.AbonoACuenta)
                {
                    await page.Locator(_locators.ApplicationPageT062800.BankAccountList).ClickAsync();
                    await page.Locator(_locators.ApplicationPageT062800.BankAccountElement).ClickAsync();
                }

            }
            await page.WaitForTimeoutAsync(500); // Esperar un segundo para asegurar que los cambios se reflejen

            // Crear la solicitud de préstamo y esperar a que se genere el número de solicitud
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApplicationCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F12Button),
                page.Locator(_locators.DashboardPage.TransactionError),
                //page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 90000, // 90 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            }
            await page.WaitForTimeoutAsync(30000);

            //await page.ClickAndWaitAsync(
            //page.Locator(_locators.DashboardPage.F12Button),
            //page.Locator(_locators.DashboardPage.OK),
            //page.Locator(_locators.DashboardPage.TransactionError),
            //_actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApplicationCoordinator),
            //new LocatorWaitForOptions
            //{
            //    Timeout = 90000, // 90 seconds timeout
            //    State = WaitForSelectorState.Visible
            //}, _outputAccessor.Output);

            string applicationNumber = await page.Locator(_locators.ApplicationPageT062800.ApplicationNumber).InputValueAsync();
            _outputAccessor.Output.WriteLine($"Solicitud: {applicationNumber}");

            // Ingresar los datos de ingresos del cliente si es necesario
            await AssingAdditonalIncomeAsync(page, clientData);

            // Asignar la garantía según el tipo de préstamo si requiere
            await AssingGuaranteeAsync(page, loanApplication, transacion062800Type);

            // Evaluar la solicitud de préstamo y esperamos el resultado de la evaluación, tomando capturas de pantalla en el proceso
            // Consideramos el error en el ambiente de QA, donde el botón de evaluación puede dar 'Error al procesar la consulta'
            await page.ClickAndWaitAsync(
                page.Locator(_locators.ApplicationPageT062800.EvaluateButton),
                page.Locator(_locators.ApplicationPageT062800.OK_ApprovalError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 90 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            bool approvalError = await page.Locator(_locators.ApplicationPageT062800.ApprovalError).CountAsync() == 1;
            if (approvalError)
            {
                await page.ClickAndWaitAsync(
                    page.Locator(_locators.DashboardPage.F7Button),
                    page.Locator(_locators.DashboardPage.OK),
                    new LocatorWaitForOptions
                    {
                        Timeout = 60000, // 90 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
            }

            if (!Enum.TryParse(await page.Locator(_locators.ApplicationPageT062800.EvaluateResult).InputValueAsync(), out EvaluationResult evaluationResult))
                throw new Exception("No se ha podido obtener el resultado de la evaluación correctamente.");
            _outputAccessor.Output.WriteLine($"Resultado de la evaluación: {evaluationResult}");

            // Se modificara el resultado si es necesario
            evaluationResult = await ModifyApplicationResultAsync(page, clientData, applicationNumber, evaluationResult);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "1. Solicitud.jpg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            // Tomar Capturas de pantalla de los resultados de la evaluación
            await GetCalificationResultAsync(page, _locators, loanApplication.EvidenceFoler, _outputAccessor.Output);

            // Tomar capturas de pantalla a los criterios de evaluación de riesgo la solicitud
            await GetCarsResultAsync(page, _locators, loanApplication.EvidenceFoler, _outputAccessor.Output);

            await ApproveAndGetPdfAsync(
                page,
                _locators,
                evaluationResult,
                clientData.RequestState,
                clientData.RequestType,
                _actionCoordinatorFactory,
                loanApplication.EvidenceFoler,
                loanApplication.Headless,
                clientData.RequestObservation1,
                clientData.RequestObservation2);

            List<string> approvingUsers = await GetApprovingUsersAsync(page, _locators, applicationNumber, loanApplication.EvidenceFoler, _outputAccessor.Output);

            // Convertir el PDF a PNG solo si es headless
            if (loanApplication.Headless)
            {
                await GetImgFromPdfDocument(_pdfConverter, loanApplication.EvidenceFoler, loanApplication.KeepPdf, _outputAccessor.Output); // Convertir el PDF del PRT a PNG
            }

            // Realizamos las aprobacion de verificacion domiciliaria y laboral
            await ValidateDocumentationAsync(page, applicationNumber, transacion062800Type); // Verificacion 1
            if (transacion062800Type == Transacion062800Type.Maxiprestamos)
            {
                await ValidateDocumentationAsync(page, applicationNumber, Transacion062800Type.Maxiprestamos); // Verificacion 2 para Maxiprestamos
            }

            return new LoanApplicationResultT062800
            {
                ApplicationNumber = applicationNumber,
                Transacion062800Type = Transacion062800Type.Maxiprestamos,
                EvaluationResult = evaluationResult,
                RecognizedApprovingUsers = approvingUsers
            };
        }
        private async Task<EvaluationResult> ModifyApplicationResultAsync(
            IPage page,
            ClientDataT062800 clientData,
            string applicationNumber,
            EvaluationResult evaluationResult)
        {
            if (clientData.ModifyLoanApplication != ModifyLoanApplication.APROBAR) // por el momento solo implementaremos el caso APROBAR
                return evaluationResult;

            _outputAccessor.Output.WriteLine("Forzando aprobación de la solicitud.");
            await _standardQueryService.ExecuteStandardQueryAsync<ForceLoanApprovalModel>(new ForceLoanApprovalModel
            {
                ApplicationNumber = applicationNumber,
                TIPOSOLICITUDCREDITO = "NOR"
            });
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F7Button),
                page.Locator(_locators.DashboardPage.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            var evalResultStr = await page.Locator(_locators.ApplicationPageT062800.EvaluateResult).InputValueAsync();
            if (!Enum.TryParse(evalResultStr, out EvaluationResult result))
                throw new Exception("No se ha podido obtener el resultado de la evaluación correctamente.");

            return result;
        }
        private async Task ValidateDocumentationAsync(IPage page, string applicationNumber, Transacion062800Type transacion062800Type)
        {
            await page.Locator(_locators.DashboardPage.TransactionInput).FillAsync("064000");
            await page.Locator(_locators.DashboardPage.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApprovalPage.ApplicationNumber).FillAsync(applicationNumber);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F7Button),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 30000 // 30 seconds timeout for the transaction to be processed
                }, _outputAccessor.Output);

            string applicationNumberResult = await page.Locator(_locators.ApprovalPage.ApplicationNumberResult).InputValueAsync();

            if (applicationNumberResult != applicationNumber)
            {
                throw new Exception("El número de aplicación no coincide con el esperado. " +
                                    $"Esperado: {applicationNumber}, " +
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

            await page.Locator(_locators.ApplicationPageT062800.ValidateDocumentationVerification).CheckAsync();
            await page.Locator(_locators.ApplicationPageT062800.ValidateDocumentationType).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062800.ValidateDocumentationTypeElement).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062800.ValidateDocumentationDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            await page.Locator(_locators.ApplicationPageT062800.ValidateDocumentationObservation).FillAsync("QA");
            await page.Locator(_locators.ApplicationPageT062800.ValidateDocumentationResult).SelectOptionAsync("CONFORME");

            await page.WaitForTimeoutAsync(500); // Esperar para asegurar que los cambios se reflejen

            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApprovalCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();
                // Presionar F12 para guardar los cambios
                await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F12Button),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            }
        }
        private async Task AssingAdditonalIncomeAsync(IPage page, ClientDataT062800 clientData)
        {
            if (Convert.ToInt32(clientData.Income) == 0)
                return;

            await page.Locator(_locators.ApplicationPageT062800.IncomeButtton).ClickAsync();
            await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062800.IncomeDate).FillAsync(DateTime.Now.AddMonths(-1).ToString("MMyyyy"));
            await page.Locator(_locators.ApplicationPageT062800.IncomeDate).PressAsync("Enter");

            await page.Locator(_locators.ApplicationPageT062800.IncomeAssets).FillAsync(clientData.Income.ToString());

            await page.Locator(_locators.ApplicationPageT062800.IncomeOther1).FillIfEditableAsync(clientData.Income.ToString());
            await page.Locator(_locators.ApplicationPageT062800.IncomeOther2).FillIfEditableAsync(clientData.Income.ToString());

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
                page.Locator(_locators.ApplicationPageT062800.IncomeReturn),
                page.Locator(_locators.DashboardPage.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }
        private async Task AssingGuaranteeAsync(IPage page, LoanApplicationModel<ClientDataT062800> loanApplication, Transacion062800Type transacion062800Type)
        {
            if (transacion062800Type != Transacion062800Type.Maxiprestamos)
                return;
            if (loanApplication.ClientData.GuaranteeType == GuaranteeType.SinGarantia)
                return;

            await page.Locator(_locators.ApplicationPageT062800.GuaranteeButton).ClickAsync();
            await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            string guaranteeType = loanApplication.ClientData.GuaranteeType switch
            {
                GuaranteeType.GarantiaLiquida => "DEPÓSITOS",
                GuaranteeType.GarantiaNoLiquida => "HIPOTECARIA",
                _ => throw new ArgumentOutOfRangeException(nameof(loanApplication.ClientData.GuaranteeType), "Tipo de garantía no soportado.")
            };

            await page.Locator(_locators.ApplicationPageT062800.GuaranteeType).ClickAsync();
            await page.Locator(_locators.DashboardPage.ListElement(guaranteeType)).ClickAsync();

            await page.Locator(_locators.ApplicationPageT062800.GuaranteeGoodsType).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062800.GuaranteeGoodsTypeElement).ClickAsync();

            await page.Locator(_locators.ApplicationPageT062800.GuaranteeCoinType).SelectOptionAsync(CoinType.Soles.ToString());

            double guaranteeValue = loanApplication.ClientData.LoanAmount * 1.2;
            await page.Locator(_locators.ApplicationPageT062800.GuaranteeTaxAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.ApplicationPageT062800.GuaranteeAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.ApplicationPageT062800.GuaranteeDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            await page.Locator(_locators.ApplicationPageT062800.GuaranteeDescription).FillAsync("QA");

            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F12Button),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            await page.WaitForTimeoutAsync(500);
            await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "7. Garantia.jpg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            await page.ClickAndWaitAsync(
                page.Locator(_locators.ApplicationPageT062800.GuaranteeReturn),
                page.Locator(_locators.DashboardPage.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }

    }
}
