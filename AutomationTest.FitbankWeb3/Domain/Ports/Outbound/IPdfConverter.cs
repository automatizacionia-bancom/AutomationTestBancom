using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Domain.Ports.Outbound
{
    public interface IPdfConverter
    {
        Task<bool> ConvertAllPagesToPngAsync(string pdfPath, string prefix, string outputFolder, int dpi);
    }
}
