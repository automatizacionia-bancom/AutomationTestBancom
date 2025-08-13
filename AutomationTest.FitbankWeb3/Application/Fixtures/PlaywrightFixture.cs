using Microsoft.Playwright;

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
