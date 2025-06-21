using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Attributes;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Application.Models.ClientDataModels
{
    [TransactionType(TransactionType.T062400)]
    public class ClientDataT062400 : IClientData
    {
        public string UserRequest { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public int Address { get; set; } = 1;
        public string Product { get; set; } = string.Empty;
        public double RequestedAmount { get; set; } = 0.0;
        public PaymentTerm PaymentTerm { get; set; } = PaymentTerm.Monthly;
        public double JewelGrossWeight { get; set; } = 0.0;
        public double JewelEmbeddedWeight { get; set; } = 0.0;
        public double Income { get; set; } = 0.0;
        public ModifyLoanApplication ModifyLoanApplication { get; set; } = ModifyLoanApplication.Default;
        public RequestType RequestType { get; set; } = RequestType.Excepcion;
        public RequestStatus RequestState { get; set; } = RequestStatus.APROBAR;
        public string RequestObservation1 { get; set; } = string.Empty;
        public string RequestObservation2 { get; set; } = string.Empty;
    }
}
