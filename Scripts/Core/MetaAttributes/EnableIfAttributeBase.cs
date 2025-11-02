using System;
using NiceAttributes.Model;

namespace NiceAttributes
{
    public abstract class EnableIfAttributeBase : MetaAttribute
    {
        public string[] Conditions { get; private set; }
        public EConditionOperator ConditionOperator { get; private set; }
        public bool Inverted { get; protected set; }

        /// <summary>
        ///		If this not null, <see cref="Conditions"/>[0] is name of an enum variable.
        /// </summary>
        public Enum EnumValue { get; private set; }

        public EnableIfAttributeBase( string condition, int lineNumber )
            : base( lineNumber )
        {
            ConditionOperator = EConditionOperator.And;
            Conditions = new string[1] { condition };
        }

#if false
        public EnableIfAttributeBase( EConditionOperator conditionOperator, params string[] conditions = null, [CallerLineNumber] int lineNumber = 0 )
        {
            ConditionOperator = conditionOperator;
            Conditions = conditions;
        }
#endif

        public EnableIfAttributeBase( string enumName, Enum enumValue, int lineNumber )
            : this( enumName, lineNumber )
        {
            if( enumValue == null )
            {
                throw new ArgumentNullException( nameof( enumValue ), "This parameter must be an enum value." );
            }

            EnumValue = enumValue;
        }
    }
}
