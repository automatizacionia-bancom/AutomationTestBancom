using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    /// <summary>
    /// Servicio que coordina turnos de usuario entre ramas en fases:
    /// 1) Cada rama registra su usuario previsto
    /// 2) Cuando todas llegan o se desregistran, el servicio elige el usuario con más asignaciones
    /// 3) Devuelve a cada rama si es su turno (true/false)
    /// 4) Ramas con turno false esperan la siguiente fase
    /// 5) Ramas que caen se excluyen automáticamente
    /// </summary>
    public interface IUserTurnCoordinatorService
    {
        /// <summary>
        /// Registra internamente una rama, genera su ID y devuelve la sesión.
        /// </summary>
        IBranchSession RegisterBranch();
    }

    /// <summary>
    /// Representa la sesión de una rama en el coordinador.
    /// </summary>
    public interface IBranchSession : IDisposable
    {
        /// <summary>
        /// Envía el usuario y espera hasta que sea su turno.
        /// </summary>
        Task ArriveUntilTurnAsync(string user, CancellationToken cancellation = default);
    }
}
