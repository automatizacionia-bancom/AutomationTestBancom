using System.Data;
using System.Data.Odbc;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;

namespace AutomationTest.FitbankWeb3.Infrastructure.Adapters.Database
{
    public class Db2GenericQueryExecutor : IGenericQueryExecutor
    {
        private readonly string _connectionString;
        public Db2GenericQueryExecutor(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }
        public async Task<DataTable> ExecuteAsync(GenericQueryModel queryModel)
        {
            if (queryModel == null)
                throw new ArgumentNullException(nameof(queryModel));
            if (string.IsNullOrWhiteSpace(queryModel.Query))
                throw new ArgumentException("El campo 'Query' no puede estar vacío.", nameof(queryModel));

            // Convertir timeout de ms a segundos
            var timeoutSeconds = queryModel.Timeout / 1000;
            if (timeoutSeconds <= 0) timeoutSeconds = 30;

            var resultTable = new DataTable();

            try
            {
                // OdbcConnection no tiene OpenAsync, pero el resto puede esperar.
                using var connection = new OdbcConnection(_connectionString);
                connection.Open();

                using var command = new OdbcCommand(queryModel.Query, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = timeoutSeconds
                };

                // Detectar si es SELECT/consulta
                var trimmed = queryModel.Query.TrimStart();
                bool isSelect = trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase)
                             || trimmed.StartsWith("WITH", StringComparison.OrdinalIgnoreCase);

                if (isSelect)
                {
                    // Ejecutar SELECT y cargar DataTable
                    using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                    resultTable.Load(reader);
                }
                else
                {
                    // Ejecutar non-query y devolver filas afectadas
                    int affected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    resultTable.Columns.Add("RowsAffected", typeof(int));
                    var row = resultTable.NewRow();
                    row["RowsAffected"] = affected;
                    resultTable.Rows.Add(row);
                }

                return resultTable;
            }
            catch (OdbcException odbcEx)
            {
                if (queryModel.ThrowOnError)
                {
                    throw new InvalidOperationException(odbcEx.Message);
                }
                return new DataTable();
            }
            catch (Exception)
            {
                if (queryModel.ThrowOnError)
                    throw;
                return new DataTable();
            }
        }
    }
}