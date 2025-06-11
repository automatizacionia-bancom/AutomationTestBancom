using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery
{
    public class DeleteUserSesionQuery : IStandardQuery
    {
        public GenericQueryModel CreateQuery(StandardQueryModel standardQueryModel)
        {
            string query =  $"DELETE FROM FITBANK.TUSUARIOSESIONES WHERE CUSUARIO = '{standardQueryModel.User}' AND FHASTA='2999-12-31 00:00:00'";

            return new GenericQueryModel
            {
                Query = query,
                Timeout = 10000, // 10 segundos
                ThrowOnError = true // No lanzar excepción si hay error, para manejarlo en el flujo de la aplicación
            };
        }
    }
}
