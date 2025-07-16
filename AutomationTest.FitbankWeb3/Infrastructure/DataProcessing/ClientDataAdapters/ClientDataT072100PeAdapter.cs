using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.Interfaces;

namespace AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.ClientDataAdapters
{
    public class ClientDataT072100PeAdapter : IClientDataAdapter<ClientDataT072100Pe>
    {
        public ClientDataT072100Pe Adapt(DataRow row)
        {
            return new ClientDataT072100Pe
            {
                UserRequest = row.SafeField<string>("Usuario") ?? string.Empty,
                Identification = row.SafeField<string>("DNI") ?? string.Empty,
                Address = row.SafeField<int?>("Direccion") ?? 1,
                Product = row.SafeField<string>("Producto") ?? string.Empty,
                LoanType = row.SafeField<LoanType?>("TipoPrestamo") ?? LoanType.Prestamo,
                LoanAmount = row.SafeField<double?>("Monto") ?? 0.0,
                LoanInstallments = row.SafeField<int?>("Plazo") ?? 0,
                ClientType = row.SafeField<ClientType?>("TipoCliente") ?? ClientType.A,
                RiskType = row.SafeField<RisktType?>("TipoRiesgo") ?? RisktType.Bajo,
                DisbursementType = row.SafeField<DisbursementType?>("FormaDesembolso") ?? DisbursementType.AbonoACuenta,
                GuaranteeType = row.SafeField<GuaranteeType?>("TipoGarantia") ?? GuaranteeType.SinGarantia,
                ModifyLoanApplication = row.SafeField<ModifyLoanApplication?>("ModificarSolicitud") ?? ModifyLoanApplication.Default,
            };
        }
    }
}