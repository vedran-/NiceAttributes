using UnityEditor;
using UnityEngine;

namespace NiceAttributes.Editor
{
    [CustomPropertyDrawer( typeof( InfoBoxAttribute ) )]
    public class InfoBoxDecoratorDrawer : DecoratorDrawer
    {
        #region GetHeight()
        public override float GetHeight()
        {
            float minHeight = EditorGUIUtility.singleLineHeight * 2.0f;
            float desiredHeight = GUI.skin.box.CalcHeight( GetTextContent(), EditorGUIUtility.currentViewWidth );
            float height = Mathf.Max( minHeight, desiredHeight ) + 10;
            return height;
        }
        #endregion GetHeight()

        #region OnGUI()
        public override void OnGUI( Rect rect )
        {
            var indentLength = NiceEditorGUI.GetIndentLength(rect);
            var infoBoxRect = new Rect( rect.x + indentLength, rect.y, rect.width - indentLength, GetHeight() );
            //var messageType = ConvertInfoBoxType( (attribute as InfoBoxAttribute).Type );
            var messageType = hasError ? MessageType.Error : ConvertInfoBoxType( (attribute as InfoBoxAttribute).Type );

            NiceEditorGUI.HelpBox( infoBoxRect, GetTextContent().text, messageType );
        }
        #endregion OnGUI()

        #region [Util] ConvertInfoBoxType()
        private MessageType ConvertInfoBoxType( InfoBoxType type )
        {
            switch( type )
            {
                case InfoBoxType.Info: return MessageType.Info;
                case InfoBoxType.Warning: return MessageType.Warning;
                case InfoBoxType.Error: return MessageType.Error;
                default: throw new System.Exception( $"Invalid type {type}" );
            }
        }
        #endregion ConvertInfoBoxType()


        #region [Util] GetTextContent()
        GUIContent textContent = null;
        bool hasError = false;
        GUIContent GetTextContent()
        {
            // TODO: If some variable changed, we should re-evaluate the expression again.

            if( textContent == null )
            {
                var infoBoxAttribute = attribute as InfoBoxAttribute;
                //if( infoBoxAttribute.ClassItem == null ) return GUIContent.none;
                //var classItem = infoBoxAttribute.ClassItem as ClassContext.ClassItem;

                var parseResult = MathematicalParser.Evaluate( infoBoxAttribute.Text, GetVariableValue, GetFunctionValue );
                var text = parseResult.result?.ToString();
                hasError = !parseResult.successful;

                //var text = parseResult.successful ? parseResult.result?.ToString() : infoBoxAttribute.Text;
                //if( !parseResult.successful && classItem != null ) classItem.errorMessage = parseResult.result.ToString();

                textContent = new GUIContent( text );
            }

            return textContent;
        }
        #endregion GetTextContent()


        private object GetVariableValue( string variableName )
        {
            if( variableName == "a" ) return 3;
            if( variableName == "b" ) return 10;

            throw new System.Exception( $"INVALID VAR with name {variableName}!" );
        }

        private object GetFunctionValue( string functionName, object parameters )
        {
            var infoBoxAttribute = (InfoBoxAttribute)attribute;
            //if( infoBoxAttribute.ClassItem == null ) return null;

            if( functionName == "f1" ) return 30;
            if( functionName == "f2" ) return 5;

            throw new System.Exception( $"INVALID FUNC with name {functionName}!" );
        }
    }
}
