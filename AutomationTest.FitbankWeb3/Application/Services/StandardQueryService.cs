using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Application.Models.QueryModels;
using AutomationTest.FitbankWeb3.Application.Transactions.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Models.AutomationTest.FitbankWeb3.Domain.Models;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;
using Microsoft.Extensions.DependencyInjection;

namespace AutomationTest.FitbankWeb3.Application.Services
{
    public class StandardQueryService : IStandardQueryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IGenericQueryExecutor _genericQueryExecutor;
        public StandardQueryService(IServiceProvider serviceProvider, IGenericQueryExecutor genericQueryExecutor)
        {
            _serviceProvider = serviceProvider;
            _genericQueryExecutor = genericQueryExecutor ?? throw new ArgumentNullException(nameof(genericQueryExecutor));
        }
        public async Task<DataTable> ExecuteStandardQueryAsync<TStandardQuery>(StandardQueryModel standardQuery) where TStandardQuery : IStandardQuery
        {
            // Obtener la instancia TStandardQuery desde DI
            var instance = _serviceProvider.GetRequiredService<TStandardQuery>();
            var query = instance.CreateQuery(standardQuery);
            return await _genericQueryExecutor.ExecuteAsync(query);
        }
    }
}
