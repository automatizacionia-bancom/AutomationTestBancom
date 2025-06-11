using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Domain.Enums
{
    public enum TransactionType
    {
        T062900, // Transacción de convenios PNP
        T062800, // Transaccion de convenios Civiles y Maxiprestamos
        T062700, // Transacción de tarjeta de crédito
        T062600, // Transacción de prestamos hipotecarios
        T062500, // Transacción de convenios FFAA
        T062400, // Transacción de credito pignoraticio
    }
}
