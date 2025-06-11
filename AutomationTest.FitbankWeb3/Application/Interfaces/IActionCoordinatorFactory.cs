using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    public interface IActionCoordinatorFactory
    {
        /// <summary>
        /// Devuelve el coordinador asociado a <paramref name="key"/>,
        /// creando uno nuevo la primera vez.
        /// </summary>
        IActionCoordinatorService GetCoordinator(ActionCoordinatorType key);
    }

}
