using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Domain.Attributes;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.ClientDataModels
{
    [TransactionType(TransactionType.T072100Pe)]
    public class ClientDataT072100Pe : IClientData
    {
        public required string UserRequest { get; set; } = string.Empty;
        public required string Identification { get; set; } = string.Empty;
        public required int Address { get; set; } = 1;
        public required string Product { get; set; } = string.Empty;
        public required LoanType LoanType { get; set; } = LoanType.Prestamo;
        public required double LoanAmount { get; set; } = 0.0;
        public required int LoanInstallments { get; set; } = 0;
        public required ClientType ClientType { get; set; } = ClientType.A;
        public required RisktType RiskType { get; set; } = RisktType.Bajo;
        public required DisbursementType DisbursementType { get; set; } = DisbursementType.AbonoACuenta;
        public required GuaranteeType GuaranteeType { get; set; } = GuaranteeType.SinGarantia;
        public required ModifyLoanApplication ModifyLoanApplication { get; set; } = ModifyLoanApplication.Default;
    }
}
