using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Inbound
{
    public interface IFullWorkflowExecutor
    {
        /// <summary>
        /// Ejecuta el flujo completo de la solicitud y aprobacion de la solicitud
        /// </summary>
        /// <param name="fullLoanRequest"></param>
        /// <returns></returns>
        Task ExecuteWorkflow(FullWorkflowModel<IClientData> fullLoanRequest);
        /// <summary>
        /// Devuelve una lista de petición de préstamo genéricas (IClientData) 
        /// basadas en la configuración actual.
        /// </summary>
        IEnumerable<FullWorkflowModel<IClientData>> LoadCases();
    }
}
