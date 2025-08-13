using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Domain.Attributes;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.ClientDataModels
{
    [TransactionType(TransactionType.T072100Be)]
    public class ClientDataT072100Be : IClientData
    {
        public required string UserRequest { get; set; } = string.Empty;
        public required string Identification { get; set; } = string.Empty;
        public required int Address { get; set; } = 1;
        public required double RMG { get; set; } = 0.0;
        public required ClientType ClientType { get; set; } = ClientType.A;
        public required GuaranteeType GuaranteeType { get; set; } = GuaranteeType.SinGarantia;
        public required ModifyLoanApplication ModifyLoanApplication { get; set; } = ModifyLoanApplication.Default;
    }
}
