using UnityEditor;

namespace NiceAttributes.Editor
{
    public abstract class PropertyValidatorBase
    {
        public abstract void ValidateProperty(SerializedProperty property);
    }
}
