using UnityEditor;

namespace NiceAttributes.Editor.PropertyValidators
{
    public abstract class PropertyValidatorBase
    {
        public abstract void ValidateProperty(SerializedProperty property);
    }
}
