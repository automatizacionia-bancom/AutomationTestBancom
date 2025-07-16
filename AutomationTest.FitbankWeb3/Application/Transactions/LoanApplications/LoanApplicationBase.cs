using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
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

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications
{
    public abstract class LoanApplicationBase<TClientData> : ILoanApplication<TClientData>
        where TClientData : IClientData
    {
        protected readonly LocatorRepositoryFixture _locators;
        protected readonly IPdfConverter _pdfConverter;
        protected readonly IStandardQueryService _standardQueryService;
        protected readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        protected readonly ITestOutputAccessor _outputAccessor;

        protected LoanApplicationBase(
            LocatorRepositoryFixture locators,
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

        public abstract Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<TClientData> loanRequest);
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
        protected async Task TakeScreenshotPdfsAsync(
            List<IPage> pdfPages,
            string evidenceFolder,
            string prefix,
            int numberOfScreenshots)
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

                for (int i = 0; i < numberOfScreenshots; i++)
                {
                    await pdfPage.ScreenshotAsync(new PageScreenshotOptions
                    {
                        Path = Path.Combine(evidenceFolder, $"{prefix} {pdfIndex + 1}_{i + 1:D2}.jpeg"),
                        FullPage = true
                    });
                    // Desplazar hacia abajo para capturar más contenido si es necesario
                    await pdfPage.Locator("body > embed").HoverAsync();
                    await pdfPage.Mouse.WheelAsync(0, 400);
                }

                await pdfPage.CloseAsync();
            }
        }
        protected async Task GetImgFromPdfDocument(LoanApplicationWorkflowModel<TClientData> loanRequest, string sufix)
        {
            if (!loanRequest.Headless) // Si no es headless, no se procesan PDFs a imagenes
                return;

            List<string> pdfFiles = await WaitForFilesWithExtensionAsync(extension: "", loanRequest.EvidenceFolder, TimeSpan.FromSeconds(90), TimeSpan.FromMilliseconds(100));

            if (pdfFiles.Count == 0)
                throw new Exception("No se encontraron archivos PDF en la carpeta de evidencia.");

            int index = 1;
            foreach (var pdfFile in pdfFiles)
            {
                string currentFullPath = Path.Combine(loanRequest.EvidenceFolder, pdfFile);

                // Renombrar el archivo a Documento_1.pdf, Documento_1_2.pdf, etc.
                string newFileName = pdfFiles.Count == 1 ? "Documento.pdf" : $"Documento_{index}.pdf";
                string newFullPath = Path.Combine(loanRequest.EvidenceFolder, newFileName);

                // Sobreescribir si existe
                if (File.Exists(newFullPath))
                    File.Delete(newFullPath);

                File.Move(currentFullPath, newFullPath);

                // Convertir PDF a imágenes
                await _pdfConverter.ConvertAllPagesToImgAsync(newFullPath, $"{sufix} {index}_", loanRequest.EvidenceFolder, 1800);

                if (loanRequest.KeepPdf)
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
    }
}
