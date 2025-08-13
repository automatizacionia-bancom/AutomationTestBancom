using AutomationTest.FitbankWeb3.Application.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Services.ActionCoordination
{
    public class ActionCoordinatorService : IActionCoordinatorService
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public IActionHandle CreateHandle() => new ActionHandle(this);

        internal Task WaitAsync() => _semaphore.WaitAsync();
        internal void Release() => _semaphore.Release();

        private class ActionHandle : IActionHandle
        {
            private readonly ActionCoordinatorService _service;
            private bool _hasWaited;
            private bool _disposed;

            public ActionHandle(ActionCoordinatorService service)
            {
                _service = service;
            }

            public async Task WaitForTurnAsync()
            {
                await _service.WaitAsync();
                _hasWaited = true;
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;
                if (_hasWaited)
                    _service.Release();
            }
        }
    }
}
