using AutomationTest.FitbankWeb3.Application.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels
{
    public class ForceOnlyCarsEssentialModel : IStandardQueryModel
    {
        public required string ApplicationNumber { get; set; }
    }
}
