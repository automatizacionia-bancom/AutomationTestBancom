using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Services.ActionCoordination
{
    public class ActionCoordinatorService : IActionCoordinatorService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public Task WaitForTurnAsync() => _semaphore.WaitAsync();
        public void ReleaseTurn()
        {
            if (_semaphore.CurrentCount == 0)
                _semaphore.Release();
        }
    }
}
