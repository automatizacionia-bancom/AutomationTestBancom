using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Services;
using Microsoft.Playwright;

namespace AutomationTest.FitbankWeb3.Application.Services
{
    public class UserTurnCoordinatorService : IUserTurnCoordinatorService
    {
        private readonly Barrier _barrier = new Barrier(0);
        private readonly ConcurrentDictionary<string, string> _branchUsers = new ConcurrentDictionary<string, string>();

        public IBranchSession RegisterBranch()
        {
            // Genera un ID único
            var branchId = Guid.NewGuid().ToString("N");
            _barrier.AddParticipant();
            return new BranchSession(this, branchId);
        }

        // Este método interno es el mismo bucle que antes, pero recibe branchId
        internal Task ArriveUntilTurnAsync(string branchId, string user, CancellationToken cancellation)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    cancellation.ThrowIfCancellationRequested();

                    // 1) Guarda el usuario para esta fase
                    _branchUsers[branchId] = user;

                    // 2) Primera sincronización: todas comparten su usuario
                    _barrier.SignalAndWait(cancellation);

                    // 3) Calcula el usuario con más votos
                    string winner = _branchUsers.Values
                        .GroupBy(u => u)
                        .OrderByDescending(g => g.Count())
                        .ThenBy(g => g.Key)
                        .First().Key;

                    // 4) Segunda sincronización: todos avanzan juntos a la próxima fase
                    _barrier.SignalAndWait(cancellation);

                    // 5) Solo **después** de la segunda señal, las ramas ganadoras salen
                    if (user == winner)
                        return;

                    // 6) Si no eres ganador, vuelves al bucle
                }
            }, cancellation);
        }

        private void Deregister(string branchId)
        {
            // Quita cualquier usuario grabado (si existe)
            _branchUsers.TryRemove(branchId, out _);
            // Siempre elimina un participante para desbloquear el barrier
            _barrier.RemoveParticipant();
        }

        // La sesión a la que expone el RegisterBranch
        private class BranchSession : IBranchSession
        {
            private readonly UserTurnCoordinatorService _svc;
            private readonly string _branchId;
            private bool _disposed;

            public BranchSession(UserTurnCoordinatorService svc, string branchId)
            {
                _svc = svc;
                _branchId = branchId;
            }
            public Task ArriveUntilTurnAsync(string user, CancellationToken cancellation = default) => _svc.ArriveUntilTurnAsync(_branchId, user, cancellation);
            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _svc.Deregister(_branchId);
            }
        }
    }
}
