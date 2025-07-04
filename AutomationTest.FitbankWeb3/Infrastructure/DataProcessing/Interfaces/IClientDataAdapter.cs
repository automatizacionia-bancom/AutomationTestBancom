using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;

namespace AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.Interfaces
{
    /// <summary>
    /// Adaptador para convertir una fila (DataRow) a un ClientData concreto.
    /// </summary>
    public interface IClientDataAdapter<TClientData>
        where TClientData : IClientData
    {
        /// <summary>
        /// Adapta una fila del origen de datos (ej: Excel) a una instancia del modelo.
        /// </summary>
        TClientData Adapt(DataRow row);
    }
}
