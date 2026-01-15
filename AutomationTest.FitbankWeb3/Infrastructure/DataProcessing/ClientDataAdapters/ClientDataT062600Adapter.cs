using System.Data;
using AutomationTest.FitbankWeb3.Application.Enums;
using AutomationTest.FitbankWeb3.Application.Enums.BusinessEnum;
using AutomationTest.FitbankWeb3.Application.Models.ClientDataModels;
using AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.Interfaces;

namespace AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.ClientDataAdapters
{
    public class ClientDataT062600Adapter : IClientDataAdapter<ClientDataT062600>
    {
        public ClientDataT062600 Adapt(DataRow row)
        {
            return new ClientDataT062600
            {
                UserRequest = row.SafeField<string>("Usuario") ?? string.Empty,
                Identification = row.SafeField<string>("DNI") ?? string.Empty,
                Address = row.SafeField<int?>("Direccion") ?? 1,
                Product = row.SafeField<string>("Producto") ?? string.Empty,
                MortgageInsurance1 = row.SafeField<InsuranceType?>("TipoSeguro1") ?? InsuranceType.Propio,
                MortgageInsurance2 = row.SafeField<InsuranceType?>("TipoSeguro2") ?? InsuranceType.Individual,
                MortgageGood = row.SafeField<MortgageGoodType?>("TipoDeBien") ?? MortgageGoodType.BienFuturo,
                MortgageProject = row.SafeField<MortgageProjectType?>("TipoDeProyecto") ?? MortgageProjectType.ProyectoPropio,
                EstimatedValue = row.SafeField<double?>("ValorEstimado") ?? 0.0,
                DownPayment = row.SafeField<double?>("CuotaInicial") ?? 0.0,
                LoanInstallments = row.SafeField<int?>("NumeroCuotas") ?? 0,
                LoanRate = row.SafeField<double?>("Tasa") ?? 0.0,
                GuaranteeType = row.SafeField<GuaranteeType?>("Garantia") ?? GuaranteeType.SinGarantia,
                Income = row.SafeField<double?>("IngresoNeto") ?? 0.0,
                BoundOptions = row.SafeField<BoundOptionsType?>("OpcionDeBono") ?? BoundOptionsType.Unspecified,
                MortgageBond = row.SafeField<MortgageBondType?>("Bono") ?? MortgageBondType.SinBono,
                ModifyLoanApplication = row.SafeField<ModifyLoanApplication?>("ModificarSolicitud") ?? ModifyLoanApplication.Default,
                RequestState = row.SafeField<RequestStatus?>("TipoEstatus") ?? RequestStatus.APROBAR,
                RequestType = row.SafeField<RequestType?>("TipoSolicitud") ?? RequestType.Excepcion,
                RequestObservation1 = row.SafeField<string>("TipoObservacion") ?? string.Empty,
                RequestObservation2 = row.SafeField<string>("TipoObservacion2") ?? string.Empty
            };
        }
    }
}