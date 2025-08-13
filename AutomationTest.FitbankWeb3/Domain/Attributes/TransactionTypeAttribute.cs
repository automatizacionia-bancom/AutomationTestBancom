using AutomationTest.FitbankWeb3.Domain.Enums;

namespace AutomationTest.FitbankWeb3.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TransactionTypeAttribute : Attribute
    {
        public TransactionType Type { get; }
        public TransactionTypeAttribute(TransactionType type) => Type = type;
    }
}
