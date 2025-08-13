namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class LocatorRepositoryGeneralDashboard
    {
        public string TransactionInput { get; } = "#entorno-pt";
        public string F12Button { get; } = "button[title='Guardar']";
        public string F7Button { get; } = "button[title='Consultar']";
        public string OK { get; } = "#entorno-estatus-contenido:text('Ok')";
        public string TransactionCorrect { get; } = "#entorno-estatus-contenido:text('TRANSACCION REALIZADA CORRECTAMENTE')";
        public string FormCorrect { get; } = "#entorno-estatus-contenido:text('Formulario cargado correctamente')";
        public string FormProcessing { get; } = "#entorno-estatus-contenido.processing";
        public string OK_TransactionCorrect { get; } = "#entorno-estatus-contenido:text-matches(\"^(?:Ok|TRANSACCION REALIZADA CORRECTAMENTE)$\")";
        public string TransactionError { get; } = "#entorno-estatus-contenido.error";
        public string TransactionNotAllowed { get; } = "#entorno-estatus-contenido:has-text('TRANSACCION NO PERMITIDA PARA ESTE ROL')";
        public string NonAuthorizingUsers { get; } = "#entorno-estatus-contenido:text('NO TIENE AUTORIZADORES')";
        public string ListElement(string elementName) => $"td:text-matches(\"^{elementName}$\", \"\")";
        public string ListElementPattern(string elementName) => $"td:text-matches(\"{elementName}\")";
    }
}
