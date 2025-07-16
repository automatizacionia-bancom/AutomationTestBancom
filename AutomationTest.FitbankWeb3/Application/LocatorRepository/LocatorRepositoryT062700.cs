using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class LocatorRepositoryT062700
    {
        public string OK_ApprovalError { get; } = "#entorno-estatus-contenido:text-matches(\"^(?:Ok|Error al procesar la consulta)$\")";
        public string ApprovalError { get; } = "#entorno-estatus-contenido:text('Error al procesar la consulta')";
        public string Identification { get; } = "#c_v2_identificacion_0";
        public string AdressList { get; } = "#container_3 > div > table > tbody > tr:nth-child(1) > td:nth-child(11) > img";
        public string AddressElement { get; } = "#entorno-formulario > form:nth-child(7) > div > table > tbody > tr > td:nth-child(1)";
        public string ProductList { get; } = "#container_5 > table > tbody > tr:nth-child(1) > td.columna_1 > img:nth-child(2)";
        public string ManagerList { get; } = "#container_5 > table > tbody > tr:nth-child(2) > td.columna_3 > img:nth-child(2)";
        public string ManagerElement { get; } = "#entorno-formulario > form:nth-child(10) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string CreditLine { get; } = "#c_txtMtoPrestamo_0";
        public string LoanRate { get; } = "#c_txtTasaNegociable_0";
        public string ExternalEvaluation { get; } = "#c_v6_evalExterna_0";
        public string CreditDataLabel { get; } = "#entorno-formulario > div.entorno-html > form > fieldset:nth-child(7) > legend";
        public string Emboss { get; } = "#c_v6_emboss_0";
        public string TcrList { get; } = "#container_6 > table > tbody > tr:nth-child(1) > td.columna_3 > img:nth-child(2)";
        public string TcrElement { get; } = "#entorno-formulario > form:nth-child(13) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string EeccList { get; } = "#container_6 > table > tbody > tr:nth-child(2) > td.columna_3 > img:nth-child(2)";
        public string EcccElement { get; } = "#entorno-formulario > form:nth-child(15) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string BillingCycleList { get; } = "#container_6 > table > tbody > tr:nth-child(5) > td.columna_1 > img";
        public string ApplicationNumber { get; } = "#c_txtCsolicitud_0";
        public string IncomeButtton { get; } = "span:has-text(\"Ingresos/Descuentos\")";
        public string IncomeDate { get; } = "#c_txtPeriodo1_0";
        public string IncomeAssets { get; } = "#c_txtImporte1_0";
        public string IncomeOther1 { get; } = "#c_txtImporte1_3";
        public string IncomeOther2 { get; } = "#c_txtImporte1_4";
        public string IncomeReturn { get; } = "span.link:has-text('Regresar')";
        public string GuaranteeButton { get; } = "#c_chk_garantia_0";
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
