using System;

namespace NiceAttributes
{
    public interface IConditionalAttribute
    {
        string[] Conditions { get; }
        EConditionOperator ConditionOperator { get; }
        bool Inverted { get; }
        Enum EnumValue { get; }
    }
}
