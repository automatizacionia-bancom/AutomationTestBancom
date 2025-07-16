using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class LocatorRepositoryT062500
    {
        public string Identification { get; } = "#c_v2_identificacion_0";
        public string Adress { get; } = "#c_v2_cdireccion_0";
        public string AdressList { get; } = "#container_3 > div > table > tbody > tr:nth-child(1) > td:nth-child(12) > img";
        public string AddressElement { get; } = "#entorno-formulario > form:nth-child(7) > div > table > tbody > tr > td:nth-child(1)";
        public string ProductList { get; } = "#container_6 > table > tbody > tr:nth-child(1) > td.columna_1 > img";
        public string ManagerList { get; } = "#container_6 > table > tbody > tr:nth-child(1) > td.columna_4 > img:nth-child(3)";
        public string ManagerElement { get; } = "#entorno-formulario > form:nth-child(12) > div > table > tbody > tr:nth-child(1) > td:nth-child(1)";
        public string LoanTypeList { get; } = "#container_7 > table > tbody > tr:nth-child(1) > td.columna_1 > img";
        public string LoanInstallments { get; } = "#c_txtCuotas_0";
        public string LoanAmount { get; } = "#c_txtMtoPrestamo_0";
        public string LoanRate { get; } = "#c_txtTasaNegociable_0";
        public string CreditDataLabel { get; } = "#entorno-formulario > div.entorno-html > form > fieldset:nth-child(8) > legend";
        public string ApprovalDate { get; } = "#c_txtFechaAprobacion_0";
        public string RemittanceNumber { get; } = "#c_txtNumeroRemesa_0";
        public string PayrollSource { get; } = "#c_cbbOrigenPlanilla_0";
        public string ApplicationNumber { get; } = "#c_txtCsolicitud_0";
        public string DisbursementOpType { get; } = "input[type='radio'][name='chekFPago'][value='0']";
        public string BankAccountList { get; } = "#container_8 > table > tbody > tr:nth-child(2) > td.columna_5 > img";
        public string BankAccountElement { get; } = "#entorno-formulario > form:nth-child(16) > div > table > tbody > tr > td";
        public string IncomeButtton { get; } = "span.input-label:has-text('Ingresos y Dsctos.')";
        public string IncomeDate { get; } = "#c_txtPeriodo1_0";
        public string IncomeAssets { get; } = "#c_txtImporte1_0";
        public string IncomeOther { get; } = "#c_txtImporte1_15";
        public string IncomeReturn { get; } = ".return-link:has-text('Regresar')";
        public string EvaluateButton { get; } = "#c_Evaluar_0";
        public string EvaluateResult { get; } = "#c_resultadoCars_0";
    }
}
