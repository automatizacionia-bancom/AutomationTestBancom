using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Attributes;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Application.Models.ClientDataModels
{
    [TransactionType(TransactionType.T062900)]
    public class ClientDataT062900 : IClientData
    {
        public string UserRequest { get; set; } = string.Empty;
        public string Identification { get; set; } = string.Empty;
        public int Address { get; set; } = 1;
        public string Product { get; set; } = string.Empty;
        public LoanType LoanType { get; set; } = LoanType.Prestamo;
        public int LoanInstallments { get; set; } = 0;
        public double LoanAmount { get; set; } = 0.0;
        public string PayrollSource { get; set; } = string.Empty;
        public double Income { get; set; } = 0.0;
        public RequestType RequestType { get; set; } = RequestType.Excepcion;
        public RequestStatus RequestState { get; set; } = RequestStatus.APROBAR;
        public string RequestObservation1 { get; set; } = string.Empty;
        public string RequestObservation2 { get; set; } = string.Empty;

    }
}
