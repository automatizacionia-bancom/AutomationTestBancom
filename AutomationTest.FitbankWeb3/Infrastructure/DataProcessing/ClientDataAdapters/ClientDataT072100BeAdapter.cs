using System.Data;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.Interfaces;

namespace AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.ClientDataAdapters
{
    public class ClientDataT072100BeAdapter : IClientDataAdapter<ClientDataT072100Be>
    {
        public ClientDataT072100Be Adapt(DataRow row)
        {
            return new ClientDataT072100Be
            {
                UserRequest = row.SafeField<string>("Usuario") ?? string.Empty,
                Identification = row.SafeField<string>("DNI") ?? string.Empty,
                Address = row.SafeField<int?>("Direccion") ?? 1,
                RMG = row.SafeField<double?>("RMG") ?? 0.0,
                ClientType = row.SafeField<ClientType?>("TipoCliente") ?? ClientType.A,
                GuaranteeType = row.SafeField<GuaranteeType?>("TipoGarantia") ?? GuaranteeType.SinGarantia,
                ModifyLoanApplication = row.SafeField<ModifyLoanApplication?>("ModificarSolicitud") ?? ModifyLoanApplication.Default,
            };
        }
    }
}