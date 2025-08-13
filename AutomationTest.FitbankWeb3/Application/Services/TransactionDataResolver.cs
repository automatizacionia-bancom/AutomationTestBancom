using System.Reflection;
using AutomationTest.FitbankWeb3.Application.Interfaces;
using AutomationTest.FitbankWeb3.Domain.Attributes;
using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Application.Services
{
    public class TransactionDataResolver : ITransactionDataResolver
    {
        private readonly Dictionary<TransactionType, List<Type>> _typeMap;

        public TransactionDataResolver()
        {
            // Escanea el ensamblado actual (puede ajustarse si es otro)
            var scanningAssembly = Assembly.GetExecutingAssembly();
            _typeMap = scanningAssembly
                .GetTypes()
                .Select(t => new
                {
                    Type = t,
                    Attr = t.GetCustomAttribute<TransactionTypeAttribute>()
                })
                .Where(x => x.Attr is not null)
                .GroupBy(x => x.Attr!.Type)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.Type).ToList()
                );
        }

        public TransactionType GetTransactionType<T>()
        {
            return GetTransactionTypeInternal(typeof(T));
        }

        public TransactionType GetTransactionType(object instance)
        {
            if (instance is null)
                throw new ArgumentNullException(nameof(instance));
            return GetTransactionTypeInternal(instance.GetType());
        }

        public Type GetDataType<TInterface>(TransactionType transactionType)
        {
            if (!_typeMap.TryGetValue(transactionType, out var types))
                throw new KeyNotFoundException(
                    $"No se encontró ningún tipo para TransactionType '{transactionType}'.");
            var interfaceType = typeof(TInterface);
            var match = types.FirstOrDefault(t => interfaceType.IsAssignableFrom(t));
            if (match is null)
                throw new InvalidOperationException(
                    $"No se encontró un tipo para TransactionType '{transactionType}' que implemente {interfaceType.Name}.");
            return match;
        }

        private TransactionType GetTransactionTypeInternal(Type type)
        {
            var attr = type.GetCustomAttribute<TransactionTypeAttribute>();
            if (attr is null)
                throw new InvalidOperationException(
                    $"El tipo {type.Name} no tiene TransactionTypeAttribute.");
            return attr.Type;
        }
    }
}
