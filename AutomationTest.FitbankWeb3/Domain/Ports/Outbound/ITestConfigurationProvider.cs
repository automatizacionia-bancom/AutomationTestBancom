using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Outbound
{
    /// <summary>
    /// Puerto de entrada: obtiene parámetros de configuración para los tests.
    /// </summary>
    public interface ITestConfigurationProvider
    {
        /// <summary>
        /// Tipo de transaccion
        /// </summary>
        TransactionType TransactionType { get; }
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

        /// <summary>
        /// Indica si se debe ejecutar en modo headless (sin interfaz gráfica).
        /// </summary>
        bool Headless { get; }

        /// <summary>
        /// Dirección IP y puerto del servidor de pruebas.
        /// </summary>
        string IpPort { get; }

        /// <summary>
        /// Indica si se debe mantener el PDF generado después de la prueba. (Solo se aplica a headless)
        /// </summary>
        bool KeepPdf { get; }

        /// <summary>
        /// Limite de seguridad maximo de aprobaciones se puede realizar a una misma solicitud de crédito.
        /// </summary>
        int MaxApprovalUser { get; }
        string ApprovalCases { get; }
    }
}
