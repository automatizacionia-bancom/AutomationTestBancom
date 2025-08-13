using System.Security.Policy;

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
        public string ApprovalStatusElementsPe { get; } = "#entorno-formulario > form:nth-child(7) > div > table > tbody";
        public string ApprovalStatusElementsBe { get; } = "#entorno-formulario > form:nth-child(8) > div > table > tbody";
        public string ApprovalStatusElements { get; } = ":nth-match(#entorno-formulario > form > div > table > tbody:visible, 1)";
        public string ApprovalComment { get; } = "#c_txtComentario_0";
        public string ApprovalUsersButton { get; } = "img.asistente-icono[src$=\"listavalores.png\"][alt=\"Evaluador\"][title=\"Evaluador\"]";
        public string ApprovalUsersListPe { get; } = "#entorno-formulario > form:nth-child(9) > div > table > tbody > tr > td";
        public string ApprovalUsersListBe { get; } = "#entorno-formulario > form:nth-child(10) > div > table > tbody > tr > td";
        public string ApprovalUsersList { get; } = ":nth-match(#entorno-formulario > form > div > table > tbody > tr > td:visible, 1)";
        public string ApplicationNumberSearchTransaction { get; } = "input.control.input.none.number-formatter[type=\"text\"][registro=\"0\"]";
        public string ApplicationNumberSearchTransactionResult { get; } = "span.input-label.none.criterios.link[registro=\"0\"]";
        public string ApplicationNumberSearchTransactionAssing { get; } = "#c_criCsolicitud_0";
        public string ApplicationNumberSearchTransactionAssingResult { get; } = "#c_txtCodSolicitud_0";
        public string ApplicationNumberSearchTransactionAssingList { get; } = "#container_0 > div > table > tbody > tr:nth-child(1) > td.input.usaIcono.list-of-values.none > img";
        public string ApplicationNumberSearchTransactionAssingButton { get; } = "button.formula-disabler.button.none[registro=\"0\"]:has-text(\"Enviar\")";
        public string ApplicationNumberSearchTransactionAssingInput { get; } = "#entorno-formulario > form > div > table > thead > tr:nth-child(2) > td > input";
        public string TransactionCurrentActivity { get; } = ":nth-match(input.record.input.usaIcono.long-text[type=\"text\"], 2)";
        public string GuaranteeSection { get; } = "a[registro=\"0\"]:has-text(\"Garantías\")";
        public string RmgValue { get; } = "#c_txtMontoRMA_0";
        public string GuaranteeCoverage { get; } = "#c_txtCoberturaA_0";
        public string GuaranteeType { get; } = "#container_6 > div > table > tbody > tr:nth-child(1) > td:nth-child(10) > img";
        public string GuaranteeGoodsType { get; } = "#container_6 > div > table > tbody > tr:nth-child(1) > td:nth-child(12) > img";
        public string GuaranteeGoodsTypeElement { get; } = "#entorno-formulario > form:nth-child(10) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string GuaranteeCondition { get; } = "#c_txtCondicionA_0";
        public string GuaranteeCoinType { get; } = "#c_txtMonedaA_0";
        public string GuaranteeTaxAmount { get; } = "#c_txtGravamenA_0";
        public string GuaranteeComertialValue { get; } = "#c_txtComercialA_0";
        public string GuaranteeFinalValue { get; } = "#c_txtRealizacionA_0";
        public string GuaranteeDate { get; } = "#c_txtFTasacionA_0";
        public string BusinessPlanSection { get; } = "a[registro=\"0\"]:has-text(\"Plan de Negocio\")";
        public string EspecificLinesSection { get; } = "a[registro=\"0\"]:has-text(\"Líneas Específicas\")";
        public string LineProductList { get; } = "#container_9 > div > table > tbody > tr:nth-child(1) > td:nth-child(8) > img";
        public string LineProductCoinType { get; } = "#c_cboMonedab_0";
        public string LineProductAmount { get; } = "#c_txtPropb_0";
        public string LineProductExpirationDay { get; } = "#c_txtFVencb_0";
        public string LineProductRate { get; } = "#c_txtTasab_0";
        public string LineProductAssingGuaranteeButton { get; } = "#c_btnGarb_0";
        public string LineProductAssingGuaranteeBox { get; } = "#c_vincula_0";
        public string LineProductAssingGuaranteeReturn { get; } = "button.formula-hider.button.link.none:has-text(\"<<Regresar\"):visible";
    }
}
