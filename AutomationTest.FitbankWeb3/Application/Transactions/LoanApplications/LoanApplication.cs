using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AutomationTest.FitbankWeb3.Application.Models.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.LoanApplicationModels.Input;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Transactions.LoanApplications
{
    public abstract class LoanApplication<TClientData, TLoanResult> : ILoanApplication<TClientData, TLoanResult>
        where TClientData : IClientData
        where TLoanResult : ILoanApplicationResult
    {
        public abstract Task<TLoanResult> ApplyForLoanAsync(IPage page,LoanApplicationModel<TClientData> loanRequest);
        // Aquí puedes agregar métodos comunes para todas las implementaciones de LoanApplication, si es necesario.
        // Por ejemplo, podrías tener un método para validar los datos del cliente o para formatear la solicitud de préstamo.
        public async Task SearchProduct(IPage page, string product)
        {
            const string pattern = "**/proc/lv";
            Action<IRoute> handler = null!;

            handler = async route =>
            {
                // 1) Leer y decodificar el POST body
                var raw = route.Request.PostData;    // e.g. "_contexto=lv&_lv=%7B...%7D"
                var decoded = WebUtility.UrlDecode(raw);

                // 2) Separar parámetros y extraer JSON de _lv
                var nvc = HttpUtility.ParseQueryString(decoded);
                var prefix = decoded.Substring(0, decoded.IndexOf("_lv=") + "_lv=".Length);
                string lvJson = nvc["_lv"]!;

                // 3) Parsear a JsonNode y localizar el array "fields"
                var rootNode = JsonNode.Parse(lvJson)!.AsObject();
                var fieldsArray = rootNode["fields"]!.AsArray();

                bool modified = false;

                // 4) Cambiar únicamente los objetos con "alias":"TPP"
                foreach (var fieldNode in fieldsArray)
                {
                    var obj = fieldNode.AsObject();
                    if (obj["alias"]?.GetValue<string>() == "TPP")
                    {
                        var productoField = fieldsArray
                            .FirstOrDefault(node => node!["title"]?.GetValue<string>() == "PRODUCTO")
                            ?.AsObject();

                        if (productoField is not null)
                        {
                            productoField["value"] = product;
                            modified = true;
                        }
                    }
                }

                if (modified)
                {
                    // 5) Serializar el JSON modificado sin saltos de línea
                    var newLvJson = rootNode.ToJsonString(new JsonSerializerOptions
                    {
                        WriteIndented = false
                    });

                    // 6) Reconstruir el body completo y re-encode
                    var rebuilt = prefix + WebUtility.UrlEncode(newLvJson);

                    // 7) Convertir a bytes y reenviar
                    var bytes = Encoding.UTF8.GetBytes(rebuilt);

                    await route.ContinueAsync(new RouteContinueOptions
                    {
                        Headers = route.Request.Headers,
                        PostData = bytes
                    });

                    // Unroute after first modification
                    await page.UnrouteAsync(pattern, handler);
                }
                else
                {
                    // Continue without modification if not matched
                    await route.ContinueAsync();
                }
            };

            await page.RouteAsync(pattern, handler);
        }
        public async Task<string> WaitForFileWithExtensionAsync(
            string extension,
            string folderPath,
            TimeSpan timeout,
            TimeSpan pollInterval,
            CancellationToken cancellationToken = default)
        {
            // Calculate the deadline for the timeout
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                // Throw if cancellation is requested
                cancellationToken.ThrowIfCancellationRequested();

                // Find the first file with the given extension in the folder
                var filePath = Directory.EnumerateFiles(folderPath)
                    .FirstOrDefault(f => string.Equals(Path.GetExtension(f), extension, StringComparison.OrdinalIgnoreCase));

                if (filePath != null)
                {
                    // Return only the file name if found
                    return Path.GetFileName(filePath);
                }

                // Wait for the specified poll interval (cancellable)
                await Task.Delay(pollInterval, cancellationToken);
            }
            // Timeout expired without finding a file
            throw new TimeoutException($"No file with extension '{extension}' was found in '{folderPath}' within {timeout}.");
        }
    }
}
