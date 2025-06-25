using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Inbound
{
    /// <summary>
    /// Puerto de entrada: obtiene parámetros de configuración para los tests.
    /// </summary>
    public interface ITestConfigurationProvider
    {
        /// <summary>
        /// Ruta al archivo de Excel que contiene los casos.
        /// </summary>
        string ExcelPath { get; }

        /// <summary>
        /// Nombre de la hoja a leer.
        /// </summary>
        string SheetName { get; }

        /// <summary>
        /// Carpeta base donde guardar evidencias.
        /// </summary>
        string EvidenceFolderBase { get; }

        // … otros valores que necesites …
    }
}
