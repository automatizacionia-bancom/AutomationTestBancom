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
    [TransactionType(TransactionType.T062600)]
    public class ClientDataT062600 : IClientData
    {
        public required string UserRequest { get; set; } = string.Empty;
        public required string Identification { get; set; } = string.Empty;
        public required int Address { get; set; } = 1;
        public required string Product { get; set; } = string.Empty;
        public required InsuranceType MortgageInsurance1 { get; set; } = InsuranceType.Mancomunado;
        public required InsuranceType MortgageInsurance2 { get; set; } = InsuranceType.Individual;
        public required MortgageGoodType MortgageGood { get; set; } = MortgageGoodType.BienFuturo;
        public required MortgageProjectType MortgageProject { get; set; } = MortgageProjectType.ProyectoPropio;
        public required double EstimatedValue { get; set; } = 0.0;
        public required double DownPayment { get; set; } = 0.0;
        public required int LoanInstallments { get; set; } = 0;
        public required double LoanRate { get; set; } = 0.0;
        public required MortgageBondType MortgageBond { get; set; } = MortgageBondType.SinBono;
        public required GuaranteeType GuaranteeType { get; set; } = GuaranteeType.SinGarantia;
        public required double Income { get; set; } = 0.0;
        public required ModifyLoanApplication ModifyLoanApplication { get; set; } = ModifyLoanApplication.Default;
        public required RequestType RequestType { get; set; } = RequestType.Excepcion;
        public required RequestStatus RequestState { get; set; } = RequestStatus.APROBAR;
        public required string RequestObservation1 { get; set; } = string.Empty;
        public required string RequestObservation2 { get; set; } = string.Empty;
    }
}
