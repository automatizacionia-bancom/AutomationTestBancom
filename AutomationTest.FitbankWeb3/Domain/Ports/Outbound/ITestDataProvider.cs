using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Outbound
{
    public interface ITestDataProvider
    {
        /// <summary>
        /// Lee todos los casos de <typeparamref name="TClientData"/> de la 
        /// fuente indicada por <paramref name="filePath"/> y <paramref name="sheetName"/>.
        /// </summary>
        IEnumerable<TClientData> GetTestCases<TClientData>(
            string filePath,
            string sheetName)
            where TClientData : IClientData;
    }
}
