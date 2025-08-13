using System.Data;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Outbound
{
    public interface IGenericQueryExecutor
    {
        Task<DataTable> ExecuteAsync(GenericQueryModel query);
    }
}
