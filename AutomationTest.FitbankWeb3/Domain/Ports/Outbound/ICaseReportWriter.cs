using AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Outbound
{
    public interface ICaseReportWriter : IAsyncDisposable
    {
        Task WriteAsync(CaseReportModel report);
    }
}
