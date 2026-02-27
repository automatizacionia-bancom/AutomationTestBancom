using System.Globalization;
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
    public class LoanApplicationT072100Pe : LoanApplicationBusinessBanking<ClientDataT072100Pe>
    {
        private static readonly Random _rnd = new Random();
        public LoanApplicationT072100Pe(
            LocatorRepositoryFixture locators,
            IPdfConverter pdfConverter,
            IStandardQueryService standardQueryService,
            IActionCoordinatorFactory actionCoordinatorFactory,
            ITestOutputAccessor output)
        : base(locators, pdfConverter, standardQueryService, actionCoordinatorFactory, output)
        { }
        public override async Task<ILoanApplicationResult> ApplyForLoanAsync(IPage page, LoanApplicationWorkflowModel<ClientDataT072100Pe> loanApplication)
        {
            ClientDataT072100Pe clientData = loanApplication.ClientData;

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
            //string creditProposalString = await page.Locator(_locators.LocatorsT072100Pe.CreditProposal).InputValueAsync();
            //if (!Enum.TryParse<SmallBusinessType>(creditProposalString.Replace('ñ', 'n').Replace(" ", ""), ignoreCase: true, out SmallBusinessType creditProposal))
            //    throw new ArgumentException("El tipo de segmento del cliente no coincide con ningun tipo de Pequena Empresa");

            //_outputAccessor.Output.WriteLine($"Tipo de segmento del cliente: {creditProposal}");

            await page.Locator(_locators.LocatorsT072100Pe.AdressList).ClickAsync();
            await page.Locator(_locators.LocatorsT072100Pe.AddressElement).ClickAsync();

            // Ingresamos el tipo de cliente y  persona de contacto
            await page.Locator(_locators.LocatorsT072100Pe.ClientType).SelectOptionAsync(clientData.ClientType.GetDescription());
            //await page.PauseAsync();
            //await page.Locator(_locators.LocatorsT072100Pe.ContactList).ClickAsync();
            //await page.Locator(_locators.LocatorsT072100Pe.ContactElement).ClickAsync();

            // No es necesario el Rating cliente ni operacion

            // Seleccionamos el producto y tipo de préstamo
            await page.Locator(_locators.LocatorsT072100Pe.ProductList).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.ListElement(clientData.Product)).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Selecionar tipo de prestamo
            await page.Locator(_locators.LocatorsT072100Pe.LoanTypeList).SelectOptionAsync(clientData.LoanType.ToString().ToUpper());

            // Seleccionamos Seguro Desgravamen
            await page.Locator(_locators.LocatorsT072100Pe.DesgravamentType).SelectOptionAsync(InsuranceType.Propio.GetDescription());
            await page.Locator(_locators.LocatorsT072100Pe.DesgravamentClass).SelectOptionAsync(InsuranceType.Individual.GetDescription());
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            // Ingreso de monto y plazo del préstamo
            _outputAccessor.Output.WriteLine($"Riesgo maximo: {clientData.LoanAmount}");

            await page.Locator(_locators.LocatorsT072100Pe.LoanAmount).FillAsync(clientData.LoanAmount.ToString(CultureInfo.InvariantCulture));
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
            await DisturbementPlaceInterceptor(page, "45");
            await page.Locator(_locators.LocatorsT072100Pe.DisturbementPlaceList).ClickAsync();
            await page.Locator(_locators.LocatorsT072100Pe.DisturbementPlaceElement).ClickAsync();

            // Ingreso de Riesgo maximo de grupo
            double rmg = clientData.LoanAmount * 1.0; // 120% del monto solicitado
            _outputAccessor.Output.WriteLine($"Riesgo maximo de grupo: {rmg}");
            await page.Locator(_locators.LocatorsT072100Pe.Rmg).FillAsync(rmg.ToString(CultureInfo.InvariantCulture));

            // Ingreso de riesgo de cliente
            if (!await page.Locator(_locators.LocatorsT072100Pe.RiskType).IsDisabledAsync())
            {
                string clienRisk = await page.Locator(_locators.LocatorsT072100Pe.RiskType).InputValueAsync();
                if (string.IsNullOrWhiteSpace(clienRisk) || clienRisk == "0")
                {
                    await page.Locator(_locators.LocatorsT072100Pe.RiskType).SelectOptionAsync(clientData.RiskType.ToString());
                }
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

            await AssingReLoan(page, loanApplication);

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

            return new LoanApplicationResultT072100Pe
            {
                ApplicationNumber = applicationNumber,
                EvaluationResult = evaluationResult,
                RecognizedApprovingUsers = usersList
            };
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
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeTaxAmount).FillAsync(guaranteeValue.ToString(CultureInfo.InvariantCulture));
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeAmount).FillAsync(guaranteeValue.ToString(CultureInfo.InvariantCulture));
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeDate).FillAsync(DateTime.Now.ToString("dd/MM/yyyy"));
            await page.Locator(_locators.LocatorsT072100Pe.GuaranteeDescription).FillAsync("QA");

            await page.WaitForTimeoutAsync(500);
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
                Path = Path.Combine(loanApplication.EvidenceFolder, "5. Garantia.jpeg"),         // Ruta donde se guarda la imagen
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
        private async Task AssingReLoan(IPage page, LoanApplicationWorkflowModel<ClientDataT072100Pe> loanApplication)
        {
            if (!loanApplication.ClientData.LoanType.Equals(LoanType.Represtamo))
                return;

            await page.Locator(_locators.LocatorsT072100Pe.ReLoanButton).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await SeleccionarCompraSegunUltimaCuotaAsync(page);
            //await page.WaitForTimeoutAsync(10000);
            await page.WaitForTimeoutAsync(500);

            // Presionar F12 para guardar los cambios
            await page.ClickAndWaitAsync(
                    page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                    page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect),
                    page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                    new LocatorWaitForOptions
                    {
                        Timeout = 60000, // 60 seconds timeout
                        State = WaitForSelectorState.Visible
                    }, _outputAccessor.Output);
            await page.Locator(_locators.LocatorsGeneralDashboard.TransactionCorrect).WaitForAsync(delayBefore: 500, new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(loanApplication.EvidenceFolder, "5. Represtamo.jpeg"),         // Ruta donde se guarda la imagen
                FullPage = true               // Captura toda la página, no solo la vista actual
            });

            await page.Locator(_locators.LocatorsT072100Pe.ReLoanButtonReturn).ClickAsync();
            await page.Locator(_locators.LocatorsGeneralDashboard.OK).WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible
            });

            await page.ClickAndWaitAsync(
                page.Locator(_locators.LocatorsGeneralDashboard.F12Button),
                page.Locator(_locators.LocatorsGeneralDashboard.OK),
                page.Locator(_locators.LocatorsGeneralDashboard.TransactionError),
                new LocatorWaitForOptions
                {
                    Timeout = 60000, // 60 seconds timeout
                    State = WaitForSelectorState.Visible
                }, _outputAccessor.Output);
            //await page.PauseAsync();
        }
        private async Task SeleccionarCompraSegunUltimaCuotaAsync(IPage page)
        {
            int ultimoIndiceValido = -1;

            // Asumimos que los índices son consecutivos desde 0
            for (int i = 0; i < 100; i++)
            {
                var cuota = page.Locator($"#c_txtCuota_{i}");

                // Si ya no existe el input, salimos del loop
                if (await cuota.CountAsync() == 0)
                    break;

                var valor = (await cuota.InputValueAsync()).Trim();

                if (valor != "0.00")
                {
                    ultimoIndiceValido = i;
                    break;
                }
            }

            if (ultimoIndiceValido >= 0)
            {
                await page.CheckAsync($"#c_ckCompra_{ultimoIndiceValido}");
            }
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
