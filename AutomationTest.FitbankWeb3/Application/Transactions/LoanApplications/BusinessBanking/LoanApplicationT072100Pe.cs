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
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.PersonalBanking;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;
using PdfSharpCore.Pdf;
using Spire.Doc;
using Spire.Xls;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.BusinessBanking
{
    public class LoanApplicationT072100Pe : ILoanApplication<ClientDataT072100Pe>
    {
        private readonly LocatorRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly ITestOutputAccessor _outputAccessor;

        private static readonly Random _rnd = new Random();

        public LoanApplicationT072100Pe(
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        {
            this._locators = locators;
            this._pdfConverter = pdfConverter;
            this._standardQueryService = standardQueryService;
            this._actionCoordinatorFactory = actionCoordinatorFactory;
            this._outputAccessor = output;
        }
        public async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT072100Pe> loanApplication)
        {
            ClientDataT072100Pe clientData = loanApplication.ClientData;

            //await SearchProduct(page, clientData.Product); // Buscar el producto de forma rapida en la lista de productos

            // Ingresar a la página de inicio de sesión de Fitbank
            await page.GotoAsync($"{loanApplication.IpPort}/WEB3/ingreso.html");

            // Ingresar las credenciales del usuario
            await page.Locator(_locators.LocatorsLogin.UsernameInput).FillAsync(clientData.UserRequest);
            await page.Locator(_locators.LocatorsLogin.PasswordInput).FillAsync("fitbank123");
            await page.Locator(_locators.LocatorsLogin.SubmitButton).ClickAsync();

            // Ingresamos a la transacción T062100 para propuesta de credito
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).FillAsync("072100");
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionInput).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.FormCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingresamos la identificación del cliente y una dirección válida
            await page.Locator(_locators.LocatorsT072100Pe.Identification).FillAsync(clientData.Identification);
            await page.Locator(_locators.LocatorsT072100Pe.Identification).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 1500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Varificar que el entorno haya cargado correctamente
            bool isLoaded = await page.Locator(_locators.LocatorsT072100Pe.CreditDataLabel).IsVisibleAsync();
            if (!isLoaded)
            {
                // Si no se cargó la página, intentamos nuevamente
                await page.Locator(_locators.LocatorsGeneralDashboard.F7Button).ClickAsync();
                await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible
                });
            }

            // Obtener el tipo de propuesta
            string creditProposalString = await page.Locator(_locators.LocatorsT072100Pe.CreditProposal).InputValueAsync();
            if (!Enum.TryParse<SmallBusinessType>(creditProposalString.Replace('ñ', 'n').Replace(" ", ""), ignoreCase: true, out SmallBusinessType creditProposal))
                throw new ArgumentException("El tipo de segmento del cliente no coincide con ningun tipo de Pequena Empresa");

            _outputAccessor.Output.WriteLine($"Tipo de segmento del cliente: {creditProposal}");

            await page.Locator(_locators.LocatorsT072100Pe.AdressList).ClickAsync();
            await page.Locator(_locators.LocatorsT072100Pe.AddressElement).ClickAsync();

            // Ingresamos el tipo de cliente y  persona de contacto
            await page.Locator(_locators.LocatorsT072100Pe.ClientType).SelectOptionAsync(clientData.ClientType.GetDescription());

            await page.Locator(_locators.LocatorsT072100Pe.ContactList).ClickAsync();
            await page.Locator(_locators.LocatorsT072100Pe.ContactElement).ClickAsync();

            // No es necesario el Rating cliente ni operacion

            // Seleccionamos el producto y tipo de préstamo
            await page.Locator(_locators.LocatorsT072100Pe.ProductList).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(clientData.Product)).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.Locator(_locators.LocatorsT072100Pe.LoanTypeList).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(clientData.LoanType.ToString())).ClickAsync();

            // Seleccionamos Seguro Desgravamen
            await page.Locator(_locators.LocatorsT072100Pe.DesgravamentType).SelectOptionAsync(InsuranceType.Propio.GetDescription());
            await page.Locator(_locators.LocatorsT072100Pe.DesgravamentClass).SelectOptionAsync(InsuranceType.Individual.GetDescription());
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingreso de monto y plazo del préstamo
            await page.Locator(_locators.LocatorsT072100Pe.LoanAmount).FillAsync(clientData.LoanAmount.ToString());
            await page.Locator(_locators.LocatorsT072100Pe.Identification).PressAsync("Enter");

            await page.Locator(_locators.LocatorsT072100Pe.LoanInstallments).FillSimulatedAsync(clientData.LoanInstallments.ToString(), "Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingreso dia de pago fijo (aleatorio entre 1 y 30)
            int payday = _rnd.Next(1, 31);

            await page.Locator(_locators.LocatorsT072100Pe.Payday).FillAsync(payday.ToString());
            await page.Locator(_locators.LocatorsT072100Pe.Identification).PressAsync("Enter");

            // Abono a cuenta
            await SelectDisbursementType(page, clientData.DisbursementType);

            // Lugar de desembolso

            await DisturbementPlaceInterceptor(page);
            await page.Locator(_locators.LocatorsT072100Pe.DisturbementPlaceList).ClickAsync();
            await page.Locator(_locators.LocatorsT072100Pe.DisturbementPlaceElement).ClickAsync();

            // Ingreso de Riesgo maximo de grupo
            double rmg = clientData.LoanAmount * 1.2; // 120% del monto solicitado
            await page.Locator(_locators.LocatorsT072100Pe.Rmg).FillAsync(rmg.ToString());

            // Ingreso de riesgo de cliente
            string clienRisk = await page.Locator(_locators.LocatorsT072100Pe.RiskType).InputValueAsync();
            if (string.IsNullOrWhiteSpace(clienRisk) || clienRisk == "0")
            {
                await page.Locator(_locators.LocatorsT072100Pe.RiskType).SelectOptionAsync(clientData.RiskType.ToString());
            }

            await page.WaitForTimeoutAsync(500); // Esperar medio segundo para asegurar que los cambios se reflejen
            using (var handle = _actionCoordinatorFactory.GetCoordinator(ActionCoordinatorType.LoanApplicationCoordinator).CreateHandle())
            {
                await handle.WaitForTurnAsync();

                await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                    page.Locator(_locators.LocatorsGeneralDashboard.OK),
                    page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                    new LocatorWaitForOptions
                    {
                        Timeout = 60000, // 60 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
            }

            string applicationNumber = await page.Locator(_locators.LocatorsT072100Pe.ApplicationNumber).InputValueAsync();
            _outputAccessor.Output.WriteLine($"Solicitud: {applicationNumber}");

            await AssingGuaranteeAsync(page, loanApplication);

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "1. Solicitud.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

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

            await ModifyApplicationResultAsync(page, clientData.ModifyLoanApplication, applicationNumber);

            // Obtenemos el resultado del CARS
            if (!Enum.TryParse(await page.Locator(_locators.LocatorsBusinessBankingDashboard.EvaluationResult).InputValueAsync(), out EvaluationResult evaluationResult))
                throw new Exception("No se ha podido obtener el resultado de la evaluación correctamente.");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "2. CARS.jpeg"),         // Ruta donde se guarda la imagen
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

            if (!loanApplication.Headless)
            {
                for (int pdfIndex = 0; pdfIndex < pdfPages.Count; pdfIndex++) // Iteramos sobre cada PDF capturado
                {
                    IPage pdfPage = pdfPages[pdfIndex];

                    await pdfPage.BringToFrontAsync(); // Traemos al frente la el pdf

                    // Esperamos a que la url cargue completamente
                    await pdfPage.WaitForURLAsync(
                        url => url.Contains("/WEB3/proc/rep/") && url.Contains("directDownload"),
                        new PageWaitForURLOptions { Timeout = 60000 }
                    );

                    // Refresca la pagina del PDF para asegurarse de que se cargue correctamente (algunos PDF's se quedan en negro)
                    await pdfPage.GotoAsync(pdfPage.Url, new PageGotoOptions
                    {
                        Timeout = 60000,
                        WaitUntil = WaitUntilState.NetworkIdle
                    });

                    // Espera a que el PDF sea scrollable y toma capturas de pantalla
                    await pdfPage.Locator("body > embed").HoverAsync();
                    await pdfPage.Mouse.WheelAsync(0, 0);

                    for (int i = 0; i < 4; i++)
                    {
                        await pdfPage.ScreenshotAsync(new PageScreenshotOptions
                        {
                            Path = Path.Combine(loanApplication.EvidenceFoler, $"3. Documento {pdfIndex + 1}_{i + 1:D2}.jpeg"),
                            FullPage = true
                        });
                        // Desplazar hacia abajo para capturar más contenido si es necesario
                        await pdfPage.Locator("body > embed").HoverAsync();
                        await pdfPage.Mouse.WheelAsync(0, 400);
                    }

                    await pdfPage.CloseAsync();
                }

                // Traemos la página principal al frente para continuar con el flujo
                await page.BringToFrontAsync();
            }

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

            string firstOption = "APROBADO";
            string secondOption = "OBSERVADO";
            if (approvalStatusElements.Contains(firstOption))
            {
                await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(firstOption)).ClickAsync();
            }
            else
            {
                await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(secondOption)).ClickAsync();
            }

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
                Path = Path.Combine(loanApplication.EvidenceFoler, $"4. Consulta.jpeg"),
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

            // Convertir los PDF's a JPEG solo si es headless
            if (loanApplication.Headless)
            {
                await GetImgFromPdfDocument(loanApplication.EvidenceFoler, loanApplication.KeepPdf); // Convertir el PDF del PRT a PNG
            }

            return new LoanApplicationResultT072100Pe
            {
                ApplicationNumber = applicationNumber,
                EvaluationResult = evaluationResult,
                RecognizedApprovingUsers = usersList
            };
        }
        protected async Task GetImgFromPdfDocument(string evidenceFolder, bool keepPdf)
        {
            List<string> pdfFiles = await WaitForFilesWithExtensionAsync(extension: "", evidenceFolder, TimeSpan.FromSeconds(90), TimeSpan.FromMilliseconds(100));

            if (pdfFiles.Count == 0)
                throw new Exception("No se encontraron archivos PDF en la carpeta de evidencia.");

            int index = 1;
            foreach (var pdfFile in pdfFiles)
            {
                string currentFullPath = Path.Combine(evidenceFolder, pdfFile);

                // Renombrar el archivo a Documento_1.pdf, Documento_1_2.pdf, etc.
                string newFileName = pdfFiles.Count == 1 ? "Documento.pdf" : $"Documento_{index}.pdf";
                string newFullPath = Path.Combine(evidenceFolder, newFileName);

                // Sobreescribir si existe
                if (File.Exists(newFullPath))
                    File.Delete(newFullPath);

                File.Move(currentFullPath, newFullPath);

                // Convertir PDF a imágenes
                await _pdfConverter.ConvertAllPagesToImgAsync(newFullPath, $"4. Documento {index}_", evidenceFolder, 1800);

                if (keepPdf)
                {
                    _outputAccessor.Output?.WriteLine($"PDF guardado en: {newFullPath}");
                }
                else
                {
                    File.Delete(newFullPath);
                    _outputAccessor.Output?.WriteLine($"PDF eliminado: {newFullPath}");
                }

                index++;
            }
        }
        protected async Task<List<string>> WaitForFilesWithExtensionAsync(
            string extension,
            string folderPath,
            TimeSpan timeout,
            TimeSpan pollInterval,
            CancellationToken cancellationToken = default)
        {
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Buscar todos los archivos con la extensión dada
                var matchingFiles = Directory.EnumerateFiles(folderPath)
                    .Where(f => string.Equals(Path.GetExtension(f), extension, StringComparison.OrdinalIgnoreCase))
                    .Select(f => Path.GetFileName(f)!)
                    .ToList();

                if (matchingFiles.Any())
                {
                    return matchingFiles;
                }

                await Task.Delay(pollInterval, cancellationToken);
            }

            throw new TimeoutException($"No files with extension '{extension}' were found in '{folderPath}' within {timeout}.");
        }
        protected async Task ModifyApplicationResultAsync(
            IPage page,
            ModifyLoanApplication modifyLoanApplication,
            string applicationNumber)
        {
            if (modifyLoanApplication == ModifyLoanApplication.Default)
                return;

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
                page.Locator(_locators.LocatorsGeneralDashboard.F7Button),
                page.Locator(_locators.LocatorsGeneralDashboard.OK_TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
        }
        private async Task AssingGuaranteeAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT072100Pe> loanApplication)
        {
            if (loanApplication.ClientData.GuaranteeType == GuaranteeType.SinGarantia)
                return;

            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeButton).ClickAsync();
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

            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeType).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(guaranteeType)).ClickAsync();

            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeGoodsType).ClickAsync();
            await page.WaitForTimeoutAsync(500); // Esperar medio segundo para asegurar que los cambios se reflejen
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeGoodsTypeElement).ClickAsync();

            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeCoinType).SelectOptionAsync(CoinType.Soles.ToString());

            double guaranteeValue = loanApplication.ClientData.LoanAmount * 1.2;
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeTaxAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeAmount).FillAsync(guaranteeValue.ToString());
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeDescription).FillAsync("QA");

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            await page.WaitForTimeoutAsync(500);
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFoler, "5. Garantia.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsT072100Pe.GuaranteeReturn),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);

            // Varificar que el checkbox segmento bien esté seleccionado
            await page.Locator(_locators.LocatorsT072100Pe.SegmentBox).CheckAsync(new LocatorCheckOptions
            {
                Force = true // Forzar el checkeo del checkbox
            });

            // Presionar F12 para guardar los cambios
            await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                    page.Locator(_locators.LocatorsGeneralDashboard.OK),
                    page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                    new LocatorWaitForOptions
                    {
                        Timeout = 60000, // 60 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
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
        private async Task DisturbementPlaceInterceptor(IPage page)
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

        private async Task SelectDisbursementType(IPage page, DisbursementType disbursementType)
        {
            await page.Locator(_locators.LocatorsT072100Pe.DisbursementType).SelectOptionAsync(disbursementType.GetDescription());

            if (disbursementType != DisbursementType.AbonoACuenta)
                return;

            // Solo para “Abono a cuenta” intentamos elegir la cuenta
            await page.Locator(_locators.LocatorsT072100Pe.BankAccountList).ClickAsync();

            Task isAvailable = page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
            Task accountError = page.Locator(_locators.LocatorsT072100Pe.BankAccountError).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            Task result = await Task.WhenAny(isAvailable, accountError);

            if (result == accountError) // Si se muestra el error de cuenta se selecciona el tipo de desembolso "Orden de Pago" y se continua
            {
                await page.Locator(_locators.LocatorsT072100Pe.DisbursementType).SelectOptionAsync(DisbursementType.OrdenDePago.GetDescription());
                return;
            }

            // Si hay al menos una cuenta selecionarmos la primera
            await page.Locator(_locators.LocatorsT072100Pe.BankAccountElement).ClickAsync();

            return;
        }
    }
}
