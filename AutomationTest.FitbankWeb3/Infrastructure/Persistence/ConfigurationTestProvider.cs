using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.Configuration;

namespace AutomationTest.FitbankWeb3.Infrastructure.Persistence
{
    public class ConfigurationTestProvider : ITestConfigurationProvider
    {
        private readonly IConfiguration _config;
        private readonly IConfigurationSection _currentTestData;

        public ConfigurationTestProvider(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // 1) Leemos el nombre (string) del TransactionType activo
            var txTypeStr = _config.GetValue<string>("TestData:TransactionType")
                ?? throw new InvalidOperationException("Se requiere TestData:TransactionType");

            // 2) Parseamos al enum
            if (!Enum.TryParse<TransactionType>(txTypeStr, ignoreCase: true, out var txType))
                throw new InvalidOperationException(
                    $"TestData:TransactionType “{txTypeStr}” no es un valor válido de TransactionType.");

            TransactionType = txType;

            // 3) Apuntamos al sub‑nodo TestData:{TransactionType}
            _currentTestData = _config.GetSection($"TestData:{txTypeStr}");
            if (!_currentTestData.Exists())
                throw new InvalidOperationException(
                    $"No existe la sección TestData:{txTypeStr} en la configuración.");
        }

        /// <summary>
        /// El TransactionType seleccionado (por env‑var o appsettings.json).
        /// </summary>
        public TransactionType TransactionType { get; }

        /// <summary>
        /// Ruta al Excel, sacada de TestData:{TransactionType}:ExcelPath
        /// </summary>
        public string ExcelPath =>
            _currentTestData["ExcelPath"]
            ?? throw new InvalidOperationException(
                $"Se requiere TestData:{TransactionType}:ExcelPath");

        /// <summary>
        /// Nombre de la hoja, sacado de TestData:{TransactionType}:SheetName
        /// </summary>
        public string SheetName =>
            _currentTestData["SheetName"]
            ?? throw new InvalidOperationException(
                $"Se requiere TestData:{TransactionType}:SheetName");

        /// <summary>
        /// Carpeta base de evidencias, sacada de TestData:{TransactionType}:EvidenceFolderBase
        /// </summary>
        public string EvidenceFolderBase =>
            _currentTestData["EvidenceFolderBase"]
            ?? throw new InvalidOperationException(
                $"Se requiere TestData:{TransactionType}:EvidenceFolderBase");

        // —————————————————————————————————————————————————
        // El resto de valores siguen en TestData (raíz)
        // —————————————————————————————————————————————————

        public bool Headless =>
            _config.GetValue("TestData:Headless", defaultValue: false);

        public string IpPort =>
            _config["TestData:IpPort"]
            ?? throw new InvalidOperationException("Se requiere TestData:IpPort");

        public bool KeepPdf =>
            _config.GetValue("TestData:KeepPdf", defaultValue: false);

        public int MaxApprovalUser =>
            _config.GetValue("TestData:MaxApprovalUser", defaultValue: 10);
    }
}
