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
    public class ClientDataT062700Adapter : IClientDataAdapter<ClientDataT062700>
    {
        public ClientDataT062700 Adapt(DataRow row)
        {
            return new ClientDataT062700
            {
                UserRequest = row.SafeField<string>("Usuario") ?? string.Empty,
                Identification = row.SafeField<string>("DNI") ?? string.Empty,
                Address = row.SafeField<int?>("Direccion") ?? 1,
                Product = row.SafeField<string>("Producto") ?? string.Empty,
                CreditLine = row.SafeField<double?>("LineaCredito") ?? 0.0,
                BillingCycle = row.SafeField<BillingCycle?>("CicloFacturacion") ?? BillingCycle.PimeraQuincena,
                GuaranteeType = row.SafeField<GuaranteeType?>("Garantia") ?? GuaranteeType.SinGarantia,
                Income = row.SafeField<double?>("SueldoBruto") ?? 0.0,
                ModifyLoanApplication = row.SafeField<ModifyLoanApplication?>("ModificarSolicitud") ?? ModifyLoanApplication.Default,
                RequestState = row.SafeField<RequestStatus?>("TipoSolicitud") ?? RequestStatus.APROBAR,
                RequestType = row.SafeField<RequestType?>("TipoSolicitud") ?? RequestType.Excepcion,
                RequestObservation1 = row.SafeField<string>("TipoObservacion") ?? string.Empty,
                RequestObservation2 = row.SafeField<string>("TipoObservacion2") ?? string.Empty
            };
        }
    }
}          