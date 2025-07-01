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
    public class ClientDataT062400Adapter : IClientDataAdapter<ClientDataT062400>
    {
        public ClientDataT062400 Adapt(DataRow row)
        {
            return new ClientDataT062400
            {
                UserRequest = row.SafeField<string>("Usuario") ?? string.Empty,
                Identification = row.SafeField<string>("DNI") ?? string.Empty,
                Address = row.SafeField<int?>("Direccion") ?? 1,
                Product = row.SafeField<string>("Producto") ?? string.Empty,
                JewelGrossWeight = row.SafeField<double?>("PesoBruto") ?? 0.0,
                JewelEmbeddedWeight = row.SafeField<double?>("PesoInscrustacion") ?? 0.0,
                RequestedAmount = row.SafeField<double?>("Monto") ?? 0.0,
                PaymentTerm = row.SafeField<PaymentTerm?>("Monto") ?? PaymentTerm.Monthly,
                Income = row.SafeField<double?>("IngresoNeto") ?? 0.0,
                DisbursementType = row.SafeField<DisbursementType?>("FormaDesembolso") ?? DisbursementType.Unspecified,
                ModifyLoanApplication = row.SafeField<ModifyLoanApplication?>("ModificarSolicitud") ?? ModifyLoanApplication.Default,
                RequestState = row.SafeField<RequestStatus?>("TipoSolicitud") ?? RequestStatus.APROBAR,
                RequestType = row.SafeField<RequestType?>("TipoSolicitud") ?? RequestType.Excepcion,
                RequestObservation1 = row.SafeField<string>("TipoObservacion") ?? string.Empty,
                RequestObservation2 = row.SafeField<string>("TipoObservacion2") ?? string.Empty
            };
        }
    }
}