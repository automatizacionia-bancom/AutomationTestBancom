using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.Interfaces;

namespace AutomationTest.FitbankWeb3.Infrastructure.Adapters.ClientDataAdapters
{
    public class ClientDataT062800Adapter : IClientDataAdapter<ClientDataT062800>
    {
        public ClientDataT062800 Adapt(DataRow row)
        {
            return new ClientDataT062800
            {
                UserRequest = row.SafeField<string>("Usuario") ?? string.Empty,
                Identification = row.SafeField<string>("DNI") ?? string.Empty,
                Address = row.SafeField<int?>("Direccion") ?? 1,
                ProductGroup = row.SafeField<string>("Grupo") ?? string.Empty,
                Product = row.SafeField<string>("Producto") ?? string.Empty,
                CoinType = row.SafeField<CoinType?>("Moneda") ?? CoinType.Soles,
                GuaranteeType = row.SafeField<GuaranteeType?>("Garantia") ?? GuaranteeType.SinGarantia,
                LoanType = row.SafeField<LoanType?>("TipoPrestamo") ?? LoanType.Prestamo,
                LoanInstallments = row.SafeField<int?>("Cuotas") ?? 0,
                LoanAmount = row.SafeField<double?>("Monto") ?? 0.0,
                Income = row.SafeField<double?>("SueldoNeto") ?? 0.0,
                DisbursementType = row.SafeField<DisbursementType?>("FormaDesembolso") ?? DisbursementType.Unspecified,
                ModifyLoanApplication = row.SafeField<ModifyLoanApplication?>("ModificarSolicitud") ?? ModifyLoanApplication.Default,
                RequestState = row.SafeField<RequestStatus?>("TipoSolicitud") ?? RequestStatus.APROBAR,
                RequestType = row.SafeField<RequestType?>("OrigenPlanilla") ?? RequestType.Excepcion,
                RequestObservation1 = row.SafeField<string>("TipoObservacion") ?? string.Empty,
                RequestObservation2 = row.SafeField<string>("TipoObservacion2") ?? string.Empty
            };
        }
    }
}