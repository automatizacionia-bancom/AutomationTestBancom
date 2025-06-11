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
        public LoanApprovalRepository ApprovalPage { get; } = new LoanApprovalRepository();
        public DashboardRepository DashboardPage { get; } = new DashboardRepository();
    }
}
