using AutomationTest.FitbankWeb3.Application.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels
{
    public class ForceLoanApprovalModel : IStandardQueryModel
    {
        public required string ApplicationNumber { get; set; }
        public required string TIPOSOLICITUDCREDITO { get; set; }
    }
}
