using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Inbound
{
    public interface ILoanApplicationExecutor
    {
        /// <summary>
        /// Ejecuta el flujo de la solicitud
        /// </summary>
        /// <param name="fullLoanRequest"></param>
        /// <returns></returns>
        Task ExecuteWorkflow(LoanApplicationWorkflowModel<IClientData> fullLoanRequest);
        /// <summary>
        /// Devuelve una lista de petición de préstamo genéricas (IClientData) 
        /// basadas en la configuración actual.
        /// </summary>
        IEnumerable<LoanApplicationWorkflowModel<IClientData>> LoadCases();
    }
}