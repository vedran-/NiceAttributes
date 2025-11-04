using System.Collections.Generic;
using UnityEngine;

namespace NiceAttributes.Examples
{
    public class ExampleScriptableObject : ScriptableObject
    {
        [BoxGroup("BoxGroup: Serialized properties")]
        [field: SerializeField] public uint u01 { get; private set; } = 7;
        [BoxGroup("BoxGroup: Serialized properties")]
        [field: SerializeField] private uint u02 { get; set; } = 1;

        [TabGroup("Animals")] public int animalsCount; 
        [TabGroup("Animals")] public List<string> animalNames = new List<string>() {"Lassie", "Laika", "Max"};
        [TabGroup("Animals/Dogs", Title = "Dogs")] public int dogsCount;
        [TabGroup("Animals/Dogs")] public bool isDogHappy;
        [TabGroup("Animals/Cats", Title = "Cats")] public int catsCount;
        [TabGroup("Animals/Cats", Title = "Cats")]
        [InfoBox("This is private non-serialized field shown with [Show] attribute")]   // TODO: Make InfoBox work with Show/TabGroup attribute
        [Show] private bool isCatHappy;

        [TabGroup("Objects")]
        public int objectsCount;
        [TabGroup("Objects/Chairs", Title = "Chairs")]
        public int chairsCount;
        [TabGroup("Objects/Tables", Title = "Tables")]
        public int tablesCount;
        [TabGroup("Objects/Electronics", Title = "Electronics")]
        public int electronicsCount;
        
        
        [OnGUI(PreDrawMethodName = "OnPreGUI", PostDrawMethodName = "OnPostGUI")]
        public string CustomGUIFunctionExample = "Look at OnPreGUI() and OnPostGUI() methods in the script to see how to use custom GUI functions.";
        
        
        [Space(50)]
        public int a01;

        [BoxGroup("A", TitleColor = NiceColor.Green)] public int a02;
        [TabGroup("A/B/H", GroupBackColor = (NiceColor)0x003f00ff)] public int a03;
        [BoxGroup("A")] public int a04;
        [TabGroup("A/B/H")] public int a05;
        [BoxGroup("A")] public int a06;
        public int a07;
        [BoxGroup("A")] public int a08;
        [TabGroup("A/B/C", TitleColor = NiceColor.Black, TitleShadowColor = 0, TitleBackColor = (NiceColor)0x3f7f2fff, GroupBackColor = (NiceColor)0x7f7f00ff)] public int a09;
        [BoxGroup("A/B", TitleBackColor = (NiceColor)0xFF00007f)] public int a10;
        [TabGroup("A/B/C")] public int a11;
        public int a12;
        [BoxGroup("A")] public int a13;
        [BoxGroup("A/D")] public int a14;
        [BoxGroup("A")] public int a15;
        public int a16;
        [BoxGroup("X/Y")] public int a17;
        [BoxGroup("X")] public int a18;
        [BoxGroup("X")] public int a19;
        public int a20;


        [HorizontalGroup("A/B/C/Buttons")]
        [Button] void B() {}
        [HorizontalGroup( "A/B/C/Buttons" )]
        [Button] void A() {}


        [TabGroup("A/B/H/Sizes", GroupBackColor = (NiceColor)0xffff00FF, TitleColor = NiceColor.Black)]
#pragma warning disable CS0414 // Field is assigned but its value is never used
        [Show] private int a = 3;
#pragma warning restore CS0414 // Field is assigned but its value is never used
        [TabGroup("A/B/H/Mines")]
        [Show] private string b => "Uga!";
        [TabGroup("A/B/H/Buttons", GroupBackColor = NiceColor.Indigo)]
        [Button] private int c() => 4;
        [Show] private int d => 5;
        [Show] private int e => 6;


#if UNITY_EDITOR
        private void OnPreGUI()
        {
            var rect = GUILayoutUtility.GetRect(0, 22);
            GUIUtil.FillRoundedRect(rect, Color.black, new GUIContent("This red square is drawn from OnPreGUI() method"));
            //GUIUtil.DrawLabel(rect, "This red square is drawn from OnPreGUI() method", new GUIStyle("box"), Color.white, Color.black);
            GUIUtil.DrawRect(rect, Color.cyan, 1);
        }
        private void OnPostGUI()
        {
            var rect = GUILayoutUtility.GetRect(0, 60);
            GUIUtil.DrawCheckeredRect(rect, color: Color.blueViolet);
            GUIUtil.DrawLabel(rect, "This checkered area is drawn from OnPostGUI() method", 
                new GUIStyle("box"), Color.white, Color.black);
            //GUIUtil.FillRect(rect, Color.blueViolet, new GUIContent("This is drawn from OnPostGUI() method"));
            GUIUtil.DrawRect(rect, Color.greenYellow, 1);
        }
#endif
    }
}