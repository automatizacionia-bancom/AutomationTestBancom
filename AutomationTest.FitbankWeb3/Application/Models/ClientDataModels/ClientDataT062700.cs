using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Domain.Attributes;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.ClientDataModels
{
    [TransactionType(TransactionType.T062700)]
    public class ClientDataT062700 : IClientData
    {
        public required string UserRequest { get; set; } = string.Empty;
        public required string Identification { get; set; } = string.Empty;
        public required int Address { get; set; } = 1;
        public required string Product { get; set; } = string.Empty;
        public required GuaranteeType GuaranteeType { get; set; } = GuaranteeType.SinGarantia;
        public required BillingCycle BillingCycle { get; set; } = BillingCycle.PimeraQuincena;
        public required double CreditLine { get; set; } = 0.0;
        public required double Income { get; set; } = 0.0;
        public required ModifyLoanApplication ModifyLoanApplication { get; set; } = ModifyLoanApplication.Default;
        public required RequestType RequestType { get; set; } = RequestType.Excepcion;
        public required RequestStatus RequestState { get; set; } = RequestStatus.APROBAR;
        public required string RequestObservation1 { get; set; } = string.Empty;
        public required string RequestObservation2 { get; set; } = string.Empty;
    }
}
