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
    public class ForceLoanApprovalQuery : IStandardQuery<ForceLoanApprovalModel>
    {
        public GenericQueryModel CreateQuery(ForceLoanApprovalModel standardQueryModel)
        {
            string applcationNumber = standardQueryModel.ApplicationNumber;
            string creditCondition = standardQueryModel.TIPOSOLICITUDCREDITO ?? string.Empty;

            string query = $"UPDATE FITBANK.TEVALUACIONCREDITO" +
                $"SET DESAPROBADOSINDISPENSABLE = 0.000000," +
                $"DESAPROBADOSESTANDAR = 0.000000," +
                $"DESAPROBADOSCRITICOS = 0.000000," +
                $"RESULTADOFINAL = 'APROBADO' {creditCondition} WHERE CSOLICITUD = '{applcationNumber}' AND FHASTA = '2999-12-31 00:00:00.000'";

            return new GenericQueryModel
            {
                Query = query,
                Timeout = 10000, // 10 segundos
                ThrowOnError = true // Lanzar excepción si hay un error
            };
        }
    }
}
