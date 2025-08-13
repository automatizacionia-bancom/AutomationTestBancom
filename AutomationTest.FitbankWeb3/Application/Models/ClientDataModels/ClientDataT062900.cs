using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Domain.Attributes;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.ClientDataModels
{
    [TransactionType(TransactionType.T062900)]
    public class ClientDataT062900 : IClientData
    {
        public required string UserRequest { get; set; }
        public required string Identification { get; set; }
        public required int Address { get; set; }
        public required string Product { get; set; }
        public required LoanType LoanType { get; set; }
        public required int LoanInstallments { get; set; }
        public required double LoanAmount { get; set; }
        public required PayrollSourceType PayrollSource { get; set; }
        public required DisbursementType DisbursementType { get; set; }
        public required double Income { get; set; }
        public required ModifyLoanApplication ModifyLoanApplication { get; set; }
        public required RequestType RequestType { get; set; }
        public required RequestStatus RequestState { get; set; }
        public required string RequestObservation1 { get; set; }
        public required string RequestObservation2 { get; set; }
    }
}
