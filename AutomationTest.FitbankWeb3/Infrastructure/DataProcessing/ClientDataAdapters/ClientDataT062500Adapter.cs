using System.Data;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.Interfaces;

namespace AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.ClientDataAdapters
{
    public class ClientDataT062500Adapter : IClientDataAdapter<ClientDataT062500>
    {
        public ClientDataT062500 Adapt(DataRow row)
        {
            return new ClientDataT062500
            {
                UserRequest = row.SafeField<string>("Usuario") ?? string.Empty,
                Identification = row.SafeField<string>("DNI") ?? string.Empty,
                Address = row.SafeField<int?>("Direccion") ?? 1,
                Group = row.SafeField<string>("Grupo") ?? string.Empty,
                Product = row.SafeField<string>("Producto") ?? string.Empty,
                LoanType = row.SafeField<LoanType?>("TipoPrestamo") ?? LoanType.Prestamo,
                LoanInstallments = row.SafeField<int?>("Cuotas") ?? 0,
                LoanAmount = row.SafeField<double?>("Monto") ?? 0.0,
                Income = row.SafeField<double?>("Haberes") ?? 0.0,
                PayrollSource = row.SafeField<PayrollSourceType?>("OrigenPlanilla") ?? PayrollSourceType.DireccionEconomia,
                DisbursementType = row.SafeField<DisbursementType?>("FormaDesembolso") ?? DisbursementType.Unspecified,
                ModifyLoanApplication = row.SafeField<ModifyLoanApplication?>("ModificarSolicitud") ?? ModifyLoanApplication.Default,
                RequestState = row.SafeField<RequestStatus?>("TipoEstatus") ?? RequestStatus.APROBAR,
                RequestType = row.SafeField<RequestType?>("TipoSolicitud") ?? RequestType.Excepcion,
                RequestObservation1 = row.SafeField<string>("TipoObservacion") ?? string.Empty,
                RequestObservation2 = row.SafeField<string>("TipoObservacion2") ?? string.Empty
            };
        }
    }
}