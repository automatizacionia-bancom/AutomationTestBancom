using System.Data;
using AutomationTest.FitbankWeb3.Application.Fixtures;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace AutomationTest.FitbankWeb3.Tests.CoreTest
{
    [Trait("Grupo", "CoreTest")]
    public class DatabaseTest : IClassFixture<TestFixture>
    {
        private readonly IGenericQueryExecutor _executor;
        private readonly IStandardQueryService _standardQueryService;
        private readonly ITestOutputHelper _output;

        public DatabaseTest(TestFixture fixture, ITestOutputHelper output)
        {
            _executor = fixture.ServiceProvider.GetRequiredService<IGenericQueryExecutor>();
            _standardQueryService = fixture.ServiceProvider.GetRequiredService<IStandardQueryService>();
            _output = output;
        }
        [Fact]
        public async Task DataBaseGeneralTest()
        {
            //"SELECT * FROM FITBANK.TUSUARIOSESIONES WHERE CTERMINAL = 'GJIMENO' AND FHASTA='2999-12-31 00:00:00'");

            DataTable table = await _executor.ExecuteAsync(new Domain.Models.AutomationTest.FitbankWeb3.Domain.Models.GenericQueryModel
            {
                Query = "SELECT * FROM FITBANK.TUSUARIOSESIONES WHERE CTERMINAL = 'HASANCHEZ' AND FHASTA='2999-12-31 00:00:00'",
                Timeout = 10000, // 10 segundos
                ThrowOnError = true, // Lanzar excepción si hay error
            });

            // Print DataTable for debugging purposes
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    _output.WriteLine($"{column.ColumnName}: {row[column]} ");
                }
                _output.WriteLine("");
            }


            //var servicio = new PdfConverter();
            //bool salida = await servicio.ConvertAllPagesToPngAsync("C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1\\PRT.pdf",
            //    "4. PRT","C:\\Users\\HASANCHEZ\\Desktop\\Fitbank RPA\\Evidencias\\Prueba\\Caso1", 300);

            //_output.WriteLine($"Se generaron archivos PNG: {salida}");
            //Assert.True(salida, "La consulta no se ejecutó correctamente o el resultado no coincide con lo esperado.");
        }
    }
}
