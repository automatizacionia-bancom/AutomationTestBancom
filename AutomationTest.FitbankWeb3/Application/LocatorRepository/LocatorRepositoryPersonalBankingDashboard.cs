namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class LocatorRepositoryPersonalBankingDashboard
    {
        public string CalificationResultSection { get; } = "a:has-text('Resultado Calificacion')";
        public string ApprovalSection { get; } = ":is(li.tab-bar-txtTrxAprobacion, li.tab-bar-062810) >> role=link[name='Aprobaciones']"; //"a:has-text('Aprobaciones')"; 
        public string ViewCarsButton { get; } = "span:has-text('Ver Criterios de Aceptacion de Riesgos')";
        public string CarsTable { get; } = "#container_2 > div > table > tbody";
        public string CarsReturn { get; } = "span:has-text('Regresar')";
        public string RequestType { get; } = "#c_cmbTipoSolicitud_0";
        public string RequestState { get; } = "#c_cmbEstado_0";
        public string RequestComment { get; } = "#c_txtComentario_0";
        public string RequestObservation1 { get; } = "#container_4 > table > tbody > tr:nth-child(2) > td.columna_4 > img";
        public string RequestObservation2 { get; } = "#container_4 > table > tbody > tr:nth-child(3) > td.columna_4 > img";
        public string ApplicationNumberSearchUsers { get; } = "#c_txtNroSolicitudC_0";
        public string ApprovalUsersButton { get; } = "#container_2 > div > table > tbody > tr:nth-child(1) > td.usaIcono.button.list-of-values.none > img";
        public string AprovalUsersList { get; } = "#entorno-formulario > form:nth-child(7) > div > table";
        public string ApprovalStatus { get; } = "#c_txtEstado_0";
        public string ApprovalProcess { get; } = "#c_txtProceso_0";
        public string EvaluationResult { get; } = "#c_resultadoCars_0";
        public string ApplicationNumberSearchTransaction { get; } = "#c_v1_cSolicitud_0";
        public string ApplicationNumberSearchTransactionResult { get; } = "#c_v1_solicitud_0";
    }
}
