using System.Text;
using AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.Configuration;

namespace AutomationTest.FitbankWeb3.Infrastructure.Adapters.FileProcessing
{
    public class TxtCaseReportWriter : ICaseReportWriter
    {
        private readonly StreamWriter _writer;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public TxtCaseReportWriter(IConfiguration config)
        {
            // 1) Leemos la ruta del reporte de la configuración
            var evidence = config["TestData:EvidenceFolderBase"]
                             ?? throw new InvalidOperationException("Missing EvidenceFolderBase");
            var filePath = Path.Combine(evidence, "Reporte de casos.txt");

            // 2) Abrimos el fichero UNA sola vez (singleton) y escribimos el header
            _writer = new StreamWriter(filePath, append: false, Encoding.UTF8)
            {
                AutoFlush = true
            };
            _writer.WriteLine("CaseIndex\tApplicationNumber\tSuccess\tMessage\tTimestamp");
        }

        public async Task WriteAsync(CaseReportModel report)
        {
            // 3) Cada llamada escribe una línea y hace flush inmediatamente
            await _lock.WaitAsync();
            try
            {
                await _writer.WriteLineAsync(
                    $"{report.CaseIndex}\t" +
                    $"{report.ApplicationNumber}\t" +
                    $"{report.Success}\t" +
                    $"{report.Message}\t" +
                    $"{report.Timestamp:O}"
                );
            }
            finally
            {
                _lock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            // 4) Se cierra al final de la aplicación
            await _writer.DisposeAsync();
            _lock.Dispose();
        }
    }
}
