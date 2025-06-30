using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    /// <summary>
    /// Puerto de entrada para cargar todos los FullLoanRequest de cualquier TClientData.
    /// </summary>
    public interface ITestCaseLoader
    {
        /// <summary>
        /// Devuelve una lista de petición de préstamo genéricas (IClientData) 
        /// basadas en la configuración actual.
        /// </summary>
        IEnumerable<FullWorkflowModel<IClientData>> LoadCases();
    }
}
