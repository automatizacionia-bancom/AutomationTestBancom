using System.Data;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.DataProcessing.ClientDataAdapters;
using Spire.Xls;

namespace AutomationTest.FitbankWeb3.Application.Adapters
{
    public class LoanApprovalExecutor : ILoanApprovalExecutor
    {
        private readonly ITestConfigurationProvider _config;
        private readonly ITestDataProvider _dataProvider;
        private readonly ITransactionDataResolver _resolver;
        private readonly ILoanApprovalOrchestrator _orchestrator;
        private readonly ITestOutputAccessor _outputAccessor;
        public LoanApprovalExecutor(
            ITestConfigurationProvider config,
            ITestDataProvider dataProvider,
            ITransactionDataResolver resolver,
            ILoanApprovalOrchestrator orchestrator,
            ITestOutputAccessor outputAccessor)
        {
            _config = config;
            _dataProvider = dataProvider;
            _resolver = resolver;
            _orchestrator = orchestrator;
            _outputAccessor = outputAccessor;
        }
        public async Task ExecuteWorkflow(LoanApprovalWorkflowModel<IClientData> fullLoanRequest)
        {
            if (!Directory.Exists(fullLoanRequest.EvidenceFolder))
            {
                Directory.CreateDirectory(fullLoanRequest.EvidenceFolder);
            }

            await ExecuteTypedTransactionAsync(fullLoanRequest);
        }
        public IEnumerable<LoanApprovalWorkflowModel<IClientData>> LoadCases()
        {
            const string sheetName = "QA";
            var wb = new Workbook();
            wb.LoadFromFile(_config.ApprovalCases);

            var sheet = wb.Worksheets[sheetName]
                        ?? throw new InvalidOperationException(
                            $"Hoja '{sheetName}' no existe en '{_config.ApprovalCases}'.");

            // 1) Exporta a DataTable
            var dt = sheet.ExportDataTable(
                exportColumnNames: true,
                firstRow: 1, firstColumn: 1,
                maxRows: sheet.LastRow, maxColumns: sheet.LastColumn);

            // 2) Elimina filas vacías **solamente al final**
            var trimmed = TrimTrailingEmptyRows(dt);

            // 3) Lista de índices a ejecutar (1‑based)
            var selected = _config.TestCaseList;
            bool all = !selected.Any(); // Si no contiene elementos se considera "todos los casos"

            // 4) Itera y filtra
            int index = 1;
            foreach (DataRow row in trimmed.Rows.Cast<DataRow>())
            {
                if (all || selected.Contains(index))
                {
                    yield return new LoanApprovalWorkflowModel<IClientData>
                    {
                        EvidenceFolder = row.SafeField<string>("CarpetaEvidencia") ?? string.Empty,
                        IpPort = _config.IpPort,
                        Headless = _config.Headless,
                        MaxApprovalUser = _config.MaxApprovalUser,
                        ApplicationNumber = row.SafeField<string>("NumeroSolicitud") ?? string.Empty,
                        Attempt = row.SafeField<int?>("Aprobacion") ?? 0,
                        RecognizedApprovingUsers = ParseUsers(row.SafeField<string>("Usuarios") ?? string.Empty),
                    };
                }
                index++;
            }
        }
        /// <summary>
        /// Ejecuta la transacción tipada de aprobación de préstamo.
        /// </summary>
        /// <param name="loanRequestApplication">Modelo genérico de la solicitud de préstamo.</param>
        private async Task ExecuteTypedTransactionAsync(LoanApprovalWorkflowModel<IClientData> loanRequestApplication)
        {
            // 1) Obtiene el tipo de transacción y el tipo concreto de datos de cliente
            var txType = _config.TransactionType;
            var clientType = _resolver.GetDataType<IClientData>(txType);

            // 2) Crea dinámicamente un LoanApprovalWorkflowModel<TClientData>
            var fullReqType = typeof(LoanApprovalWorkflowModel<>).MakeGenericType(clientType);
            var typedReq = Activator.CreateInstance(fullReqType)!;

            // 3) Copia las propiedades del modelo genérico al modelo tipado
            foreach (var prop in fullReqType.GetProperties().Where(p => p.CanWrite))
            {
                var value = typeof(LoanApprovalWorkflowModel<IClientData>)
                    .GetProperty(prop.Name)!.GetValue(loanRequestApplication);
                prop.SetValue(typedReq, value);
            }

            // 4) Invoca TransactionAsync<TClientData>(typedReq) usando reflexión
            var method = _orchestrator
               .GetType()
               .GetMethod(nameof(ILoanApprovalOrchestrator.TransactionAsync))!
               .MakeGenericMethod(clientType);

            var task = (Task)method.Invoke(_orchestrator, new object[] { typedReq })!;

            // 5) Espera la finalización de la tarea
            await task;
        }
        private List<string> ParseUsers(string usersCsv)
        {
            if (string.IsNullOrWhiteSpace(usersCsv))
                return new List<string>();

            return usersCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(u => u.Trim())
                .Where(u => !string.IsNullOrEmpty(u))
                .ToList();
        }
        private DataTable TrimTrailingEmptyRows(DataTable table)
        {
            int lastNonEmpty = -1;

            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                var row = table.Rows[i];
                bool allEmpty = true;

                foreach (var item in row.ItemArray)
                {
                    if (item != null
                        && item != DBNull.Value
                        && !string.IsNullOrWhiteSpace(item.ToString()))
                    {
                        allEmpty = false;
                        break;
                    }
                }

                if (!allEmpty)
                {
                    lastNonEmpty = i;
                    break;
                }
            }

            // Si todas las filas estaban vacías, devolvemos sólo el encabezado
            int rowsToKeep = Math.Max(lastNonEmpty + 1, 1);

            // Clonamos la estructura (columnas y esquema)
            var result = table.Clone();

            // Copiamos solo hasta rowsToKeep (excluye index >= rowsToKeep)
            for (int i = 0; i < rowsToKeep; i++)
                result.ImportRow(table.Rows[i]);

            return result;
        }
    }
}
