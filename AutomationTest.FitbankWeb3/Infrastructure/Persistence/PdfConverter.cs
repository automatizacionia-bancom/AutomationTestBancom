using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Ghostscript.NET;
using Ghostscript.NET.Processor;

namespace AutomationTest.FitbankWeb3.Infrastructure.Persistence
{
    public class PdfConverter : IPdfConverter
    {
        public async Task<bool> ConvertAllPagesToPngAsync(string pdfPath, string prefix, string outputFolder, int dpi)
        {
            // 1) Validar que el archivo PDF exista
            if (!File.Exists(pdfPath))
                throw new FileNotFoundException($"No se encontró el archivo PDF en '{pdfPath}'.");

            // 2) Crear la carpeta de salida si no existe
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // 3) Obtener la versión de Ghostscript
            GhostscriptVersionInfo gsVersion = GhostscriptVersionInfo.GetLastInstalledVersion();

            var outputPattern = Path.Combine(outputFolder, $"{prefix} %02d.jpg");
            var switches = new List<string>
            {
                "-dNOPAUSE",
                "-dBATCH",
                "-dSAFER",
                "-sDEVICE=png16m",
                $"-r{dpi}",
                $"-sOutputFile={outputPattern}",
                pdfPath
            };

            // 5) Invocar a GhostscriptProcessor para procesar
            await Task.Run(() =>
            {
                using (var processor = new GhostscriptProcessor(gsVersion, true))
                {
                    processor.StartProcessing(switches.ToArray(), null);
                }
            });

            // 6) (Opcional) Listar los archivos generados para confirmación
            var generatedFiles = Directory
                .EnumerateFiles(outputFolder, $"{prefix} *.jpg")
                .OrderBy(f => f)
                .ToList();

            return generatedFiles.Any();
        }
    }
}
