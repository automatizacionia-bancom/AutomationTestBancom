namespace AutomationTest.FitbankWeb3.Domain.Ports.Outbound
{
    public interface IPdfConverter
    {
        Task<bool> ConvertAllPagesToImgAsync(string pdfPath, string prefix, string outputFolder, int dpi);
    }
}
