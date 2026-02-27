using AutomationTest.FitbankWeb3.Application.Models.QueryModels.StandardQueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;

namespace AutomationTest.FitbankWeb3.Application.Transactions.StandardQuery
{
    public class UpdatePasswordUserQuery : IStandardQuery<UpdatePasswordUserModel>
    {
        public GenericQueryModel CreateQuery(UpdatePasswordUserModel standardQueryModel)
        {
            //string query = $"DELETE FROM FITBANK.TUSUARIOSESIONES WHERE CUSUARIO = '{standardQueryModel.User}' AND FHASTA='2999-12-31 00:00:00'";

            string query = $"UPDATE FITBANK.TUSUARIOPASSWORD SET PASSWORD='F1D9574BDEC5ABF5B36B1F9607C535A4', FCADUCIDADPASSWORD='2099-12-31' WHERE FHASTA='2999-12-31 00:00:00' AND CUSUARIO='{standardQueryModel.User}'";

            return new GenericQueryModel
            {
                Query = query,
                Timeout = 10000, // 10 segundos
                ThrowOnError = false // No lanzar excepción si hay error, para manejarlo en el flujo de la aplicación
            };
        }
    }
}
