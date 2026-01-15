namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class LocatorRepositoryT062400
    {
        public string OK_RangeError { get; } = "#entorno-estatus-contenido:text-matches(\"^(?:Ok|EL MONTO A SOLICITAR DE).*\")";
        public string Identification { get; } = "#c_v2_identificacion_0";
        public string Adress { get; } = "#c_v2_cdireccion_0";
        public string AdressList { get; } = "#container_3 > div > table > tbody > tr > td:nth-child(9) > img";
        public string AddressElement { get; } = "#entorno-formulario > form:nth-child(7) > div > table > tbody > tr > td:nth-child(1)";
        public string ProductList { get; } = "#container_5 > table > tbody > tr:nth-child(1) > td.columna_1 > img:nth-child(2)";
        public string ManagerList { get; } = "#container_5 > table > tbody > tr:nth-child(1) > td.columna_3 > img:nth-child(2)";
        public string ManagerElement { get; } = "#entorno-formulario > form:nth-child(10) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string LotNumber { get; } = "#c_txt_NumeroLote_0";
        public string ConsultSentinel { get; } = "#c_btnSentinel_0";
        public string JewelTypeList { get; } = "#container_10 > div > table > tbody > tr:nth-child(1) > td:nth-child(3) > img";
        public string JewelTypeElement { get; } = "#entorno-formulario > form:nth-child(15) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string JewelSubTypeList { get; } = "#container_10 > div > table > tbody > tr:nth-child(1) > td:nth-child(5) > img";
        public string JewelSubTypeElement { get; } = "#entorno-formulario > form:nth-child(16) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string JewelCaratList { get; } = "#container_10 > div > table > tbody > tr:nth-child(1) > td:nth-child(7) > img";
        public string JewelCaratElement { get; } = "#entorno-formulario > form:nth-child(17) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string JewelConditionList { get; } = "#container_10 > div > table > tbody > tr:nth-child(1) > td:nth-child(10) > img";
        public string JewelConditionElement { get; } = "#entorno-formulario > form:nth-child(18) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string JewelDescription { get; } = "#c_v8_observacion_0";
        public string JewelGrossWeight { get; } = "#c_v8_pesoBruto_0";
        public string JewelEmbeddedWeight { get; } = "#c_v8_pesoIncrustacion_0";
        public string CreditData { get; } = "a:has-text('Datos del Credito')";
        public string RequestedAmount { get; } = "#c_txtMtoPrestamo_0";
        public string LoanRate { get; } = "#c_txtTasaNegociable_0";
        public string PaymentTerm { get; } = "#c_txtPlazo_0";
        public string DisbursementOpType { get; } = "input[type='radio'][name='chekFPago'][value='0'][registro='0']";
        public string DisbursementAcType { get; } = "input[type='radio'][name='chekFPago'][registro='0']";
        public string BankAccount { get; } = "#container_11 > table > tbody > tr:nth-child(5) > td.columna_1 > img";
        public string ApplicationNumber { get; } = "#c_txtCsolicitud_0";
        public string IncomeButtton { get; } = "span:has-text('Ingresos y Descuentos')";
        public string IncomeCategoryList { get; } = "#container_5 > table > tbody > tr:nth-child(3) > td.columna_1 > img";
        public string IncomeCategoryElement { get; } = "#entorno-formulario > form:nth-child(5) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string IncomeDescription { get; } = "#c_txtActividad_0";
        public string IncomeContactNumberList { get; } = "#container_5 > table > tbody > tr:nth-child(5) > td.columna_1 > img";
        public string IncomeContactNumberElement { get; } = "#entorno-formulario > form:nth-child(6) > div > table > tbody > tr > td:nth-child(1)";
        public string IncomeDate { get; } = "#c_txtPeriodo1_0";
        public string IncomeAssets { get; } = "#c_txtImporte1_0";
        public string IncomeOther { get; } = "#c_txtImporte1_1";
        public string IncomeReturn { get; } = "span.link:has-text('Regresar')";
        public string EvaluateButton { get; } = "#c_Evaluar_0";
        public string EvaluateResult { get; } = "#c_resultadoCars_0";
    }
}
