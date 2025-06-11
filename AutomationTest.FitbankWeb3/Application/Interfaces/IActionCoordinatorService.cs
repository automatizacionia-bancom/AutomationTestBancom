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
        /// Licencia para ejecutar una acción. El método espera hasta que sea su turno, sin timeout.
        /// </summary>
        /// <returns></returns>
        Task WaitForTurnAsync();
        /// <summary>
        /// Libera el turno para que otro proceso pueda ejecutar su acción.
        /// </summary>
        void ReleaseTurn();
    }
}
