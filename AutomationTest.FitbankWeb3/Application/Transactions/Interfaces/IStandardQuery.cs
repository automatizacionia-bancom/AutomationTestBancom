using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Interfaces
{
    public interface IStandardQuery<TStandardQuery> where TStandardQuery : IStandardQueryModel
    {
        GenericQueryModel CreateQuery(TStandardQuery standardQueryModel);
    }
}
