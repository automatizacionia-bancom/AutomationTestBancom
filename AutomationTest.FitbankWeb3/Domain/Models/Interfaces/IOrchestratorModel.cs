using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomationTest.FitbankWeb3.Domain.Models.Interfaces
{
    public interface IOrchestratorModel<TClientData> where TClientData : IClientData
    {
        public string EvidenceFoler { get; set; }
        public string IpPort { get; set; }
        public bool Headless { get; set; }
    }
}
