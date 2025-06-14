using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Services.ActionCoordination;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalLoan;
using AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications
{
    public class LoanApplicationT062900 : LoanApplication<ClientDataT062900>
    {
        private readonly ElementRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly ITestOutputAccessor _outputAccessor;

        public LoanApplicationT062900(ElementRepositoryFixture locators, IPdfConverter pdfConverter, IStandardQueryService standardQueryService, IActionCoordinatorFactory actionCoordinatorFactory, ITestOutputAccessor output)
        {
            _locators = locators;
            _pdfConverter = pdfConverter;
            _standardQueryService = standardQueryService;
            _actionCoordinatorFactory = actionCoordinatorFactory;
            _outputAccessor = output;
        }
        public override async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationModel<ClientDataT062900> loanApplication)
        {
            ClientDataT062900 clientData = loanApplication.ClientData;

            // Verificar que el usuario no tenga una sesión activa
            await _standardQueryService.ExecuteStandardQueryAsync<DeleteUserSesionModel>(new DeleteUserSesionModel
            {
                User = clientData.UserRequest
            });

            await SearchProduct(page, clientData.Product); // Buscar el producto de forma rapida en la lista de productos

            await page.GotoAsync($"{loanApplication.IpPort}/WEB3/ingreso.html");

            // Ingresar las credenciales del usuario
            await page.Locator(_locators.Login.UsernameInput).FillAsync(clientData.UserRequest);
            await page.Locator(_locators.Login.PasswordInput).FillAsync("fitbank123");
            await page.Locator(_locators.Login.SubmitButton).ClickAsync();

            // Ingresamos a la transacción T062900 para convenios PNP
            await page.Locator(_locators.DashboardPage.TransactionInput).FillAsync("062900");
            await page.Locator(_locators.DashboardPage.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingresamos la identificación del cliente y una dirección válida
            await page.Locator(_locators.ApplicationPageT062900.Identification).FillAsync(clientData.Identification);
            await page.Locator(_locators.ApplicationPageT062900.Identification).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062900.AdressList).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062900.AddressElement).ClickAsync();

            // Seleccionamos el producto, gestor y tipo de préstamo
            await page.Locator(_locators.ApplicationPageT062900.ProductList).ClickAsync();
            await page.Locator(_locators.DashboardPage.ListElement(clientData.Product)).ClickAsync();
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062900.ManagerList).ClickAsync();
            await page.Locator(_locators.ApplicationPageT062900.ManagerElement).ClickAsync();

            await page.Locator(_locators.ApplicationPageT062900.LoanTypeList).ClickAsync();
            await page.Locator(_locators.DashboardPage.ListElement(clientData.LoanType.ToString())).ClickAsync();

            // Ingresamos el monto del préstamo y las cuotas
            await page.Locator(_locators.ApplicationPageT062900.LoanAmount).FillAsync(clientData.LoanAmount.ToString());
            await page.Locator(_locators.ApplicationPageT062900.LoanAmount).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.ApplicationPageT062900.LoanInstallments).FillAsync(clientData.LoanInstallments.ToString());
            await page.Locator(_locators.ApplicationPageT062900.LoanInstallments).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Verificar que se haya generado correctamente la tasa de interés
            string loanRate = await page.Locator(_locators.ApplicationPageT062900.LoanRate).InputValueAsync();
            if (string.IsNullOrWhiteSpace(loanRate))
                Assert.Fail("No se ha generado correctamente la tasa de la solicitud");

            // Seleccionar la fuente de planilla
            await page.Locator(_locators.ApplicationPageT062900.PayrollSource).SelectOptionAsync(clientData.PayrollSource);

            // Crear la solicitud de préstamo y esperar a que se genere el número de solicitud
            await page.ClickAndWaitAsync(
            page.Locator(_locators.DashboardPage.F12Button),
            page.Locator(_locators.DashboardPage.OK),
            page.Locator(_locators.DashboardPage.TransactionError),
            _actionCoordinatorFactory.GetCoordinator(Enums.ActionCoordinatorType.LoanApplicationCoordinator),
            new LocatorWaitForOptions
            {
                Timeout = 90000, // 90 seconds timeout
                State = WaitForSelectorState.Visible
            }, _outputAccessor.Output);

            string applicationNumber = await page.Locator(_locators.ApplicationPageT062900.ApplicationNumber).InputValueAsync();
            _outputAccessor.Output.WriteLine($"Solicitud: {applicationNumber}");

            // Ingresar los datos de ingresos del cliente si es necesario
            if (Convert.ToInt32(clientData.Income) != 0)
            {
                await page.Locator(_locators.ApplicationPageT062900.IncomeButtton).ClickAsync();
                await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible
                });

                await page.Locator(_locators.ApplicationPageT062900.IncomeDate).FillIfEditableAsync(DateTime.Now.AddMonths(-1).ToString("MMyyyy"));
                await page.Locator(_locators.ApplicationPageT062900.IncomeDate).PressAsync("Enter");

                await page.Locator(_locators.ApplicationPageT062900.IncomeAssets).FillIfEditableAsync(clientData.Income.ToString());
                await page.Locator(_locators.ApplicationPageT062900.IncomeAssets).PressAsync("Enter");

                await page.Locator(_locators.ApplicationPageT062900.IncomeOther).FillAsync(clientData.Income.ToString());
                await page.Locator(_locators.ApplicationPageT062900.IncomeOther).PressAsync("Enter");

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
                    page.Locator(_locators.ApplicationPageT062900.IncomeReturn),
                    page.Locator(_locators.DashboardPage.OK),
                    new LocatorWaitForOptions
                    {
                        Timeout = 60000, // 60 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
            }

            // Evaluar la solicitud de préstamo y esperamos el resultado de la evaluación, tomando capturas de pantalla en el proceso
            await page.ClickAndWaitAsync(
                page.Locator(_locators.ApplicationPageT062900.EvaluateButton),
                page.Locator(_locators.DashboardPage.OK),
                page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 90000, // 90 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            if (!Enum.TryParse<EvaluationResult>(await page.Locator(_locators.ApplicationPageT062900.EvaluateResult).InputValueAsync(), out EvaluationResult evaluationResult))
                throw new Exception("No se ha podido obtener el resultado de la evaluación correctamente.");
            _outputAccessor.Output.WriteLine($"Resultado de la evaluación: {evaluationResult}");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "1. Solicitud.jpg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            // Tomamos capturas de pantalla del resultado de la calificación
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.CalificationResultSection),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "2. Resultado Calificacion 1.jpg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            await page.Locator(_locators.DashboardPage.ViewCarsButton).ScrollIntoViewIfNeededAsync();
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "2. Resultado Calificacion 2.jpg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            // Tomamos capturas de pantalla a los criterios de aprobación de crédito
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.ViewCarsButton),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            for (int i = 0; i < 3; i++)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = Path.Combine(loanApplication.EvidenceFoler, $"3. CARS {i + 1}.jpg"),         // Ruta donde se guarda la imagen
                    FullPage = true               // Captura toda la página, no solo la vista actual
                });

                int scrollOffset = (i + 1) * 250;
                await page.Locator(_locators.DashboardPage.CarsTable).EvaluateAsync($"el => el.scrollTop = {scrollOffset}");
            }

            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.CarsReturn),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            // Ingresamos a la sección de aprobación de la solicitud
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.ApprovalSection),
                page.Locator(_locators.DashboardPage.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            // Ingresamos el estado de la solicitud, el comentario y las observaciones si es necesario
            await page.Locator(_locators.DashboardPage.RequestState).SelectOptionAsync(clientData.RequestState.ToString());
            await page.Locator(_locators.DashboardPage.RequestComment).FillAsync("QA");

            if (evaluationResult is EvaluationResult.RECHAZADO)
            {
                await page.Locator(_locators.DashboardPage.RequestType).SelectOptionAsync(clientData.RequestType.GetDescription());

                if (!string.IsNullOrWhiteSpace(clientData.RequestObservation1))
                {
                    await page.Locator(_locators.DashboardPage.RequestObservation1).ClickAsync();
                    await page.Locator(_locators.DashboardPage.ListElement(clientData.RequestObservation1)).ClickAsync();
                }
                if (!string.IsNullOrWhiteSpace(clientData.RequestObservation2))
                {
                    await page.Locator(_locators.DashboardPage.RequestObservation2).ClickAsync();
                    await page.Locator(_locators.DashboardPage.ListElement(clientData.RequestObservation2)).ClickAsync();
                }
            }
            await page.WaitForTimeoutAsync(1000); // Esperar un segundo para asegurar que los cambios se reflejen

            // Iniciamos la espera por el popup del PDF
            var pdfpageTask = page.Context.WaitForPageAsync(new BrowserContextWaitForPageOptions
            {
                Timeout = 0  // espera indefinidamente por el popup
            });

            // Presionamos F12 para generar el PDF de aprobación
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F12Button),
                page.Locator(_locators.DashboardPage.OK),
                page.Locator(_locators.DashboardPage.TransactionError),
                _actionCoordinatorFactory.GetCoordinator(Enums.ActionCoordinatorType.LoanApprovalCoordinator),
                new LocatorWaitForOptions
                {
                    Timeout = 90000,
                    State = WaitForSelectorState.Visible
                },
                _outputAccessor.Output
            );

            // Esperamos a que se abra el popup del PDF
            var pdfpage = await pdfpageTask;

            // Tomamos una captura de pantalla de la página del PDF si no es headless
            if (!loanApplication.Headless)
            {
                await pdfpage.WaitForURLAsync(
                    url => url.Contains("/WEB3/proc/rep/") && url.Contains("directDownload"),
                    new PageWaitForURLOptions { Timeout = 60000 }
                );

                await pdfpage.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await pdfpage.WaitForTimeoutAsync(1000);

                for (int i = 0; i < 3; i++)
                {
                    await pdfpage.ScreenshotAsync(new PageScreenshotOptions
                    {
                        Path = Path.Combine(loanApplication.EvidenceFoler, $"4. PRT 0{i + 1}.jpg"),
                        FullPage = true
                    });
                    // Desplazar hacia abajo para capturar más contenido si es necesario
                    await pdfpage.Locator("body > embed").HoverAsync();
                    await pdfpage.Mouse.WheelAsync(0, 400);
                }

                await pdfpage.CloseAsync();
            }

            // Traemos la página principal al frente para continuar con el flujo
            await page.BringToFrontAsync();

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, $"5. Aprobacion.jpg"),
                FullPage = true
            });

            // Vamos a consultar la solicitud aprobada y verificar los usuarios aprobadores
            await page.Locator(_locators.DashboardPage.TransactionInput).FillAsync("064060");
            await page.Locator(_locators.DashboardPage.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.DashboardPage.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.DashboardPage.ApplicationNumberSearch).FillAsync(applicationNumber);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F7Button),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.ApprovalUsers),
                page.Locator(_locators.DashboardPage.OK),
                page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, $"6. Consulta.jpg"),
                FullPage = true
            });

            // Extraer los usuarios aprobadores de la tabla
            ILocator usersTable = page.Locator(_locators.DashboardPage.AprovalUsersList);
            var rows = usersTable.Locator("tbody > tr");
            int rowCount = await rows.CountAsync();
            List<string> approvingUsers = new();

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

            // Convertir el PDF a PNG si es headless
            if (loanApplication.Headless)
            {
                string pdfFile = await WaitForFileWithExtensionAsync("", loanApplication.EvidenceFoler, TimeSpan.FromSeconds(60), TimeSpan.FromMilliseconds(100));

                // Renombrar el archivo descargado
                string currentFullPath = Path.Combine(loanApplication.EvidenceFoler, pdfFile);
                string newFullPath = Path.Combine(loanApplication.EvidenceFoler, "PRT.pdf");

                // Cambiar el nombre del archivo descargado, sobreescribir si existe
                if (File.Exists(newFullPath))
                {
                    File.Delete(newFullPath);
                }
                File.Move(currentFullPath, newFullPath);

                await _pdfConverter.ConvertAllPagesToPngAsync(newFullPath, "4. PRT", loanApplication.EvidenceFoler, 300);

                if (loanApplication.KeepPdf)
                {
                    _outputAccessor.Output.WriteLine($"PDF guardado en: {newFullPath}");
                }
                else
                {
                    // Eliminar el PDF si no se necesita
                    if (File.Exists(newFullPath))
                    {
                        File.Delete(newFullPath);
                        _outputAccessor.Output.WriteLine($"PDF eliminado: {newFullPath}");
                    }
                }
            }

            return new LoanApplicationResultT062900
            {
                ApplicationNumber = applicationNumber,
                EvaluationResult = evaluationResult,
                RecognizedApprovingUsers = approvingUsers
            };
        }
    }
}
