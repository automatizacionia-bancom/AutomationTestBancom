namespace AutomationTest.FitbankWeb3.Domain.Models
{
    namespace AutomationTest.FitbankWeb3.Domain.Models
    {
        public class GenericQueryModel
        {
            public string Query { get; set; } = string.Empty;
            public int Timeout { get; set; } = 30000;
            public bool ThrowOnError { get; set; } = true;
        }
    }
}
