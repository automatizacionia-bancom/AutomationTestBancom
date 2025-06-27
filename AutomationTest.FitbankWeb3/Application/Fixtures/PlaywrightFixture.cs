using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace AutomationTest.FitbankWeb3.Application.Fixtures
{
    public class PlaywrightFixture : IAsyncLifetime
    {
        public IPlaywright PlaywrightVar { get; private set; } = null!;
        public async Task InitializeAsync()
        {
            PlaywrightVar = await Playwright.CreateAsync();
        }
        public async Task DisposeAsync()
        {
            PlaywrightVar?.Dispose();
            await Task.CompletedTask;
        }
    }
}
