using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.LocatorRepository;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Services;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalBanking
{
    public abstract class LoanApplication<TClientData> : ILoanApplication<TClientData>
        where TClientData : IClientData
    {
        protected readonly ElementRepositoryFixture _locators;
        protected readonly IPdfConverter _pdfConverter;
        protected readonly IStandardQueryService _standardQueryService;
        protected readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        protected readonly ITestOutputAccessor _outputAccessor;

        protected LoanApplication(
            ElementRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        {
            _locators = locators;
            _pdfConverter = pdfConverter;
            _standardQueryService = standardQueryService;
            _actionCoordinatorFactory = actionCoordinatorFactory;
            _outputAccessor = output;
        }
        public abstract Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationModel<TClientData> loanRequest);

        // Agregar métodos comunes para todas las implementaciones de LoanApplication, si es necesario.
        protected async Task GetCalificationResultAsync(IPage page, string evidenceFoler)
        {
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
                Path = Path.Combine(evidenceFoler, "2. Resultado Calificacion 1.jpg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            await page.Locator(_locators.DashboardPage.ViewCarsButton).ScrollIntoViewIfNeededAsync();
            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(evidenceFoler, "2. Resultado Calificacion 2.jpg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });
        }
        protected async Task GetCarsResultAsync(IPage page, string evidenceFoler)
        {
            bool viewCarsButton = await page.Locator(_locators.DashboardPage.ViewCarsButton).IsVisibleAsync();
            if (!viewCarsButton) // Si el botón "Ver Criterios de Aceptación de Riesgos" no está visible, vamos a la sección de resultados de calificación
            {
                await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.CalificationResultSection),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            }

            // Tomamos capturas de pantalla a los criterios de aprobación de crédito
            await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.ViewCarsButton),
                page.Locator(_locators.DashboardPage.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            for (int i = 1; i <= 3; i++)
            {
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = Path.Combine(evidenceFoler, $"3. CARS {i}.jpg"),         // Ruta donde se guarda la imagen
                    FullPage = true               // Captura toda la página, no solo la vista actual
                });

                int scrollOffset = (i) * 250;
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
        }
        protected async Task ApproveAndGetPdfAsync(
            IPage page,
            EvaluationResult evaluationResult,
            RequestStatus requestStatus,
            RequestType requestType,
            string evidenceFoler,
            bool headless,
            string? requestObservation1 = null,
            string? requestObservation2 = null)
        {
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
            await page.Locator(_locators.DashboardPage.RequestState).SelectOptionAsync(requestStatus.ToString());
            await page.Locator(_locators.DashboardPage.RequestComment).FillAsync("QA");

            if (evaluationResult is EvaluationResult.RECHAZADO)
            {
                await page.Locator(_locators.DashboardPage.RequestType).SelectOptionAsync(requestType.GetDescription());

                if (!string.IsNullOrWhiteSpace(requestObservation1))
                {
                    await page.Locator(_locators.DashboardPage.RequestObservation1).ClickAsync();
                    await page.Locator(_locators.DashboardPage.ListElement(requestObservation1)).ClickAsync();
                }
                if (!string.IsNullOrWhiteSpace(requestObservation2))
                {
                    await page.Locator(_locators.DashboardPage.RequestObservation2).ClickAsync();
                    await page.Locator(_locators.DashboardPage.ListElement(requestObservation2)).ClickAsync();
                }
            }
            await page.WaitForTimeoutAsync(500); // Esperar un segundo para asegurar que los cambios se reflejen

            // Iniciamos la espera por el popup del PDF
            var pdfpageTask = page.Context.WaitForPageAsync(new BrowserContextWaitForPageOptions
            {
                Timeout = 0  // espera indefinidamente por el popup
            });

            // Presionamos F12 para generar el PDF de aprobación
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApprovalCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync(); // Esperar a que el coordinador permita continuar

                await page.ClickAndWaitAsync(
                page.Locator(_locators.DashboardPage.F12Button),
                page.Locator(_locators.DashboardPage.OK),
                page.Locator(_locators.DashboardPage.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 90000,
                    State = WaitForSelectorState.Visible
                },
                _outputAccessor.Output);
            }

            // Esperamos a que se abra el popup del PDF
            var pdfpage = await pdfpageTask;

            // Tomamos una captura de pantalla de la página del PDF si no es headless
            // En el caso headless, no se puede tomar screenshot de un PDF directamente, se guardara el PDF en la carpeta de evidencia
            if (!headless)
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
                        Path = Path.Combine(evidenceFoler, $"4. PRT 0{i + 1}.jpg"),
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
                Path = Path.Combine(evidenceFoler, $"5. Aprobacion.jpg"),
                FullPage = true
            });
        }
        protected async Task<List<string>> GetApprovingUsersAsync(IPage page, string applicationNumber, string evidenceFolder)
        {
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
                Path = Path.Combine(evidenceFolder, $"6. Consulta.jpg"),
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
                    _outputAccessor.Output?.WriteLine($"Usuario {i + 1}: {user}");
                }
            }

            return approvingUsers;
        }
        protected async Task GetImgFromPdfDocument(string evidenceFolder, bool keepPdf)
        {
            string pdfFile = await WaitForFileWithExtensionAsync(extension: "", evidenceFolder, TimeSpan.FromSeconds(90), TimeSpan.FromMilliseconds(100));

            // Renombrar el archivo descargado
            string currentFullPath = Path.Combine(evidenceFolder, pdfFile);
            string newFullPath = Path.Combine(evidenceFolder, "PRT.pdf");

            // Cambiar el nombre del archivo descargado, sobreescribir si existe
            if (File.Exists(newFullPath))
            {
                File.Delete(newFullPath);
            }
            File.Move(currentFullPath, newFullPath);

            await _pdfConverter.ConvertAllPagesToPngAsync(newFullPath, "4. PRT", evidenceFolder, 300);

            if (keepPdf)
            {
                _outputAccessor.Output?.WriteLine($"PDF guardado en: {newFullPath}");
            }
            else
            {
                // Eliminar el PDF si no se necesita
                if (File.Exists(newFullPath))
                {
                    File.Delete(newFullPath);
                    _outputAccessor.Output?.WriteLine($"PDF eliminado: {newFullPath}");
                }
            }
        }
        protected async Task<EvaluationResult> ModifyApplicationResultAsync(
            IPage page,
            ModifyLoanApplication modifyLoanApplication,
            string applicationNumber,
            EvaluationResult firstEvaluationResult)
        {
            if (modifyLoanApplication == ModifyLoanApplication.Default) // por el momento solo implementaremos el caso APROBAR
                return firstEvaluationResult;

            _outputAccessor.Output.WriteLine("Forzando aprobación de la solicitud.");

            Task<DataTable> queryTask = modifyLoanApplication switch
            {
                ModifyLoanApplication.APROBAR =>
                    _standardQueryService.ExecuteStandardQueryAsync<ForceLoanApprovalModel>(
                        new ForceLoanApprovalModel
                        {
                            ApplicationNumber = applicationNumber,
                            TIPOSOLICITUDCREDITO = "NOR"
                        }
                    ),
                ModifyLoanApplication.ESPECIAL =>
                    _standardQueryService.ExecuteStandardQueryAsync<ForceLoanApprovalModel>(
                        new ForceLoanApprovalModel
                        {
                            ApplicationNumber = applicationNumber,
                            TIPOSOLICITUDCREDITO = "ESP"
                        }
                    ),
                ModifyLoanApplication.RECHAZAR =>
                    _standardQueryService.ExecuteStandardQueryAsync<ForceOnlyCarsEssentialModel>(
                        new ForceOnlyCarsEssentialModel
                        {
                            ApplicationNumber = applicationNumber,
                        }
                    ),
                _ => throw new InvalidOperationException(
                        $"Operación no soportada: {modifyLoanApplication}")
            };

            DataTable resultTable = await queryTask;

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
        protected async Task SearchProduct(IPage page, string product, string? group = null, CoinType? coin = null)
        {
            const string pattern = "**/proc/lv";
            var output = _outputAccessor.Output; // capturo aquí, en el mismo contexto
            Action<IRoute> handler = null!;

            handler = async route =>
            {
                // 1) Leer y decodificar el POST body
                var raw = route.Request.PostData;    // e.g. "_contexto=lv&_lv=%7B...%7D"
                var decoded = WebUtility.UrlDecode(raw);

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
                    var obj = fieldNode.AsObject();
                    if (obj["alias"]?.GetValue<string>() == "TPP")
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
        protected async Task<string> WaitForFileWithExtensionAsync(
            string extension,
            string folderPath,
            TimeSpan timeout,
            TimeSpan pollInterval,
            CancellationToken cancellationToken = default)
        {
            // Calculate the deadline for the timeout
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                // Throw if cancellation is requested
                cancellationToken.ThrowIfCancellationRequested();

                // Find the first file with the given extension in the folder
                var filePath = Directory.EnumerateFiles(folderPath)
                    .FirstOrDefault(f => string.Equals(Path.GetExtension(f), extension, StringComparison.OrdinalIgnoreCase));

                if (filePath != null)
                {
                    // Return only the file name if found
                    return Path.GetFileName(filePath);
                }

                // Wait for the specified poll interval (cancellable)
                await Task.Delay(pollInterval, cancellationToken);
            }
            // Timeout expired without finding a file
            throw new TimeoutException($"No file with extension '{extension}' was found in '{folderPath}' within {timeout}.");
        }
    }
}
