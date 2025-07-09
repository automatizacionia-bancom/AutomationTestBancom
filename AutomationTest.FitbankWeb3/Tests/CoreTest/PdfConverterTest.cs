using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using AutomationTest.FitbankWeb3.Infrastructure.Adapters.FileProcessing;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    [Trait("Grupo", "CoreTest")]
    public class PdfConverterTest : IDisposable
    {
        private readonly string _tempFolder;
        private readonly string _pdfPath;
        private readonly string _outputFolder;

        public PdfConverterTest()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolder);

            _pdfPath = Path.Combine(_tempFolder, "test.pdf");
            _outputFolder = Path.Combine(_tempFolder, "out");
        }

        [Fact]
        public async Task ConvertAllPagesToPngAsync_CreatesOnePngPerPage_AndImagesAreValid()
        {
            // 1) Generar un PDF de prueba de 2 páginas
            var document = new PdfDocument();
            for (int i = 1; i <= 2; i++)
            {
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawString(
                    $"Página de prueba #{i}",
                    new XFont("Arial", 20),
                    XBrushes.Black,
                    new XPoint(50, 50)
                );
            }

            document.Save(_pdfPath);

            // 2) Ejecutar el converter
            var converter = new CrossPlatformPdfConverter();
            bool result = await converter.ConvertAllPagesToImgAsync(
                pdfPath: _pdfPath,
                prefix: "pg_",
                outputFolder: _outputFolder,
                dpi: 150
            );

            // 3) Asegurar que devolvió true y que hay dos archivos
            Assert.True(result, "Debe devolver true cuando genera imágenes.");
            var files = Directory
                .EnumerateFiles(_outputFolder, "pg_*.jpeg")
                .OrderBy(f => f)
                .ToList();
            Assert.Equal(2, files.Count);

            // 4) Verificar que cada PNG se puede abrir y tiene dimensiones > 0
            foreach (var file in files)
            {
                Assert.True(File.Exists(file), $"No se encontró {file}");
                using var img = Image.Load<Rgba32>(file);
                Assert.True(img.Width > 0 && img.Height > 0,
                    $"Imagen {Path.GetFileName(file)} inválida o vacía.");
            }
        }
        public void Dispose()
        {
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, recursive: true);
        }
    }
}
