using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Interfaces;

namespace AutomationTest.FitbankWeb3.Application.Services
{
    public class DynamicBranchBarrier : IBranchSynchronizationService
    {
        private readonly Barrier _barrier = new Barrier(0);

        public IDisposable RegisterBranch(string branchId)
        {
            // Añade un participante al barrier
            _barrier.AddParticipant();
            return new Registration(_barrier);
        }

        public Task ArriveAndWaitAsync(string branchId, CancellationToken cancellation = default)
        {
            // SignalAndWait es bloqueante, así que lo envolvemos en Task.Run
            return Task.Run(() =>
            {
                try
                {
                    // Señala la llegada y espera a los demás participantes
                    _barrier.SignalAndWait();
                }
                catch (BarrierPostPhaseException)
                {
                    // Ignorar incongruencias de fase
                }
            }, cancellation);
        }

        private class Registration : IDisposable
        {
            private readonly Barrier _barrier;
            private bool _disposed;

            public Registration(Barrier barrier)
            {
                _barrier = barrier;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                // Quita un participante; si otros esperan, libera la barrier
                _barrier.RemoveParticipant();
            }
        }
    }
}
