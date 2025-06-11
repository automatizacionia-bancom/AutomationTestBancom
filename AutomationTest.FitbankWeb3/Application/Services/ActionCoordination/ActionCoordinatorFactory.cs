using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AutomationTest.FitbankWeb3.Application.Services.ActionCoordination
{
    public class ActionCoordinatorFactory : IActionCoordinatorFactory
    {
        private readonly IServiceProvider _provider;
        private readonly ConcurrentDictionary<ActionCoordinatorType, IActionCoordinatorService> _map
            = new();

        public ActionCoordinatorFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IActionCoordinatorService GetCoordinator(ActionCoordinatorType key)
        {
            return _map.GetOrAdd(key,
                _ => _provider.GetRequiredService<IActionCoordinatorService>());
        }
    }
}
