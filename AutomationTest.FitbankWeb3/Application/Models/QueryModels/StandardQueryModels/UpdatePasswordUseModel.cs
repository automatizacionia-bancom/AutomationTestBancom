using AutomationTest.FitbankWeb3.Application.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels
{
    public class UpdatePasswordUserModel : IStandardQueryModel
    {
        public required string User { get; set; }
        ///public string 
    }
}
