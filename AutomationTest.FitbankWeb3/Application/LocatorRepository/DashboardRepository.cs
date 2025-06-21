using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class DashboardRepository
    {
        public string TransactionInput { get; } = "#entorno-pt";
        public string F12Button { get; } = "button[title='Guardar']";
        public string F7Button { get; } = "button[title='Consultar']";
        public string OK { get; } = "#entorno-estatus-contenido:text('Ok')";
        public string TransactionCorrect { get; } = "#entorno-estatus-contenido:text('TRANSACCION REALIZADA CORRECTAMENTE')";
        public string OK_TransactionCorrect { get; } = "#entorno-estatus-contenido:text-matches(\"^(?:Ok|TRANSACCION REALIZADA CORRECTAMENTE)$\")";
        public string TransactionError { get; } = "#entorno-estatus-contenido.error";
        public string NonAuthorizingUsers { get; } = "#entorno-estatus-contenido:text('NO TIENE AUTORIZADORES')";
        public string CalificationResultSection { get; } = "a:has-text('Resultado Calificacion')";
        public string ApprovalSection { get; } = "a:has-text('Aprobaciones')";//":is(li.tab-bar-txtTrxAprobacion, li.tab-bar-062810) >> role=link[name='Aprobaciones']";
        public string ViewCarsButton { get; } = "span:has-text('Ver Criterios de Aceptacion de Riesgos')";
        public string CarsTable { get; } = "#container_2 > div > table > tbody";
        public string CarsReturn { get; } = "span:has-text('Regresar')";
        public string RequestType { get; } = "#c_cmbTipoSolicitud_0";
        public string RequestState { get; } = "#c_cmbEstado_0";
        public string RequestComment { get; } = "#c_txtComentario_0";
        public string RequestObservation1 { get; } = "#container_4 > table > tbody > tr:nth-child(2) > td.columna_4 > img";
        public string RequestObservation2 { get; } = "#container_4 > table > tbody > tr:nth-child(3) > td.columna_4 > img";
        public string ApplicationNumberSearch { get; } = "#c_txtNroSolicitudC_0";
        public string ApprovalUsers { get; } = "#container_2 > div > table > tbody > tr:nth-child(1) > td.usaIcono.button.list-of-values.none > img";
        public string AprovalUsersList { get; } = "#entorno-formulario > form:nth-child(7) > div > table";
        public string ApprovalStatus { get; } = "#c_txtEstado_0";
        public string ApprovalProcess { get; } = "#c_txtProceso_0";
        public string ListElement(string elementName) => $"td:text-matches(\"^{elementName}$\", \"\")";
    }
}
