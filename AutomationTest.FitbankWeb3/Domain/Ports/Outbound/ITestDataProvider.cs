using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

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
