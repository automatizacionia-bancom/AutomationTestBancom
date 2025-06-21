using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Extensions;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Output;
using AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Input;
using AutomationTest.FitbankWeb3.Application.Models.OrchestratorsModels;
using AutomationTest.FitbankWeb3.Application.Models.TransactionModels;
using AutomationTest.FitbankWeb3.Application.Services;
using AutomationTest.FitbankWeb3.Application.Services.ActionCoordination;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications;
using AutomationTest.FitbankWeb3.Application.Transactions.LoanApprovals;
using AutomationTest.FitbankWeb3.Application.Transactions.Orchestrators;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Meziantou.Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace AutomationTest.FitbankWeb3.Tests
{
    public class MainTest : IClassFixture<TestFixture>
    {
        private readonly IServiceProvider _provider;
        private readonly IServiceScope _scope;
        private readonly PlaywrightFixture _playwright;
        private readonly ElementRepositoryFixture _locators;
        private readonly IPdfConverter _pdfConverter;
        private readonly IStandardQueryService _standardQueryService;
        private readonly ITransactionUsersSelectionService _transactionUsersSelectionService;
        private readonly IActionCoordinatorFactory _actionCoordinatorFactory;
        private readonly IBranchSynchronizationService _branchSynchronizationService;
        private readonly IUserTurnCoordinatorService _userTurnCoordinatorService;
        private readonly ITestOutputHelper _output;

        public MainTest(TestFixture fixture, ITestOutputHelper output)
        {
            var accessor = fixture.ServiceProvider
               .GetRequiredService<ITestOutputAccessor>();
            accessor.Set(output);

            _provider = fixture.ServiceProvider;
            // 1) Cada clase de test abre su propio scope para aislar servicios Scoped/ Transient.
            _scope = fixture.ServiceProvider.CreateScope();

            // 2) Resolvemos aquí los servicios que necesitemos
            _playwright = _scope.ServiceProvider.GetRequiredService<PlaywrightFixture>();
            _locators = _scope.ServiceProvider.GetRequiredService<ElementRepositoryFixture>();
            _pdfConverter = _scope.ServiceProvider.GetRequiredService<IPdfConverter>();
            _standardQueryService = _scope.ServiceProvider.GetRequiredService<IStandardQueryService>();
            _transactionUsersSelectionService = _scope.ServiceProvider.GetRequiredService<ITransactionUsersSelectionService>();
            _actionCoordinatorFactory = _scope.ServiceProvider.GetRequiredService<IActionCoordinatorFactory>();
            _branchSynchronizationService = _scope.ServiceProvider.GetRequiredService<IBranchSynchronizationService>();
            _userTurnCoordinatorService = _scope.ServiceProvider.GetRequiredService<IUserTurnCoordinatorService>();

            _output = output;
        }
        // Holds all test data in a list for scalability
        //private static readonly List<LoanApplicationModel<ClientDataT062900>> ClientDataList = new()
        //{
        //    new LoanApplicationModel<ClientDataT062900>
        //    {
        //        Headless = false,
        //        KeepPdf = false,
        //        ClientData = new ClientDataT062900
        //        {
        //            UserRequest = "NGONZALES",
        //            Identification = "09607112",
        //            Address = 1,
        //            LoanAmount = 15000.00,
        //            LoanType = LoanType.Prestamo,
        //            LoanInstallments = 72,
        //            PayrollSource = "Dir/Of. Economia",
        //            Product = "159",
        //            Income = 0.00,
        //            RequestType = RequestType.IngresoARiesgos,
        //            RequestState = RequestStatus.APROBAR,
        //            RequestObservation1 = "SUPERVISADOS",
        //            RequestObservation2 = "OTROS",
        //        } ,
        //        IpPort = "http://10.0.2.54:8380",
        //        EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1"
        //    },
        //    //new LoanRequestModel<ClientDataT062900>
        //    //{
        //    //     ClientData = new ClientDataT062900
        //    //    {
        //    //    UserRequest = "NGONZALES",
        //    //    Identification = "16681272",
        //    //    Address = "1",
        //    //    LoanAmount = 15000.00,
        //    //    LoanType = LoanType.Prestamo,
        //    //    LoanInstallments = 72,
        //    //    PayrollSource = "Dir/Of. Economia",
        //    //    Product = "159",
        //    //    Income = 20000.00,
        //    //    RequestType = RequestType.IngresoARiesgos,
        //    //    RequestState = RequestState.APROBAR,
        //    //    RequestObservation1 = "SUPERVISADOS",
        //    //    RequestObservation2 = "OTROS",
        //    //    },
        //    //    EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso2"
        //    //},

        //};
        //private static readonly List<LoanApprovalModel> ClientDataList = new()
        //{
        //    new LoanApprovalModel
        //    {
        //        ApplicationNumber = "1245202963",
        //        ApprovalNumber = 1,
        //        ApprovingUser = "FOMORALES",
        //        EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1",
        //        IpPort = "http://10.0.2.54:8380",
        //    },
        //    new LoanApprovalModel
        //    {
        //        ApplicationNumber = "1245202964",
        //        ApprovalNumber = 1,
        //        ApprovingUser = "FOMORALES",
        //        EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1",
        //        IpPort = "http://10.0.2.54:8380",
        //    },
        //    new LoanApprovalModel
        //    {
        //        ApplicationNumber = "1245202965",
        //        ApprovalNumber = 1,
        //        ApprovingUser = "FOMORALES",
        //        EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1",
        //        IpPort = "http://10.0.2.54:8380",
        //    }
        //};
        private static readonly List<FullLoanRequest<ClientDataT062900>> ClientDataList = new()
        {
            //new FullLoanRequest<ClientDataT062900>
            //{
            //    ClientData = new ClientDataT062900
            //    {
            //        UserRequest = "ILADINES",
            //        Identification = "09607112",
            //        Address = 1,
            //        Product = "101",
            //        LoanType = LoanType.Prestamo,
            //        LoanInstallments = 72,
            //        LoanAmount = 15000.00,
            //        PayrollSource = "Dir/Of. Economia",
            //        Income = 5000.0,
            //        ModifyLoanApplication = ModifyLoanApplication.APROBAR,
            //        RequestType = RequestType.IngresoARiesgos,
            //        RequestState = RequestStatus.APROBAR,
            //        RequestObservation1 = "SUPERVISADOS",
            //        RequestObservation2 = "OTROS",
            //    },
            //    IpPort = "http://10.0.2.54:8380",
            //    EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1",
            //    Headless = true,
            //    KeepPdf = false,
            //    MaxApprovalUser = 10,
            //},
             new FullLoanRequest<ClientDataT062900>
            {
                ClientData = new ClientDataT062900
                {
                    UserRequest = "ILADINES",
                    Identification = "73754741",
                    Address = 1,
                    Product = "101",
                    LoanType = LoanType.Prestamo,
                    LoanInstallments = 72,
                    LoanAmount = 30000.00,
                    PayrollSource = PayrollSourceType.DireccionEconomia,
                    Income = 5000.0,
                    ModifyLoanApplication = ModifyLoanApplication.Default,
                    RequestType = RequestType.IngresoARiesgos,
                    RequestState = RequestStatus.APROBAR,
                    RequestObservation1 = "SUPERVISADOS",
                    RequestObservation2 = "OTROS",
                },
                IpPort = "http://10.0.2.54:8380",
                EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso2",
                Headless = true,
                KeepPdf = false,
                MaxApprovalUser = 10,
            },
             new FullLoanRequest<ClientDataT062900>
            {
                ClientData = new ClientDataT062900
                {
                    UserRequest = "ILADINES",
                    Identification = "20029263",
                    Address = 1,
                    Product = "101",
                    LoanType = LoanType.Prestamo,
                    LoanInstallments = 72,
                    LoanAmount = 95000.00,
                    PayrollSource = PayrollSourceType.DireccionEconomia,
                    Income = 5000.0,
                    ModifyLoanApplication = ModifyLoanApplication.Default,
                    RequestType = RequestType.IngresoARiesgos,
                    RequestState = RequestStatus.APROBAR,
                    RequestObservation1 = "SUPERVISADOS",
                    RequestObservation2 = "OTROS",
                },
                IpPort = "http://10.0.2.54:8380",
                EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso3",
                Headless = true,
                KeepPdf = false,
                MaxApprovalUser = 10,
            },
            //new FullLoanRequest<ClientDataT062900>
            //{
            //    ClientData = new ClientDataT062900
            //    {
            //        UserRequest = "ILADINES",
            //        GuaranteeType = GuaranteeType.GarantiaLiquida,
            //        Identification = "07859785",
            //        Address = 1,
            //        BillingCycle = BillingCycle.PimeraQuincena,
            //        CreditLine = 8000.00,
            //        Product = "001",
            //        Income = 5000.0,
            //        ModifyLoanApplication = ModifyLoanApplication.APROBAR,
            //        RequestType = RequestType.IngresoARiesgos,
            //        RequestState = RequestStatus.APROBAR,
            //        RequestObservation1 = "SUPERVISADOS",
            //        RequestObservation2 = "OTROS",
            //    },
            //    IpPort = "http://10.0.2.54:8380",
            //    EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso3",
            //    Headless = false,
            //    KeepPdf = false,
            //    MaxApprovalUser = 10,
            //},
            //new FullLoanRequest<ClientDataT062900>
            //{
            //    ClientData = new ClientDataT062900
            //    {
            //        UserRequest = "ILADINES",
            //        GuaranteeType = GuaranteeType.GarantiaLiquida,
            //        Identification = "25591463",
            //        Address = 1,
            //        BillingCycle = BillingCycle.PimeraQuincena,
            //        CreditLine = 8000.00,
            //        Product = "001",
            //        Income = 5000.0,
            //        ModifyLoanApplication = ModifyLoanApplication.Default,
            //        RequestType = RequestType.IngresoARiesgos,
            //        RequestState = RequestStatus.APROBAR,
            //        RequestObservation1 = "SUPERVISADOS",
            //        RequestObservation2 = "OTROS",
            //    },
            //    IpPort = "http://10.0.2.54:8380",
            //    EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso4",
            //    Headless = false,
            //    KeepPdf = false,
            //    MaxApprovalUser = 10,
            //},
            // new FullLoanRequest<ClientDataT062900>
            //{
            //    ClientData = new ClientDataT062900
            //    {
            //        UserRequest = "ILADINES",
            //        Identification = "07821002",
            //        Address = 1,
            //        Product = "001",
            //        JewelEmbeddedWeight = 50,
            //        JewelGrossWeight = 150,
            //        PaymentTerm = PaymentTerm.Monthly,
            //        RequestedAmount = 8000.00,
            //        Income = 5000.0,
            //        ModifyLoanApplication = ModifyLoanApplication.APROBAR,
            //        RequestType = RequestType.IngresoARiesgos,
            //        RequestState = RequestStatus.APROBAR,
            //        RequestObservation1 = "SUPERVISADOS",
            //        RequestObservation2 = "OTROS",
            //    },
            //    IpPort = "http://10.0.2.54:8380",
            //    EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso5",
            //    Headless = false,
            //    KeepPdf = false,
            //    MaxApprovalUser = 10,
            //},
            //new FullLoanRequest<ClientDataT062900>
            //{
            //    ClientData = new ClientDataT062900
            //    {
            //        UserRequest = "ILADINES",
            //        GuaranteeType = GuaranteeType.GarantiaNoLiquida,
            //        Identification = "07821002",
            //        Address = 1,
            //        BillingCycle = BillingCycle.PimeraQuincena,
            //        CreditLine = 8000.00,
            //        Product = "001",
            //        Income = 5000.0,
            //        ModifyLoanApplication = ModifyLoanApplication.Default,
            //        RequestType = RequestType.IngresoARiesgos,
            //        RequestState = RequestStatus.APROBAR,
            //        RequestObservation1 = "SUPERVISADOS",
            //        RequestObservation2 = "OTROS",
            //    },
            //    IpPort = "http://10.0.2.54:8380",
            //    EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso6",
            //    Headless = false,
            //    KeepPdf = false,
            //    MaxApprovalUser = 10,
            //},
        };
        // Returns the test indices (primitive values), always matching the ClientDataList size
        public static TheoryData<int> GetData()
        {
            var data = new TheoryData<int>();

            for (int i = 0; i < ClientDataList.Count; i++)
                data.Add(i);
            return data;
        }

        // Maps index to ClientDataT062900 from the list
        private static FullLoanRequest<ClientDataT062900> GetClientDataByIndex(int index)
        {
            if (index < 0 || index >= ClientDataList.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid test data index");
            return ClientDataList[index];
        }
        //[Theory]
        //[MemberData(nameof(GetData))]
        //public async Task LoginTheoryTest(int clientDataIndex)
        //{
        //    var clientData = GetClientDataByIndex(clientDataIndex);
        //    ILoanApplication<ClientDataT062900, LoanApplicationResultT062900> loginTest = new LoanApplicationT062900(_playwright, _locators, _pdfConverter, _standardQueryService, _actionCoordinatorService, _outputAccessor.Output);
        //    await loginTest.ApplyForLoanAsync(clientData);
        //}
        //[Theory]
        //[MemberData(nameof(GetData))]
        //public async Task LoginTheoryTest(int clientDataIndex)
        //{
        //    var LoanAppproval = GetClientDataByIndex(clientDataIndex);

        //    var browser = await _playwright.PlaywrightVar.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        //    {
        //        Headless = false,
        //        DownloadsPath = LoanAppproval.EvidenceFoler, // Ruta para las descargas
        //    });
        //    var context = await browser.NewContextAsync();
        //    var page = await context.NewPageAsync();

        //    ILoanApproval approvalTest = new LoanApproval(page, _locators, _pdfConverter, _standardQueryService, _actionCoordinatorFactory, _outputAccessor.Output);
        //    await approvalTest.ApproveLoanAsync(LoanAppproval);
        //}
        //[Fact]
        //public async Task LoanApprovalTest()
        //{
        //    var LoanAppproval = new LoanApprovalModel
        //    {
        //        ApprovalNumber = 1,
        //        EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1",
        //        ApplicationNumber = "1245202956",
        //        ApprovingUser = "FOMORALES",
        //        IpPort = "http://10.0.2.54:8380",
        //    };
        //    var browser = await _playwright.PlaywrightVar.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        //    {
        //        Headless = false,
        //        DownloadsPath = LoanAppproval.EvidenceFoler, // Ruta para las descargas
        //    });
        //    var context = await browser.NewContextAsync();
        //    var page = await context.NewPageAsync();

        //    ILoanApproval approvalTest = new LoanApproval(page, _locators, _pdfConverter, _standardQueryService, _actionCoordinatorService, _outputAccessor.Output);
        //    await approvalTest.ApproveLoanAsync(LoanAppproval);
        //}
        [Theory]
        [MemberData(nameof(GetData))]
        public async Task Orquestatortest(int clientDataIndex)
        {
            var loanRequest = GetClientDataByIndex(clientDataIndex);

            if (!Directory.Exists(loanRequest.EvidenceFoler))
            {
                Directory.CreateDirectory(loanRequest.EvidenceFoler);
            }

            //var clientDatanew = new FullLoanRequest<ClientDataT062900>
            //{
            //    ClientData = new ClientDataT062900
            //    {
            //        UserRequest = "ILADINES",
            //        GuaranteeType = GuaranteeType.GarantiaLiquida,
            //        Identification = "07821002",
            //        Address = 1,
            //        Product = "001",
            //        BillingCycle = BillingCycle.SegundaQuincena,
            //        CreditLine = 8000.00,
            //        Income = 5000.0,
            //        ModifyLoanApplication = ModifyLoanApplication.Default,
            //        RequestType = RequestType.IngresoARiesgos,
            //        RequestState = RequestStatus.APROBAR,
            //        RequestObservation1 = "SUPERVISADOS",
            //        RequestObservation2 = "OTROS",
            //    },
            //    IpPort = "http://10.0.2.54:8380",
            //    EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso6",
            //    Headless = false,
            //    KeepPdf = false,
            //    MaxApprovalUser = 10,
            //};

            FullTransactionOrchestrator orchestrator = new FullTransactionOrchestrator(_provider, _playwright, _locators, _pdfConverter, _standardQueryService, _transactionUsersSelectionService, _actionCoordinatorFactory, _branchSynchronizationService, _userTurnCoordinatorService, _output);
            await orchestrator.TransactionAsync<ClientDataT062900>(loanRequest);

            //ApprovalOnlyTransactionOrchestrator approvalOrchestrator = new ApprovalOnlyTransactionOrchestrator(_provider, _playwright, _locators, _pdfConverter, _standardQueryService, _transactionUsersSelectionService, _actionCoordinatorFactory, _branchSynchronizationService, _userTurnCoordinatorService, _output);
            //var approvalRequestModel = new ApprovalRequestModel<ClientDataT062900>
            //{
            //    ApplicationNumber = "1245203424",
            //    EvidenceFoler = loanRequest.EvidenceFoler,
            //    Attempt = 2,
            //    IpPort = loanRequest.IpPort,
            //    RecognizedApprovingUsers = new List<string> { "HVENCES" },
            //    MaxApprovalUser = 10,
            //    Headless = false
            //};
            //await approvalOrchestrator.TransactionAsync(approvalRequestModel);
        }
    }
}
