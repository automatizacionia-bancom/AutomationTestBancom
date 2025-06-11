using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    /// <summary>
    /// Resuelve el TransactionType y los tipos concretos anotados con TransactionTypeAttribute,
    /// disambiguando según la interfaz esperada.
    /// </summary>
    public interface ITransactionDataResolver
    {
        /// <summary>
        /// Obtiene el TransactionType asociado al tipo genérico T.
        /// </summary>
        TransactionType GetTransactionType<T>();

        /// <summary>
        /// Obtiene el TransactionType de la instancia proporcionada.
        /// </summary>
        TransactionType GetTransactionType(object instance);

        /// <summary>
        /// Obtiene el tipo concreto asociado al TransactionType que implementa la interfaz TInterface.
        /// </summary>
        /// <typeparam name="TInterface">Interfaz que debe implementar el tipo.</typeparam>
        Type GetDataType<TInterface>(TransactionType transactionType);
    }
}
