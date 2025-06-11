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
        public IPlaywright PlaywrightVar { get; private set; }
        //public IBrowser Browser { get; private set; }
        //public IBrowserContext Context { get; private set; }

        public async Task InitializeAsync()
        {
            PlaywrightVar = await Playwright.CreateAsync();
            //Browser = await PlaywrightVar.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            //{
            //    Headless = true,
            //    //DownloadsPath = "C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1", // Ruta para las descargas
            //});
           // Context = await Browser.NewContextAsync();
        }

        public async Task DisposeAsync()
        {
            //await Context.CloseAsync();
            //await Browser.CloseAsync();
            PlaywrightVar?.Dispose();
        }
    }
}
