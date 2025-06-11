using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    public interface ITestOutputAccessor
    {
        ITestOutputHelper Output { get; }
        void Set(ITestOutputHelper output);
    }
}
