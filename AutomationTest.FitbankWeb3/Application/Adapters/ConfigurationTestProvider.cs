using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Ports.Inbound;
using Microsoft.Extensions.Configuration;

namespace AutomationTest.FitbankWeb3.Application.Adapters
{
    public class ConfigurationTestProvider : ITestConfigurationProvider
    {
        private readonly IConfiguration _config;

        public ConfigurationTestProvider(IConfiguration config)
        {
            _config = config;
        }

        public string ExcelPath
            => _config["TestData:ExcelPath"]
               ?? throw new InvalidOperationException("Se requiere TestData:ExcelPath");

        public string SheetName
            => _config["TestData:SheetName"]
               ?? throw new InvalidOperationException("Se requiere TestData:SheetName");

        public string EvidenceFolderBase
            => _config["TestData:EvidenceFolderBase"]
               ?? "C:\\Temp\\Evidencias"; // valor por defecto
    }
}
