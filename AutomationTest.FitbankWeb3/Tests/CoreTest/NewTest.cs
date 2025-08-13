using Microsoft.Playwright;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    public class NewTest
    {
        private readonly ITestOutputHelper _output;
        public NewTest(ITestOutputHelper output)
        {
            _output = output;
        }
        [Fact]
        public async Task Prueba()
        {
            IPlaywright playwright = await Playwright.CreateAsync();
            IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false,
            });

            IBrowserContext context = await browser.NewContextAsync();

            var pageLogin = await context.NewPageAsync();

            await pageLogin.GotoAsync("http://10.0.2.50:8180/WEB");

            await pageLogin.Locator("#ingreso > table > tbody > tr:nth-child(2) > td > div > input").FillAsync("JTELLO");
            await pageLogin.Locator("#password").FillAsync("fitbank123");

            await pageLogin.EvaluateAsync(@"() => {
                const realOpen = window.open;
                window.open = (url, name, features) => {
                    // Llama a window.open con **solo** url y target
                    // omitiendo por completo el tercer argumento
                    return realOpen.call(window, url, name);
                };
            }");

            var popupTask = context.WaitForPageAsync();

            await pageLogin.Locator("#ingreso > table > tbody > tr:nth-child(5) > td > div > input").ClickAsync();

            IPage pageFitbank = await popupTask;

            await pageFitbank.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var frameLocator = pageFitbank.FrameLocator("frameset[name='fp'] > frame");


            await frameLocator.Locator("#ps").FillAsync("02");
            await frameLocator.Locator("#pt").FillAsync("3200");
            await frameLocator.Locator("#pt").PressAsync("Enter");

            await pageFitbank.PauseAsync();
            await pageFitbank.WaitForLoadStateAsync(LoadState.NetworkIdle);


        }
    }
}
