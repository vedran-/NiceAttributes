using UnityEngine;

namespace NiceAttributes.Examples
{
    public class ExampleScriptableObject : ScriptableObject
    {
        [BoxGroup("BoxGroup: Serialized properties")]
        [field: SerializeField] public uint u01 { get; private set; } = 7;
        [BoxGroup("BoxGroup: Serialized properties")]
        [field: SerializeField] private uint u02 { get; set; } = 1;

        [TabGroup("Animals")]
        public int animalsCount;
        [TabGroup("Animals/Dogs", Title = "Dogs")]
        public int dogsCount;
        [TabGroup("Animals/Dogs", Title = "Dogs")]
        public bool isDogHappy;
        [TabGroup("Animals/Cats", Title = "Cats")]
        public int catsCount;
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


        [TabGroup("A/B/H/Sizes", GroupBackColor = (NiceColor)0xffff000)]
#pragma warning disable CS0414 // Field is assigned but its value is never used
        [Show] private int a = 3;
#pragma warning restore CS0414 // Field is assigned but its value is never used
        [TabGroup("A/B/H/Mines")]
        [Show] private string b => "Uga!";
        [TabGroup("A/B/H/Buttons")]
        [Button] private int c() => 4;
        [Show] private int d => 5;
        [Show] private int e => 6;
    }
}