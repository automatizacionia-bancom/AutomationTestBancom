using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Outbound
{
    public interface IGenericQueryExecutor
    {
        Task<DataTable> ExecuteAsync(GenericQueryModel query);
    }
}
