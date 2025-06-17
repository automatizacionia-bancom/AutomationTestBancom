using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    public interface IActionCoordinatorService
    {
        /// <summary>
        /// Crea un handle que, al Dispose(), libera el semáforo.
        /// </summary>
        IActionHandle CreateHandle();
    }
    public interface IActionHandle : IDisposable
    {
        /// <summary>
        /// Espera hasta que sea tu turno.
        /// </summary>
        Task WaitForTurnAsync();
    }
}
