using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    public interface IBranchSynchronizationService
    {
        /// <summary>
        /// Registra una rama y devuelve un 'scope' que, al disponerse,
        /// la elimina de la barrera (como si hubiera fallado).
        /// </summary>
        IDisposable RegisterBranch(string branchId);

        /// <summary>
        /// Marca llegada de una rama y espera.
        /// </summary>
        Task ArriveAndWaitAsync(string branchId, CancellationToken cancellation = default);
    }
}
