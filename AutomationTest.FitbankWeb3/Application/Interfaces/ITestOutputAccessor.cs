using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Interfaces
{
    public interface ITestOutputAccessor
    {
        ITestOutputHelper Output { get; }
        void Set(ITestOutputHelper output);
    }
}
