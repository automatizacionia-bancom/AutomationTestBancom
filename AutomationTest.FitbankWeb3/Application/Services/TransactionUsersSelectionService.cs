using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Enums;
using AutomationTest.FitbankWeb3.Domain.Ports.Outbound;

namespace AutomationTest.FitbankWeb3.Application.Services
{
    public class TransactionUsersSelectionService : ITransactionUsersSelectionService
    {
        private readonly ITransactionUsersProvider _transactionUsersProvider;

        private static readonly Random _rng = Random.Shared;
        public TransactionUsersSelectionService(ITransactionUsersProvider transactionUsersProvider)
        {
            _transactionUsersProvider = transactionUsersProvider;
        }
        public async Task<string> SelectOptimalUserAsync(TransactionType transactionCode, List<string> recognizedUsers)
        {
            if (recognizedUsers == null || recognizedUsers.Count == 0)
                throw new ArgumentException("La lista de usuarios reconocidos no puede estar vacía.", nameof(recognizedUsers));

            var recommendedUsers = await _transactionUsersProvider.GetUsersForTransactionAsync(transactionCode);

            // 1. Intenta encontrar el primer recomendado que esté en los reconocidos
            string? selected = recommendedUsers?.FirstOrDefault(user => recognizedUsers.Contains(user));
            if (!string.IsNullOrEmpty(selected))
                return selected;

            // 2. Si no hay coincidencias, selecciona uno aleatorio de los reconocidos
            return recognizedUsers[_rng.Next(recognizedUsers.Count)];
        }
    }
}
