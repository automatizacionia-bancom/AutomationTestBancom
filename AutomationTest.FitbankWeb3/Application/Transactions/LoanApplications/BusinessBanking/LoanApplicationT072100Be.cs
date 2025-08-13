using System.Data;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications.BusinessBanking
{
    public class LoanApplicationT072100Be : LoanApplicationBusinessBanking<ClientDataT072100Be>
    {
        private static readonly Random _rnd = new Random();
        public LoanApplicationT072100Be(
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        : base(locators, pdfConverter, standardQueryService, actionCoordinatorFactory, output)
        { }
        public override async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT072100Be> loanApplication)
        {
            ClientDataT072100Be clientData = loanApplication.ClientData;

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
            await page.Locator(_locators.LocatorsT072100Be.Identification).FillAsync(clientData.Identification);
            await page.Locator(_locators.LocatorsT072100Be.Identification).PressAsync("Enter");
            await page.Locator(_locators.LocatorsGeneralDashboard.FormProcessing).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden
            });

            // Varificar que el entorno haya cargado correctamente
            bool isLoaded = await page.Locator(_locators.LocatorsT072100Be.ShareholdersLabel).IsVisibleAsync();
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
            string creditProposalString = await page.Locator(_locators.LocatorsT072100Be.CreditProposal).InputValueAsync();

            // Clean up the string to remove special characters before parsing
            string cleanedCreditProposalString = new string(creditProposalString.Where(char.IsLetterOrDigit).ToArray());
            if (!Enum.TryParse<BankBusinessType>(cleanedCreditProposalString, ignoreCase: true, out BankBusinessType creditProposal))
                creditProposal = BankBusinessType.Default;

            _outputAccessor.Output.WriteLine($"Tipo de segmento del cliente: {creditProposal}");

            await page.Locator(_locators.LocatorsT072100Be.AdressList).ClickAsync();
            await page.Locator(_locators.LocatorsT072100Be.AddressElement).ClickAsync();

            // Ingresamos el tipo de cliente y  persona de contacto
            await page.Locator(_locators.LocatorsT072100Be.ClientType).SelectOptionAsync(clientData.ClientType.GetDescription());

            await page.Locator(_locators.LocatorsT072100Be.ContactList).ClickAsync();
            await page.Locator(_locators.LocatorsT072100Be.ContactElement).ClickAsync();

            // Ingresamos el Rating cliente
            await AssingClientRatingAsync(page);

            // Ingreso de Riesgo maximo
            await page.Locator(_locators.LocatorsT072100Be.Rma).FillAsync(clientData.RMG.ToString());

            // Ingreso de Riesgo maximo de grupo
            double rmg = clientData.RMG * 1.2; // 120% del monto solicitado
            await page.Locator(_locators.LocatorsT072100Be.Rmg).FillAsync(rmg.ToString());

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

            string applicationNumber = await page.Locator(_locators.LocatorsT072100Be.ApplicationNumber).InputValueAsync();
            _outputAccessor.Output.WriteLine($"Solicitud: {applicationNumber}");

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFolder, "1. Solicitud.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            // Evaluar la solicitud y obtener el PDF
            EvaluationResult evaluationResult = await EvaluationAndGetPdfAsync(
                page,
                applicationNumber,
                clientData.ModifyLoanApplication,
                loanApplication.EvidenceFolder,
                loanApplication.Headless);

            // Aprobamos la solicitud y obtenemos los usuarios aprobadores
            List<string> usersList = await ApproveAndGetUsersAsync(page, loanApplication.EvidenceFolder);

            // Convertir los PDF's a JPEG solo si es headless
            if (loanApplication.Headless)
            {
                await GetImgFromPdfDocument(loanApplication, "3. Documento CARS"); // Convertir el PDF del PRT a JPEG
            }

            return new LoanApplicationResultT072100Be
            {
                ApplicationNumber = applicationNumber,
                EvaluationResult = evaluationResult,
                RecognizedApprovingUsers = usersList
            };
        }
        private async Task AssingClientRatingAsync(IPage page)
        {
            await page.Locator(_locators.LocatorsT072100Be.ClientRating).ClickAsync();

            List<int> ratingValues = new List<int> { 12, 12, 14, 7, 6, 6, 4, 4, 3, 5, 4, 3, 10, 10 }; // Listamos los valores de los rating

            for (int i = 0; i < ratingValues.Count; i++)
            {
                string ratingLocator = $"{_locators.LocatorsT072100Be.RatingElementBase}{i}";

                // Seleccionamos el rating de cliente
                await page.Locator(ratingLocator).FillAsync(ratingValues[i].ToString());

                // Presionamos Enter para guardar el rating en el ultimo campo
                if (i == ratingValues.Count - 1)
                {
                    await page.Locator(ratingLocator).PressAsync("Enter");
                }
            }

            await page.Locator(_locators.LocatorsT072100Be.RatingWindowsClose).ClickAsync(); // Cerramos la ventana

            await page.WaitForTimeoutAsync(500); // Esperar medio segundo para asegurar que los cambios se reflejen

            string ratingValue = await page.Locator(_locators.LocatorsT072100Be.ClientRatingResult).InputValueAsync(); // Obtenemos el resultado del rating del cliente

            await page.Locator(_locators.LocatorsT072100Be.OperationRatingResult).FillSimulatedAsync(ratingValue, "Enter"); // Asignamos el rating del cliente al rating de operación
        }
    }
}
