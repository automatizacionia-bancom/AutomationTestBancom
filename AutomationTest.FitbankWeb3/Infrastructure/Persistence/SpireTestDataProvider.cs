using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Models.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spire.Xls;

namespace AutomationTest.FitbankWeb3.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación usando Spire.XLS; el path se pasa al método.
    /// </summary>
    public class SpireTestDataProvider : ITestDataProvider
    {
        private readonly IServiceProvider _sp;

        public SpireTestDataProvider(IServiceProvider sp)
        {
            _sp = sp;
        }

        public IEnumerable<TClientData> GetTestCases<TClientData>(
            string filePath,
            string sheetName)
            where TClientData : IClientData
        {
            // 1) Carga el Excel
            var wb = new Workbook();
            wb.LoadFromFile(filePath);

            var sheet = wb.Worksheets[sheetName]
                        ?? throw new InvalidOperationException(
                            $"Hoja '{sheetName}' no existe en '{filePath}'.");

            // 2) Exporta a DataTable
            var dt = sheet.ExportDataTable(
                exportColumnNames: true,
                firstRow: 1, firstColumn: 1,
                maxRows: sheet.LastRow, maxColumns: sheet.LastColumn);

            // 2.1) Elimina filas vacías **solamente al final**
            var trimmed = TrimTrailingEmptyRows(dt);

            // 3) Adapta cada fila (saltando encabezado en row 0)
            var adapter = _sp.GetRequiredService<IClientDataAdapter<TClientData>>();
            foreach (DataRow row in trimmed.Rows.Cast<DataRow>())
            {
                yield return adapter.Adapt(row);
            }
        }
        /// <summary>
        /// Quita las filas vacías al final del DataTable, dejando intactas
        /// las vacías que estén en medio.
        /// </summary>
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
