using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Application.Transactions.Interfaces
{
    public interface IStandardQuery<TStandardQuery> where TStandardQuery : IStandardQueryModel
    {
        GenericQueryModel CreateQuery(TStandardQuery standardQueryModel);
    }
}
