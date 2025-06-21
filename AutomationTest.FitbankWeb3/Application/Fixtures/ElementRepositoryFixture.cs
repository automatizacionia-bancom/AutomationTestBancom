using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.LocatorRepository;

namespace AutomationTest.FitbankWeb3.Application.Fixtures
{
    public class ElementRepositoryFixture
    {
        public LoginRepository Login { get; } = new LoginRepository();
        public LoanApplicationT062900Repository ApplicationPageT062900 { get; } = new LoanApplicationT062900Repository();
        public LoanApplicationT062800Repository ApplicationPageT062800 { get; } = new LoanApplicationT062800Repository();
        public LoanApplicationT062700Repository ApplicationPageT062700 { get; } = new LoanApplicationT062700Repository();
        public LoanApplicationT062500Repository ApplicationPageT062500 { get; } = new LoanApplicationT062500Repository();
        public LoanApplicationT062400Repository ApplicationPageT062400 { get; } = new LoanApplicationT062400Repository();
        public LoanApprovalRepository ApprovalPage { get; } = new LoanApprovalRepository();
        public DashboardRepository DashboardPage { get; } = new DashboardRepository();
    }
}
