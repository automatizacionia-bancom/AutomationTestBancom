using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Outbound
{
    public interface ITransactionUsersProvider
    {
        /// <summary>
        /// Obtiene los usuarios asociados a un determinado código de transacción.
        /// </summary>
        /// <param name="transactionCode"></param>
        /// <returns></returns>
        Task<List<string>> GetUsersForTransactionAsync(TransactionType transactionCode);
    }
}
