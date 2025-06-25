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
    public class ClientDataT062900Adapter : IClientDataAdapter<ClientDataT062900>
    {
        public ClientDataT062900 Adapt(DataRow row)
        {
            return new ClientDataT062900
            {
                UserRequest = row.SafeField<string>("Usuario") ?? string.Empty,
                Identification = row.SafeField<string>("DNI") ?? string.Empty,
                Address = row.SafeField<int?>("Direccion") ?? 1,
                Product = row.SafeField<string>("Producto") ?? string.Empty,
                LoanType = row.SafeField<LoanType?>("TipoPrestamo") ?? LoanType.Prestamo,
                LoanInstallments = row.SafeField<int?>("Cuotas") ?? 0,
                LoanAmount = row.SafeField<double?>("Monto") ?? 0.0,
                Income = row.SafeField<double?>("OtrosIngresos") ?? 0.0,
                PayrollSource = row.SafeField<PayrollSourceType?>("OrigenPlanilla") ?? PayrollSourceType.DireccionEconomia,
                DisbursementType = row.SafeField<DisbursementType?>("FormaDesembolso") ?? DisbursementType.Unspecified,
                ModifyLoanApplication = row.SafeField<ModifyLoanApplication?>("OrigenPlanilla") ?? ModifyLoanApplication.Default,
                RequestState = row.SafeField<RequestStatus?>("OrigenPlanilla") ?? RequestStatus.APROBAR,
                RequestType = row.SafeField<RequestType?>("OrigenPlanilla") ?? RequestType.Excepcion,
                RequestObservation1 = row.SafeField<string>("TipoObservacion") ?? string.Empty,
                RequestObservation2 = row.SafeField<string>("TipoObservacion2") ?? string.Empty
            };
        }
    }
}