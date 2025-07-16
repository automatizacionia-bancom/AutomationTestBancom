using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;
using PdfSharpCore.Pdf;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.BusinessBanking
{
    public abstract class LoanApplicationBusinessBanking<TClientData> : LoanApplicationBase<TClientData> 
        where TClientData : IClientData
    {
        public LoanApplicationBusinessBanking(
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        : base(locators, pdfConverter, standardQueryService, actionCoordinatorFactory, output)
        { }

        public abstract override Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<TClientData> loanRequest);
        protected async Task<EvaluationResult> EvaluationAndGetPdfAsync(
           IPage page,
           string applicationNumber,
           ModifyLoanApplication modifyLoanApplication,
           string evidenceFolder,
           bool headless)
        {
            // Ingreso a la sección de aprobaciones
            await page.Locator(_locators.LocatorsBusinessBankingDashboard.CarsSectionCredit).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });


            // Aprobacion de CARS
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApprovalCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.Locator(_locators.LocatorsGeneralDashboard.F12Button).ClickAsync();

                string selector = _locators.LocatorsBusinessBankingDashboard.ExecuteProcessState;
                string expected = "Proceso Ejecutado";
                await page.WaitForFunctionAsync(
                    $@"() => {{
                        const el = document.querySelector(""{selector}"");
                        return el && el.value.includes(""{expected}"");
                    }}",
                    new PageWaitForFunctionOptions { Timeout = 90_000 }
                );

                await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible
                });
            }

            await ModifyApplicationResultAsync(page, modifyLoanApplication, applicationNumber);

            // Obtenemos el resultado del CARS
            if (!Enum.TryParse(await page.Locator(_locators.LocatorsBusinessBankingDashboard.EvaluationResult).InputValueAsync(), out EvaluationResult evaluationResult))
                throw new Exception("No se ha podido obtener el resultado de la evaluación correctamente.");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(evidenceFolder, "2. CARS.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

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

            // Hacemos click vara ver los resultados de CARS
            await page.Locator(_locators.LocatorsBusinessBankingDashboard.CarsReportButton).ClickAsync();
            await page.WaitForTimeoutAsync(1000); // Damos un tiempo de un segundo para que el fitbank genere todos los PDF's

            isCollecting = false; // Dejamos de capturar PDF's

            _outputAccessor.Output.WriteLine($"Acción completada. Total PDFs capturados: {pdfPages.Count}");

            if (!headless)
            {
                await TakeScreenshotPdfsAsync(pdfPages, evidenceFolder, "3. Documento", numberOfScreenshots: 4);

                // Traemos la página principal al frente para continuar con el flujo
                await page.BringToFrontAsync();
            }

            return evaluationResult;
        }
        protected async Task<List<string>> ApproveAndGetUsersAsync(
            IPage page,
            string evidenceFolder)
        {
            await page.Locator(_locators.LocatorsBusinessBankingDashboard.ApprovalSection).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

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

            // Elige la primera que esté presente en approvalStatusElements o, de lo contrario, la última
            var selected = options.FirstOrDefault(opt => approvalStatusElements.Contains(opt))
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
                    page.Locator(_locators.LocatorsGeneralDashboard.OK),
                    page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                    new LocatorWaitForOptions
                    {
                        Timeout = 90000, // 90 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
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
                Path = Path.Combine(evidenceFolder, $"4. Consulta.jpeg"),
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

            return usersList;
        }
        protected async Task<List<string>> GetFirstColumnAsync(IPage page, string tbodySelector)
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
        protected async Task DisturbementPlaceInterceptor(IPage page)
        {
            bool hasModified = false;
            string pattern = "**/proc/lv";
            var output = _outputAccessor.Output;

            await page.RouteAsync(pattern, async route =>
            {
                // Si ya se modificó una vez, continuar sin cambios
                if (hasModified)
                {
                    await route.ContinueAsync();
                    return;
                }

                try
                {
                    // Obtener y decodificar el cuerpo de la petición
                    var raw = route.Request.PostData;
                    var decoded = WebUtility.UrlDecode(raw) ?? string.Empty;
                    if (string.IsNullOrEmpty(decoded) || !decoded.Contains("_lv="))
                    {
                        await route.ContinueAsync();
                        return;
                    }

                    // Separar parámetros y extraer JSON de _lv
                    var nvc = HttpUtility.ParseQueryString(decoded);
                    var prefix = decoded.Substring(0, decoded.IndexOf("_lv=") + "_lv=".Length);
                    string lvJson = nvc["_lv"]!;

                    // Parsear el JSON dentro del campo _lv
                    var lvJsonNode = JsonNode.Parse(lvJson);
                    if (lvJsonNode == null)
                    {
                        await route.ContinueAsync();
                        return;
                    }

                    // Verificar si tiene references con alias "TS"
                    var references = lvJsonNode["references"]?.AsArray();
                    if (references == null)
                    {
                        await route.ContinueAsync();
                        return;
                    }

                    bool hasValidReference = false;
                    foreach (var reference in references)
                    {
                        if (reference?["alias"]?.ToString() == "TS")
                        {
                            hasValidReference = true;
                            break;
                        }
                    }

                    if (!hasValidReference)
                    {
                        await route.ContinueAsync();
                        return;
                    }

                    // Modificar el valor del campo con elementName "txtCodAgencia"
                    var fields = lvJsonNode["fields"]?.AsArray();
                    if (fields != null)
                    {
                        foreach (var field in fields)
                        {
                            if (field?["elementName"]?.ToString() == "txtCodAgencia")
                            {
                                field["value"] = "45";
                                break;
                            }
                        }
                    }

                    // Serializar el JSON modificado sin saltos de línea
                    var newLvJson = lvJsonNode.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });

                    // Reconstruir el body completo y re-encode
                    var rebuilt = prefix + WebUtility.UrlEncode(newLvJson);

                    // Convertir a bytes y reenviar
                    var bytes = Encoding.UTF8.GetBytes(rebuilt);

                    // Marcar como modificado para no volver a hacerlo
                    hasModified = true;

                    // Continuar con la petición modificada
                    await route.ContinueAsync(new RouteContinueOptions
                    {
                        Headers = route.Request.Headers,
                        PostData = bytes
                    });
                }
                catch (Exception ex)
                {
                    // En caso de error, continuar sin modificaciones
                    output.WriteLine($"Error al interceptar petición: {ex.Message}");
                    await route.ContinueAsync();
                }
            });
        }
    }
}
