using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    public interface ITransactionUsersSelectionService
    {
        /// <summary>
        /// Asincronicamente selecciona el usuario óptimo para una transacción dada,
        /// </summary>
        /// <param name="transactionCode"></param>
        /// <param name="recognizedUsers"></param>
        /// <returns></returns>
        Task<string> SelectOptimalUserAsync(TransactionType transactionCode, List<string> recognizedUsers);
    }
}
