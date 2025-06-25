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
using AutomationTest.FitbankWeb3.Infrastructure.Configuration;
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
        private readonly ITestOutputAccessor _outputAccessor;

        public MainTest(TestFixture fixture, ITestOutputHelper output)
        {
            var accessor = fixture.ServiceProvider
               .GetRequiredService<ITestOutputAccessor>();
            accessor.Set(output);

            _provider = fixture.ServiceProvider;
            // 1) Cada clase de test abre su propio scope para aislar servicios Scoped/ Transient.
            _scope = fixture.ServiceProvider.CreateScope();

            // 2) Resolvemos aquí los servicios que necesitemos
            _outputAccessor = _scope.ServiceProvider.GetRequiredService<ITestOutputAccessor>();
            _playwright = _scope.ServiceProvider.GetRequiredService<PlaywrightFixture>();
            _locators = _scope.ServiceProvider.GetRequiredService<ElementRepositoryFixture>();
            _pdfConverter = _scope.ServiceProvider.GetRequiredService<IPdfConverter>();
            _standardQueryService = _scope.ServiceProvider.GetRequiredService<IStandardQueryService>();
            _transactionUsersSelectionService = _scope.ServiceProvider.GetRequiredService<ITransactionUsersSelectionService>();
            _actionCoordinatorFactory = _scope.ServiceProvider.GetRequiredService<IActionCoordinatorFactory>();
            _branchSynchronizationService = _scope.ServiceProvider.GetRequiredService<IBranchSynchronizationService>();
            _userTurnCoordinatorService = _scope.ServiceProvider.GetRequiredService<IUserTurnCoordinatorService>();

        }
        private static readonly List<FullLoanRequest<ClientDataT062700>> ClientDataList = new()
        {
            new FullLoanRequest<ClientDataT062700>
            {
                ClientData = new ClientDataT062700
                {
                    UserRequest = "ILADINES",
                    Identification = "76310952",
                    Address = 1,
                    Income = 2000.00,
                    Product = "001",
                    BillingCycle = BillingCycle.PimeraQuincena,
                    CreditLine = 8000.00,
                    GuaranteeType = GuaranteeType.SinGarantia,
                    ModifyLoanApplication = ModifyLoanApplication.APROBAR,
                    RequestType = RequestType.IngresoARiesgos,
                    RequestState = RequestStatus.APROBAR,
                    RequestObservation1 = "SUPERVISADOS",
                    RequestObservation2 = "OTROS",
                },
                IpPort = "http://10.0.2.54:8380",
                EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\cajamarca\\Caso1",
                Headless = true,
                KeepPdf = false,
                MaxApprovalUser = 10,
            },
            //new FullLoanRequest<ClientDataT062700>
            //{
            //    ClientData = new ClientDataT062700
            //    {
            //        UserRequest = "ILADINES",
            //        Identification = "76310952",
            //        Address = 1,
            //        Income = 2000.00,
            //        Product = "001",
            //        BillingCycle = BillingCycle.PimeraQuincena,
            //        CreditLine = 8000.00,
            //        GuaranteeType = GuaranteeType.SinGarantia,
            //        ModifyLoanApplication = ModifyLoanApplication.Default,
            //        RequestType = RequestType.IngresoARiesgos,
            //        RequestState = RequestStatus.APROBAR,
            //        RequestObservation1 = "SUPERVISADOS",
            //        RequestObservation2 = "OTROS",
            //    },
            //    IpPort = "http://10.0.2.54:8380",
            //    EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\cajamarca\\Caso2",
            //    Headless = true,
            //    KeepPdf = false,
            //    MaxApprovalUser = 10,
            //},
            new FullLoanRequest<ClientDataT062700>
            {
                ClientData = new ClientDataT062700
                {
                    UserRequest = "ILADINES",
                    Identification = "07812813",
                    Address = 1,
                    Income = 2000.00,
                    Product = "001",
                    BillingCycle = BillingCycle.PimeraQuincena,
                    CreditLine = 8000.00,
                    GuaranteeType = GuaranteeType.GarantiaLiquida,
                    ModifyLoanApplication = ModifyLoanApplication.APROBAR,
                    RequestType = RequestType.IngresoARiesgos,
                    RequestState = RequestStatus.APROBAR,
                    RequestObservation1 = "SUPERVISADOS",
                    RequestObservation2 = "OTROS",
                },
                IpPort = "http://10.0.2.54:8380",
                EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\cajamarca\\Caso3",
                Headless = true,
                KeepPdf = false,
                MaxApprovalUser = 10,
            },
            new FullLoanRequest<ClientDataT062700>
            {
                ClientData = new ClientDataT062700
                {
                    UserRequest = "ILADINES",
                    Identification = "74885761",
                    Address = 1,
                    Income = 2000.00,
                    Product = "001",
                    BillingCycle = BillingCycle.PimeraQuincena,
                    CreditLine = 8000.00,
                    GuaranteeType = GuaranteeType.GarantiaLiquida,
                    ModifyLoanApplication = ModifyLoanApplication.Default,
                    RequestType = RequestType.IngresoARiesgos,
                    RequestState = RequestStatus.APROBAR,
                    RequestObservation1 = "SUPERVISADOS",
                    RequestObservation2 = "OTROS",
                },
                IpPort = "http://10.0.2.54:8380",
                EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\cajamarca\\Caso4",
                Headless = true,
                KeepPdf = false,
                MaxApprovalUser = 10,
            },
            new FullLoanRequest<ClientDataT062700>
            {
                ClientData = new ClientDataT062700
                {
                    UserRequest = "ILADINES",
                    Identification = "07577279",
                    Address = 1,
                    Income = 2000.00,
                    Product = "001",
                    BillingCycle = BillingCycle.PimeraQuincena,
                    CreditLine = 8000.00,
                    GuaranteeType = GuaranteeType.GarantiaNoLiquida,
                    ModifyLoanApplication = ModifyLoanApplication.APROBAR,
                    RequestType = RequestType.IngresoARiesgos,
                    RequestState = RequestStatus.APROBAR,
                    RequestObservation1 = "SUPERVISADOS",
                    RequestObservation2 = "OTROS",
                },
                IpPort = "http://10.0.2.54:8380",
                EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\cajamarca\\Caso5",
                Headless = true,
                KeepPdf = false,
                MaxApprovalUser = 10,
            },
            new FullLoanRequest<ClientDataT062700>
            {
                ClientData = new ClientDataT062700
                {
                    UserRequest = "ILADINES",
                    Identification = "74885761",
                    Address = 1,
                    Income = 2000.00,
                    Product = "001",
                    BillingCycle = BillingCycle.PimeraQuincena,
                    CreditLine = 8000.00,
                    GuaranteeType = GuaranteeType.GarantiaNoLiquida,
                    ModifyLoanApplication = ModifyLoanApplication.Default,
                    RequestType = RequestType.IngresoARiesgos,
                    RequestState = RequestStatus.APROBAR,
                    RequestObservation1 = "SUPERVISADOS",
                    RequestObservation2 = "OTROS",
                },
                IpPort = "http://10.0.2.54:8380",
                EvidenceFoler = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\cajamarca\\Caso6",
                Headless = true,
                KeepPdf = false,
                MaxApprovalUser = 10,
            },
        };
        // Returns the test indices (primitive values), always matching the ClientDataList size
        public static TheoryData<int> GetData()
        {
            var data = new TheoryData<int>();

            for (int i = 0; i < ClientDataList.Count; i++)
                data.Add(i);
            return data;
        }

        // Maps index to ClientDataT062700 from the list
        private static FullLoanRequest<ClientDataT062700> GetClientDataByIndex(int index)
        {
            if (index < 0 || index >= ClientDataList.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid test data index");
            return ClientDataList[index];
        }
        [Theory]
        [MemberData(nameof(GetData))]
        public async Task Orquestatortest(int clientDataIndex)
        {
            var loanRequest = GetClientDataByIndex(clientDataIndex);

            if (!Directory.Exists(loanRequest.EvidenceFoler))
            {
                Directory.CreateDirectory(loanRequest.EvidenceFoler);
            }
            
            ITransactionOrchestrator orchestrator = _provider.GetRequiredService<ITransactionOrchestrator>();
            await orchestrator.TransactionAsync<ClientDataT062700>(loanRequest);
            
            //ApprovalOnlyTransactionOrchestrator approvalOrchestrator = new ApprovalOnlyTransactionOrchestrator(_provider, _playwright, _locators, _pdfConverter, _standardQueryService, _transactionUsersSelectionService, _actionCoordinatorFactory, _branchSynchronizationService, _userTurnCoordinatorService, _outputAccessor.Output);
            //var approvalRequestModel = new ApprovalRequestModel<ClientDataT062700>
            //{
            //    ApplicationNumber = "1061702007",
            //    EvidenceFoler = loanRequest.EvidenceFoler,
            //    Attempt = 2,
            //    IpPort = loanRequest.IpPort,
            //    RecognizedApprovingUsers = new List<string> { "HDELALCAZARX" },
            //    MaxApprovalUser = 10,
            //    Headless = true
            //};
            //await approvalOrchestrator.TransactionAsync(approvalRequestModel);
        }
    }
}
