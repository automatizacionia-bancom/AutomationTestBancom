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
    [TransactionType(TransactionType.T062400)]
    public class ClientDataT062400 : IClientData
    {
        public required string UserRequest { get; set; }
        public required string Identification { get; set; }
        public required int Address { get; set; }
        public required string Product { get; set; }
        public required double RequestedAmount { get; set; }
        public required PaymentTerm PaymentTerm { get; set; }
        public required double JewelGrossWeight { get; set; }
        public required double JewelEmbeddedWeight { get; set; }
        public required double Income { get; set; }
        public required DisbursementType DisbursementType { get; set; }
        public required ModifyLoanApplication ModifyLoanApplication { get; set; }
        public required RequestType RequestType { get; set; }
        public required RequestStatus RequestState { get; set; }
        public required string RequestObservation1 { get; set; }
        public required string RequestObservation2 { get; set; }
    }
}
