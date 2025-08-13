namespace AutomationTest.FitbankWeb3.Application.Models.LoanApprovalModels.Input
{
    public class LoanApprovalModel
    {
        public required int ApprovalNumber { get; set; }
        public required string ApplicationNumber { get; set; }
        public required string ApprovingUser { get; set; }
        public required string EvidenceFoler { get; set; }
        public required string IpPort { get; set; }
    }
}
