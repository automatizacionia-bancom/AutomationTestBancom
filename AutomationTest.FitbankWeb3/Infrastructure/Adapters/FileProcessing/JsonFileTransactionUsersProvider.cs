using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;

namespace AutomationTest.FitbankWeb3.Infrastructure.Adapters.FileProcessing
{
    public class JsonFileTransactionUsersProvider : ITransactionUsersProvider
    {
        private readonly string _jsonPath;

        public JsonFileTransactionUsersProvider(string jsonPath)
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
                throw new ArgumentException("La ruta del fichero JSON no puede ser nula o vacía.", nameof(jsonPath));

            _jsonPath = Path.IsPathRooted(jsonPath)
                ? jsonPath
                : Path.Combine(AppContext.BaseDirectory, jsonPath);

            if (!File.Exists(_jsonPath))
                throw new FileNotFoundException($"No se encontró el fichero JSON en {_jsonPath}");
        }
        public async Task<List<string>> GetUsersForTransactionAsync(TransactionType transactionCode)
        {
            // Deserializa todo el diccionario
            var json = await File.ReadAllTextAsync(_jsonPath);
            var allData = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
                          ?? new Dictionary<string, List<string>>();

            // Devuelve solo la sección solicitada
            return allData.TryGetValue(transactionCode.ToString(), out var users)
                ? users
                : new List<string>();
        }
    }
}
