using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    public interface IStandardQueryService
    {
        Task<DataTable> ExecuteStandardQueryAsync<TStandardQuery>(StandardQueryModel standardQuery) where TStandardQuery : IStandardQuery;
    }
}
