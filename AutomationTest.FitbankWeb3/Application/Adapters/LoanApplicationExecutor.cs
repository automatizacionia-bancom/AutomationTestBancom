using System.Collections;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;

namespace AutomationTest.FitbankWeb3.Application.Adapters
{
    public class LoanApplicationExecutor : ILoanApplicationExecutor
    {
        private readonly ITestConfigurationProvider _config;
        private readonly ITestDataProvider _dataProvider;
        private readonly ITransactionDataResolver _resolver;
        private readonly ILoanApplicationOrchestrator _orchestrator;
        //private readonly ICaseReportWriter _reportWriter;
        private readonly ITestOutputAccessor _outputAccessor;

        public LoanApplicationExecutor(
            ITestConfigurationProvider config,
            ITestDataProvider dataProvider,
            ITransactionDataResolver resolver,
            ILoanApplicationOrchestrator orchestrator,
            //ICaseReportWriter reportWriter,
            ITestOutputAccessor outputAccessor)
        {
            _config = config;
            _dataProvider = dataProvider;
            _resolver = resolver;
            _orchestrator = orchestrator;
            //_reportWriter = reportWriter;
            _outputAccessor = outputAccessor;
        }
        public async Task ExecuteWorkflow(LoanApplicationWorkflowModel<IClientData> fullLoanRequest)
        {
            if (!Directory.Exists(fullLoanRequest.EvidenceFolder))
            {
                Directory.CreateDirectory(fullLoanRequest.EvidenceFolder);
            }

            await ExecuteTypedTransactionAsync(fullLoanRequest);

            //await _reportWriter.WriteAsync(new CaseReportModel
            //{
            //    CaseIndex = _config.TestCaseList.IndexOf(fullLoanRequest.ClientData) + 1,
            //    ApplicationNumber = fullLoanRequest.ClientData.ApplicationNumber,
            //    Success = true, // Aquí se puede mejorar para capturar el éxito real
            //    Message = "Transacción completada con éxito",
            //    Timestamp = DateTime.UtcNow
            //});
        }
        public IEnumerable<LoanApplicationWorkflowModel<IClientData>> LoadCases()
        {
            // 1) Determina el tipo de ClientData a usar
            var txType = _config.TransactionType;
            var clientDataType = _resolver.GetDataType<IClientData>(txType);

            // 2) Invoca ITestDataProvider.GetTestCases<TClientData>
            var generic = typeof(ITestDataProvider)
                .GetMethod(nameof(ITestDataProvider.GetTestCases))!
                .MakeGenericMethod(clientDataType);

            var rawList = (IEnumerable)generic.Invoke(
                _dataProvider,
                new object[] { _config.ExcelPath, _config.SheetName }
            )!;

            // 3) Obtén la lista de casos a ejecutar (índices 1‑based)
            var selected = _config.TestCaseList; // List<int>
            bool all = !selected.Any(); // Si no contiene elementos se considera "todos los casos"

            // 4) Filtra y proyecta
            int index = 1;
            foreach (var cd in rawList.Cast<IClientData>())
            {
                if (all || selected.Contains(index))
                {
                    var caseFolder = Path.Combine(_config.EvidenceFolderBase, $"Caso {index}");
                    yield return new LoanApplicationWorkflowModel<IClientData>
                    {
                        ClientData = cd,
                        EvidenceFolder = caseFolder,
                        IpPort = _config.IpPort,
                        Headless = _config.Headless,
                        KeepPdf = _config.KeepPdf,
                    };
                }
                index++;
            }
        }
        /// <summary>
        /// Ejecuta la transacción tipada para el flujo de solicitud de préstamo.
        /// </summary>
        /// <param name="loanRequestApplication">Modelo genérico de flujo de solicitud de préstamo.</param>
        private async Task ExecuteTypedTransactionAsync(LoanApplicationWorkflowModel<IClientData> loanRequestApplication)
        {
            // 1) Descubre el tipo concreto de TClientData
            var clientData = loanRequestApplication.ClientData!;
            var clientType = clientData.GetType(); // Ejemplo: typeof(ClientDataT062900)

            // 2) Crea dinámicamente una instancia de LoanApplicationWorkflowModel<TClientData>
            var fullReqType = typeof(LoanApplicationWorkflowModel<>).MakeGenericType(clientType);
            var typedReq = Activator.CreateInstance(fullReqType)!;

            // 3) Copia las propiedades del modelo genérico al modelo tipado
            foreach (var prop in fullReqType.GetProperties().Where(p => p.CanWrite))
            {
                // Obtiene el valor de la propiedad desde el modelo genérico
                var value = typeof(LoanApplicationWorkflowModel<IClientData>)
                    .GetProperty(prop.Name)!.GetValue(loanRequestApplication);
                prop.SetValue(typedReq, value);
            }

            // 4) Invoca TransactionAsync<TClientData>(typedReq) usando reflexión
            var method = _orchestrator
               .GetType()
               .GetMethod(nameof(ILoanApplicationOrchestrator.TransactionAsync))!
               .MakeGenericMethod(clientType);

            var task = (Task)method.Invoke(_orchestrator, new object[] { typedReq })!;

            // 5) Espera la finalización de la tarea
            await task;
        }
    }
}
