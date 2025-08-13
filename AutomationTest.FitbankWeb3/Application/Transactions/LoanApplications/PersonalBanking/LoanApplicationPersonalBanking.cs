using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Web;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalBanking
{
    public abstract class LoanApplicationPersonalBanking<TClientData> : LoanApplicationBase<TClientData>
        where TClientData : IClientData
    {
        public LoanApplicationPersonalBanking(
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        : base(locators, pdfConverter, standardQueryService, actionCoordinatorFactory, output)
        { }
        public abstract override Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<TClientData> loanRequest);

        // Agregar métodos comunes para todas las implementaciones de LoanApplication, si es necesario.
        protected async Task GetCalificationResultAsync(IPage page, string evidenceFoler)
        {
            // Tomamos capturas de pantalla del resultado de la calificación
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsPersonalBankingDashboard.CalificationResultSection),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(evidenceFoler, "2. Resultado Calificacion 1.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            await page.Locator(_locators.LocatorsPersonalBankingDashboard.ViewCarsButton).ScrollIntoViewIfNeededAsync();
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(evidenceFoler, "2. Resultado Calificacion 2.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });
        }
        protected async Task GetCarsResultAsync(IPage page, string evidenceFoler)
        {
            bool viewCarsButton = await page.Locator(_locators.LocatorsPersonalBankingDashboard.ViewCarsButton).IsVisibleAsync();
            if (!viewCarsButton) // Si el botón "Ver Criterios de Aceptación de Riesgos" no está visible, vamos a la sección de resultados de calificación
            {
                await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsPersonalBankingDashboard.CalificationResultSection),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            }

            // Tomamos capturas de pantalla a los criterios de aprobación de crédito
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsPersonalBankingDashboard.ViewCarsButton),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            for (int i = 1; i <= 3; i++)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = Path.Combine(evidenceFoler, $"3. CARS {i}.jpeg"),         // Ruta donde se guarda la imagen
                    FullPage = true               // Captura toda la página, no solo la vista actual
                });

                int scrollOffset = (i) * 250;
                await page.Locator(_locators.LocatorsPersonalBankingDashboard.CarsTable).EvaluateAsync($"el => el.scrollTop = {scrollOffset}");
            }

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsPersonalBankingDashboard.CarsReturn),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }
        protected async Task ApproveAndGetPdfAsync(
            IPage page,
            EvaluationResult evaluationResult,
            RequestStatus requestStatus,
            RequestType requestType,
            string evidenceFolder,
            bool headless,
            string? requestObservation1 = null,
            string? requestObservation2 = null)
        {
            // Ingresamos a la sección de aprobación de la solicitud
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsPersonalBankingDashboard.ApprovalSection),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            // Ingresamos el estado de la solicitud, el comentario y las observaciones si es necesario
            await page.Locator(_locators.LocatorsPersonalBankingDashboard.RequestState).SelectOptionAsync(requestStatus.ToString());
            await page.Locator(_locators.LocatorsPersonalBankingDashboard.RequestComment).FillAsync("QA");

            if (evaluationResult is EvaluationResult.RECHAZADO)
            {
                await page.Locator(_locators.LocatorsPersonalBankingDashboard.RequestType).SelectOptionAsync(requestType.GetDescription());

                if (!string.IsNullOrWhiteSpace(requestObservation1))
                {
                    await page.Locator(_locators.LocatorsPersonalBankingDashboard.RequestObservation1).ClickAsync();
                    await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(requestObservation1)).ClickAsync();
                }
                if (!string.IsNullOrWhiteSpace(requestObservation2))
                {
                    await page.Locator(_locators.LocatorsPersonalBankingDashboard.RequestObservation2).ClickAsync();
                    await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(requestObservation2)).ClickAsync();
                }
            }
            await page.WaitForTimeoutAsync(500); // Esperar un segundo para asegurar que los cambios se reflejen

            // Iniciamos la espera por el popup del PDF
            var pdfPages = new List<IPage>();
            var isCollecting = true;

            page.Context.Page += (sender, newPage) => // Configurar listener para capturar TODOS los PDFs que aparezcan
            {
                if (isCollecting)
                {
                    pdfPages.Add(newPage);
                }
            };

            // Presionamos F12 para generar el PDF de aprobación
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApprovalCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync(); // Esperar a que el coordinador permita continuar

                await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 90000,
                    State = WaitForSelectorState.Visible
                },
                _outputAccessor.Output);
            }

            // Terminado el proceso de aprobación, dejamos de capturar PDFs
            isCollecting = false;

            _outputAccessor.Output.WriteLine($"Acción completada. Total PDFs capturados: {pdfPages.Count}");

            // Tomamos una captura de pantalla de la página del PDF si no es headless
            // En el caso headless, no se puede tomar screenshot de un PDF directamente, se guardara el PDF en la carpeta de evidencia
            if (!headless)
            {

                await TakeScreenshotPdfsAsync(pdfPages, evidenceFolder, "4. Documento", numberOfScreenshots: 3);

                // Traemos la página principal al frente para continuar con el flujo
                await page.BringToFrontAsync();
            }

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(evidenceFolder, $"5. Aprobacion.jpeg"),
                FullPage = true
            });
        }
        protected async Task<List<string>> GetApprovingUsersAsync(IPage page, string applicationNumber, string evidenceFolder)
        {
            // Vamos a consultar la solicitud aprobada y verificar los usuarios aprobadores
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("064060");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsPersonalBankingDashboard.ApplicationNumberSearchUsers).FillAsync(applicationNumber);
            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F7Button),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsPersonalBankingDashboard.ApprovalUsersButton),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(evidenceFolder, $"6. Consulta.jpeg"),
                FullPage = true
            });

            // Extraer los usuarios aprobadores de la tabla
            ILocator usersTable = page.Locator(_locators.LocatorsPersonalBankingDashboard.AprovalUsersList);
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
                    _outputAccessor.Output?.WriteLine($"Usuario {i + 1}: {user}");
                }
            }

            return approvingUsers;
        }
        protected async Task SearchProduct(IPage page, string product, string? group = null, CoinType? coin = null)
        {
            const string pattern = "**/proc/lv";
            var output = _outputAccessor.Output; // capturo aquí, en el mismo contexto
            Action<IRoute> handler = null!;

            handler = async route =>
            {
                // 1) Leer y decodificar el POST body
                var raw = route.Request.PostData;    // e.g. "_contexto=lv&_lv=%7B...%7D"
                var decoded = WebUtility.UrlDecode(raw) ?? string.Empty;

                if (string.IsNullOrEmpty(decoded) || !decoded.Contains("_lv="))
                {
                    await route.ContinueAsync();
                    return;
                }

                // 2) Separar parámetros y extraer JSON de _lv
                var nvc = HttpUtility.ParseQueryString(decoded);
                var prefix = decoded.Substring(0, decoded.IndexOf("_lv=") + "_lv=".Length);
                string lvJson = nvc["_lv"]!;

                // 3) Parsear a JsonNode y localizar el array "fields"
                var rootNode = JsonNode.Parse(lvJson)!.AsObject();
                var fieldsArray = rootNode["fields"]!.AsArray();

                bool modified = false;

                // 4) Cambiar únicamente los objetos con "alias":"TPP"
                foreach (var fieldNode in fieldsArray)
                {
                    var obj = fieldNode?.AsObject();
                    if (obj?["alias"]?.GetValue<string>() == "TPP")
                    {
                        modified = true;
                    }
                }

                if (modified)
                {
                    var productoField = fieldsArray
                            .FirstOrDefault(node => node!["title"]?.GetValue<string>() == "PRODUCTO")
                            ?.AsObject();

                    if (productoField is not null)
                    {
                        productoField["value"] = product;
                        output.WriteLine("Producto modificado a: " + product);
                    }

                    if (group is not null)
                    {
                        var grupoField = fieldsArray
                            .FirstOrDefault(node => node!["title"]?.GetValue<string>() == "GRUPO PRODUCTO")
                            ?.AsObject();
                        if (grupoField is not null)
                        {
                            grupoField["value"] = group;
                            output.WriteLine("Grupo modificado a: " + group);
                        }
                    }

                    if (coin is not null)
                    {
                        var monedaField = fieldsArray
                            .FirstOrDefault(node => node!["title"]?.GetValue<string>() == "MONEDA")
                            ?.AsObject();
                        if (monedaField is not null)
                        {
                            monedaField["value"] = coin.GetDescription();
                            output.WriteLine("Moneda modificada a: " + coin);
                        }
                    }

                    // 5) Serializar el JSON modificado sin saltos de línea
                    var newLvJson = rootNode.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });

                    // 6) Reconstruir el body completo y re-encode
                    var rebuilt = prefix + WebUtility.UrlEncode(newLvJson);

                    // 7) Convertir a bytes y reenviar
                    var bytes = Encoding.UTF8.GetBytes(rebuilt);

                    await route.ContinueAsync(new RouteContinueOptions
                    {
                        Headers = route.Request.Headers,
                        PostData = bytes
                    });

                    // Unroute after first modification
                    await page.UnrouteAsync(pattern, handler);
                }
                else
                {
                    // Continue without modification if not matched
                    await route.ContinueAsync();
                }
            };

            await page.RouteAsync(pattern, handler);
        }
    }
}
