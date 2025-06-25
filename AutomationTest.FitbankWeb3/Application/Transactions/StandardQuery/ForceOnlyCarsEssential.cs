using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery
{
    public class ForceOnlyCarsEssential : IStandardQuery<ForceOnlyCarsEssentialModel>
    {
        public GenericQueryModel CreateQuery(ForceOnlyCarsEssentialModel standardQueryModel)
        {
            string applcationNumber = standardQueryModel.ApplicationNumber;

            string query = $"UPDATE FITBANK.TEVALUACIONCREDITO " +
                $"SET DESAPROBADOSINDISPENSABLE = 0.000000 " +
                $"WHERE CSOLICITUD = '{applcationNumber}' " +
                $"AND FHASTA = '2999-12-31 00:00:00.000'";

            return new GenericQueryModel
            {
                Query = query,
                Timeout = 10000, // 10 segundos
                ThrowOnError = true // Lanzar excepción si hay un error
            };
        }
    }
}
