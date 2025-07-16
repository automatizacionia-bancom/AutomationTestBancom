using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.LocatorRepository;
using PdfSharpCore.Pdf.Content.Objects;

namespace AutomationTest.FitbankWeb3.Application.Fixtures
{
    public class LocatorRepositoryFixture
    {
        public LocatorRepositoryLogin LocatorsLogin { get; } = new LocatorRepositoryLogin();
        public LocatorRepositoryT062900 LocatorsT062900 { get; } = new LocatorRepositoryT062900();
        public LocatorRepositoryT062800 LocatorsT062800 { get; } = new LocatorRepositoryT062800();
        public LocatorRepositoryT062700 LocatorsT062700 { get; } = new LocatorRepositoryT062700();
        public LocatorRepositoryT062600 LocatorsT062600 { get; } = new LocatorRepositoryT062600();
        public LocatorRepositoryT062500 LocatorsT062500 { get; } = new LocatorRepositoryT062500();
        public LocatorRepositoryT062400 LocatorsT062400 { get; } = new LocatorRepositoryT062400();
        public LocatorRepositoryT072100Pe LocatorsT072100Pe { get; } = new LocatorRepositoryT072100Pe();
        public LocatorRepositoryPersonalBankingDashboard LocatorsPersonalBankingDashboard { get; } = new LocatorRepositoryPersonalBankingDashboard();
        public LocatorRepositoryBusinessBankingDashboard LocatorsBusinessBankingDashboard { get; } = new LocatorRepositoryBusinessBankingDashboard();
        public LocatorRepositoryGeneralDashboard LocatorsGeneralDashboard { get; } = new LocatorRepositoryGeneralDashboard();
    }
}
