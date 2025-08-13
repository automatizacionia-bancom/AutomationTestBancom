namespace AutomationTest.FitbankWeb3.Infrastructure.Configuration
{
    public class TransactionUserProviderSettings
    {
        /// <summary>
        /// “Json”, “Database”, “Api”, etc.
        /// </summary>
        public string ProviderType { get; set; } = "Json";

        /// <summary>
        /// Ruta al JSON, cadena de conexión, URL, según el ProviderType
        /// </summary>
        public string Connection { get; set; } = string.Empty;
    }
}
