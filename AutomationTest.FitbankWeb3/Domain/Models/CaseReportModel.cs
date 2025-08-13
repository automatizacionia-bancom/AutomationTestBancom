namespace AutomationTest.FitbankWeb3.Domain.Models
{
    public class CaseReportModel
    {
        public required int CaseIndex;
        public required string ApplicationNumber;
        public required bool Success;
        public required string Message;
        public required DateTime Timestamp;
    }
}
