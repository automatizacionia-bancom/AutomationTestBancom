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

        public ConfigurationTestProvider(IConfiguration config) => _config = config;

        public TransactionType TransactionType => Enum.TryParse<TransactionType>(_config.GetValue<string>("TestData:TransactionType")
            ?? throw new InvalidOperationException("Se requiere TestData:TransactionType"), ignoreCase: true, out var tipo) ? tipo : throw new InvalidOperationException("TestData:TransactionType no es un valor válido");
        public string ExcelPath => _config["TestData:ExcelPath"] ?? throw new InvalidOperationException("Se requiere TestData:ExcelPath");
        public string SheetName => _config["TestData:SheetName"] ?? throw new InvalidOperationException("Se requiere TestData:SheetName");
        public string EvidenceFolderBase => _config["TestData:EvidenceFolderBase"] ?? "C:\\Temp\\Evidencias"; // valor por defecto
        public bool Headless => _config.GetValue("TestData:Headless", false);
        public string IpPort => _config["TestData:IpPort"] ?? throw new InvalidOperationException("Se requiere TestData:IpPort");
        public bool KeepPdf => _config.GetValue("TestData:KeepPdf", false); // valor por defecto es false
        public int MaxApprovalUser => _config.GetValue("TestData:MaxApprovalUser", 10); // valor por defecto es 10
    }
}
