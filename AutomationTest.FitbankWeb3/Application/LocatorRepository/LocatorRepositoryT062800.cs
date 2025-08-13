namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class LocatorRepositoryT062800
    {
        public string OK_ApprovalError { get; } = "#entorno-estatus-contenido:text-matches(\"^(?:Ok|Error al procesar la consulta)$\")";
        public string ApprovalError { get; } = "#entorno-estatus-contenido:text('Error al procesar la consulta')";
        public string Identification { get; } = "#c_v2_identificacion_0";
        public string Adress { get; } = "#c_v2_cdireccion_0";
        public string AdressList { get; } = "#container_3 > div > table > tbody > tr:nth-child(1) > td:nth-child(12) > img";
        public string AddressElement { get; } = "#entorno-formulario > form:nth-child(7) > div > table > tbody > tr > td:nth-child(1)";
        public string ProductList { get; } = "#container_5 > table > tbody > tr:nth-child(1) > td.columna_1 > img:nth-child(2)";
        public string ManagerList { get; } = "img[title='PROMOTORES']";
        public string ManagerElement { get; } = "#entorno-formulario > form:nth-child(12) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string LoanTypeList { get; } = "#container_5 > table > tbody > tr:nth-child(2) > td.columna_1 > img";
        public string LoanInstallments { get; } = "#c_txtCuotas_0";
        public string LoanAmount { get; } = "#c_txtMtoPrestamo_0";
        public string LoanRate { get; } = "#c_txtTasaNegociable_0";
        public string CreditDataLabel { get; } = "#entorno-formulario > div.entorno-html > form > fieldset:nth-child(7) > legend";
        public string DisbursementType { get; } = "#c_cboFormaDesembolso_0";
        public string BankAccountList { get; } = "#container_6 > table > tbody > tr:nth-child(3) > td.columna_6 > img";
        public string BankAccountElement { get; } = "#entorno-formulario > form:nth-child(14) > div > table > tbody > tr:nth-child(1)";
        public string ApplicationNumber { get; } = "#c_txtCsolicitud_0";
        public string IncomeButtton { get; } = "#c_lblIngresoDescuento_0";
        public string IncomeDate { get; } = "#c_txtPeriodo1_0";
        public string IncomeAssets { get; } = "#c_txtImporte1_0";
        public string IncomeOther1 { get; } = "#c_txtImporte1_4";
        public string IncomeOther2 { get; } = "#c_txtImporte1_3";
        public string IncomeReturn { get; } = "span.link:has-text('Regresar')";
        public string GuaranteeButton { get; } = "#c_chkGarantia_0";
        public string GuaranteeType { get; } = "#container_5 > div > table > tbody > tr:nth-child(1) > td:nth-child(5) > img";
        public string GuaranteeGoodsType { get; } = "#container_5 > div > table > tbody > tr:nth-child(1) > td:nth-child(7) > img";
        public string GuaranteeGoodsTypeElement { get; } = "#entorno-formulario > form:nth-child(7) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string GuaranteeCoinType { get; } = "#c_v2_Moneda_0";
        public string GuaranteeTaxAmount { get; } = "#c_v2_montoGrm_0";
        public string GuaranteeAmount { get; } = "#c_v2_montoRealiz_0";
        public string GuaranteeDate { get; } = "#c_v2_fechTasac_0";
        public string GuaranteeDescription { get; } = "#c_v2_comentarios_0";
        public string GuaranteeReturn { get; } = "span.link:has-text('Regresar')";
        public string EvaluateButton { get; } = "#c_Evaluar_0";
        public string EvaluateResult { get; } = "#c_resultadoCars_0";
        public string ValidateDocumentationVerification { get; } = "#c_chkVerificado_0";
        public string ValidateDocumentationType { get; } = "#container_5 > table > tbody > tr:nth-child(2) > td.columna_1 > img";
        public string ValidateDocumentationTypeElement { get; } = "#entorno-formulario > form:nth-child(7) > div > table > tbody > tr:nth-child(1) > td:nth-child(2)";
        public string ValidateDocumentationDate { get; } = "#c_v6_fecVerificacion_0";
        public string ValidateDocumentationObservation { get; } = "#c_v6_observaciones_0";
        public string ValidateDocumentationResult { get; } = "#c_v6_resultado_0";
    }
}
