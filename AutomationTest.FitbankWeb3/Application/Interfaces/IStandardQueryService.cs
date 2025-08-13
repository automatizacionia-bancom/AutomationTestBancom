using System.Data;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    public interface IStandardQueryService
    {
        Task<DataTable> ExecuteStandardQueryAsync<TStandardQueryModel>(TStandardQueryModel standardQuery) where TStandardQueryModel : IStandardQueryModel;
    }
}
