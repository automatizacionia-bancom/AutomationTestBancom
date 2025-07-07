using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Spire.Pdf;

namespace AutomationTest.FitbankWeb3.Infrastructure.Adapters.FileProcessing
{
    public class CrossPlatformPdfConverter : IPdfConverter
    {
        public async Task<bool> ConvertAllPagesToImgAsync(
            string pdfPath,
            string prefix,
            string outputFolder,
            int dpi = 300)
        {
            if (string.IsNullOrEmpty(pdfPath) || !File.Exists(pdfPath))
                throw new FileNotFoundException($"PDF file not found: {pdfPath}");

            if (string.IsNullOrEmpty(outputFolder))
                throw new ArgumentException("Output folder cannot be null or empty", nameof(outputFolder));

            if (dpi <= 0)
                throw new ArgumentException("DPI must be greater than 0", nameof(dpi));

            bool anyPageConverted = false;
            Directory.CreateDirectory(outputFolder);

            var pageDimensions = new PageDimensions(dpi, dpi);
            using var docReader = DocLib.Instance.GetDocReader(pdfPath, pageDimensions);
            int pageCount = docReader.GetPageCount();

            for (int pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
                try
                {
                    await ConvertSinglePageAsync(docReader, pageIndex, prefix, outputFolder, dpi);
                    anyPageConverted = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error converting page {pageIndex + 1}: {ex.Message}");
                }
            }

            return anyPageConverted;
        }

        private async Task ConvertSinglePageAsync(
            IDocReader docReader,
            int pageIndex,
            string prefix,
            string outputFolder,
            int dpi)
        {
            using var pageReader = docReader.GetPageReader(pageIndex);

            int width = pageReader.GetPageWidth();
            int height = pageReader.GetPageHeight();
            var rawBytes = pageReader.GetImage();

            // Carga el bitmap BGRA y conviértelo a Rgba32
            using var pageImage = Image.LoadPixelData<Bgra32>(rawBytes, width, height)
                                       .CloneAs<Rgba32>();

            // Crea un lienzo en blanco
            using var canvas = new Image<Rgba32>(width, height);
            canvas.Mutate(ctx =>
            {
                ctx.Fill(Color.White);         // usa Color.White
                ctx.DrawImage(pageImage, 1f);  // dibuja la página encima
            });

            // Ajusta metadata de resolución
            canvas.Metadata.HorizontalResolution = dpi;
            canvas.Metadata.VerticalResolution = dpi;

            // Guarda PNG
            string fileName = $"{prefix}{pageIndex + 1:D2}.jpeg";
            string outputPath = Path.Combine(outputFolder, fileName);
            await canvas.SaveAsPngAsync(outputPath);
        }
    }
}
