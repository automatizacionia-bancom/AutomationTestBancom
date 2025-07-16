using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class LocatorRepositoryBusinessBankingDashboard
    {
        public string CarsSectionCredit { get; } = "a[registro=\"0\"]:has-text(\"CARs\")";
        public string ExecuteProcessState { get; } = "#c_msgCarsProcesado_0";
        public string EvaluationResult { get; } = "#c_resultadoCars_0";
        public string CarsReportButton { get; } = "button.button.report.none:has-text(\"Reporte Cars\")";
        public string ApprovalSection { get; } = "a[registro=\"0\"]:has-text(\"Aprobaciones\")";
        public string ApprovalStatusList { get; } = "img.asistente-icono[src$=\"listavalores.png\"][alt=\"ESTADOS\"][title=\"ESTADOS\"]";
        public string ApprovalStatusElements { get; } = "#entorno-formulario > form:nth-child(7) > div > table > tbody";
        public string ApprovalComment { get; } = "#c_txtComentario_0";
        public string ApprovalUsersButton { get; } = "img.asistente-icono[src$=\"listavalores.png\"][alt=\"Evaluador\"][title=\"Evaluador\"]";
        public string ApprovalUsersList { get; } = "#entorno-formulario > form:nth-child(9) > div > table > tbody > tr > td";
        public string ApplicationNumberSearchTransaction { get; } = "input.control.input.none.number-formatter[type=\"text\"][registro=\"0\"]";
        public string ApplicationNumberSearchTransactionResult { get; } = "span.input-label.none.criterios.link[registro=\"0\"]";
        public string ApplicationNumberSearchTransactionAssing { get; } = "#c_criCsolicitud_0";
        public string ApplicationNumberSearchTransactionAssingResult { get; } = "#c_txtCodSolicitud_0";
        public string ApplicationNumberSearchTransactionAssingList { get; } = "#container_0 > div > table > tbody > tr:nth-child(1) > td.input.usaIcono.list-of-values.none > img";
        public string ApplicationNumberSearchTransactionAssingButton { get; } = "button.formula-disabler.button.none[registro=\"0\"]:has-text(\"Enviar\")";
    }
}
