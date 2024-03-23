using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NiceAttributes
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple = false, Inherited = true )]
    public class DropdownAttribute : DrawerAttribute
    {
        public string ValuesName { get; private set; }

        public DropdownAttribute( string valuesName, [CallerLineNumber] int lineNumber = 0 )
            : base( lineNumber )
        {
            ValuesName = valuesName;
        }
    }

    public interface IDropdownList : IEnumerable<KeyValuePair<string, object>>
    {
    }

    public class DropdownList<T> : IDropdownList
    {
        private List<KeyValuePair<string, object>> _values;

        public DropdownList()
        {
            _values = new List<KeyValuePair<string, object>>();
        }

        public void Add(string displayName, T value)
        {
            _values.Add(new KeyValuePair<string, object>(displayName, value));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static explicit operator DropdownList<object>(DropdownList<T> target)
        {
            DropdownList<object> result = new DropdownList<object>();
            foreach (var kvp in target)
            {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }
    }
}
