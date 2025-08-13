using AutomationTest.FitbankWeb3.Application.Interfaces;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Application.Services
{
    public class TestOutputAccessor : ITestOutputAccessor
    {
        // Un AsyncLocal mantiene un valor por flujo asíncrono
        private static readonly AsyncLocal<ITestOutputHelper> _current =
            new AsyncLocal<ITestOutputHelper>();

        public ITestOutputHelper Output
            => _current.Value ?? throw new InvalidOperationException("ITestOutputHelper no ha sido asignado.");

        public void Set(ITestOutputHelper output)
        {
            _current.Value = output;
        }
    }
}
