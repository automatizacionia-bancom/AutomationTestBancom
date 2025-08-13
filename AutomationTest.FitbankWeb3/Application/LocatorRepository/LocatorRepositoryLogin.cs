namespace AutomationTest.FitbankWeb3.Application.LocatorRepository
{
    public class LocatorRepositoryLogin
    {
        public string UsernameInput { get; } = "input[type='text']";
        public string PasswordInput { get; } = "input[type='password']";
        public string SubmitButton { get; } = "input[type='submit']";
        public string ForceLogin { get; } = "#forzar";
    }
}
