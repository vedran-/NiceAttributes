# NiceAttributes — Onboarding Guide

> For any agent or developer working on this codebase.

---

## 1. What Is This Project?

NiceAttributes is a **Unity editor extension** that replaces the default Inspector with a custom rendering pipeline driven by C# attributes. It allows developers to decorate fields, properties, and methods with attributes like `[BoxGroup]`, `[ShowIf]`, `[ProgressBar]`, `[Button]`, etc., and have them rendered beautifully in the Unity Inspector.

**Key differentiators from similar libraries (NaughtyAttributes, Odin):**
- Source-code-order rendering (members appear in Inspector in the same order as in the .cs file)
- Nested groups with lazy definition (`[BoxGroup("X/Y")]` before `[HorizontalGroup("X")]`)
- Generic `[Group]` placeholder — change group type by modifying a single attribute

**Version:** 0.10.0

---

## 2. Project Structure

### 2.1 Two Assembly Definitions

```
NiceAttributes.Core.asmdef    →  Runtime attributes (NO UnityEditor dependency)
NiceAttributes.Editor.asmdef  →  Editor processing (references Core + UnityEditor)
NiceAttributes.Tests.asmdef   →  NUnit tests (references Core + Editor + TestAssemblies)
```

**Critical rule:** Core code MUST NOT reference `UnityEditor`. Only Editor code can use `UnityEditor` namespaces. This is enforced by asmdef boundaries.

### 2.2 Directory Map

```
Scripts/
├── Core/                              # ← Runtime assembly (safe for builds)
│   ├── Model/                         # Base attribute classes
│   │   ├── DrawerAttribute.cs         # Base for standard drawer attrs (inherits PropertyAttribute)
│   │   ├── MetaAttribute.cs           # Base for meta attrs (inherits Attribute)
│   │   ├── SpecialCaseDrawerAttribute.cs  # Base for special case drawers
│   │   └── ValidatorAttribute.cs      # Base for validator attrs
│   │
│   ├── DrawerAttributes/              # Standard drawer attributes (~20 files)
│   │   ├── ProgressBarAttribute.cs    # → ProgressBarPropertyDrawer
│   │   ├── DropdownAttribute.cs       # → DropdownPropertyDrawer
│   │   ├── InfoBoxAttribute.cs        # → InfoBoxDecoratorDrawer
│   │   ├── OnGUIAttribute.cs          # → OnGUIPropertyDrawer
│   │   └── ... (16 more)
│   │
│   ├── DrawerAttributes_SpecialCase/  # Special case drawer attributes
│   │   ├── ButtonAttribute.cs         # → handled by ButtonRenderer
│   │   └── ReorderableListAttribute.cs → ReorderableListPropertyDrawer
│   │
│   ├── GroupAttributes/               # Grouping attributes
│   │   ├── BaseGroupAttribute.cs      # Abstract base — has OnGUI_GroupStart/End
│   │   ├── BoxGroupAttribute.cs
│   │   ├── TabGroupAttribute.cs       # Has special TabParent nested class
│   │   ├── HorizontalGroupAttribute.cs
│   │   ├── VerticalGroupAttribute.cs
│   │   ├── FoldoutAttribute.cs
│   │   └── GroupAttribute.cs          # Generic placeholder (overwritten by specific type)
│   │
│   ├── MetaAttributes/                # Visibility / behavior modifiers
│   │   ├── ShowAttribute.cs
│   │   ├── HideAttribute.cs
│   │   ├── ShowIfAttribute.cs / ShowIfAttributeBase.cs
│   │   ├── HideIfAttribute.cs
│   │   ├── EnableIfAttribute.cs / EnableIfAttributeBase.cs
│   │   ├── DisableIfAttribute.cs
│   │   ├── ReadOnlyAttribute.cs
│   │   ├── LabelAttribute.cs
│   │   └── OnValueChangedAttribute.cs
│   │
│   ├── ValidatorAttributes/           # Value validation
│   │   ├── MinValueAttribute.cs       → MinValuePropertyValidator
│   │   ├── MaxValueAttribute.cs       → MaxValuePropertyValidator
│   │   ├── RequiredAttribute.cs       → RequiredPropertyValidator
│   │   └── ValidateInputAttribute.cs  → ValidateInputPropertyValidator
│   │
│   ├── Interfaces/
│   │   ├── INiceAttribute.cs          # Single property: int LineNumber { get; }
│   │   └── IConditionalAttribute.cs   # Shared interface for ShowIf/EnableIf conditions
│   │
│   ├── Enums/
│   │   ├── EConditionOperator.cs      # And / Or
│   │   └── NiceColor.cs               # RGBA as uint enum (0xRRGGBBAA)
│   │
│   ├── Utility/
│   │   ├── GUIUtil.cs                 # Drawing primitives (partial class)
│   │   ├── GUIUtil_Color.cs           # Color push/pop stack (partial class)
│   │   ├── GUIStyles.cs               # Cached GUIStyles (lazy-loaded, FindAssets cached)
│   │   ├── NiceColorExtensions.cs     # NiceColor → Color conversion
│   │   └── Util.cs                    # Color/Rect extension methods
│   │
│   └── GlobalConfig.cs                # Enable/disable toggle (persisted via EditorPrefs)
│
├── Editor/                            # ← Editor assembly (UnityEditor only)
│   ├── NiceInspector.cs               # Entry: CustomEditor for all UnityEngine.Object
│   ├── NiceEditorWindow.cs            # Entry: Base class for custom editor windows
│   ├── ClassContext.cs                # Thin facade (71 lines) — coordinates extracted components
│   │
│   ├── Data/
│   │   ├── ClassItem.cs               # Represents a single member (field/prop/method) — now public
│   │   └── GroupInfo.cs               # Represents a group node in the hierarchy — now public
│   │
│   ├── Discovery/                     # Member discovery (extracted from ClassContext)
│   │   └── MemberDiscoverer.cs        # GetAllMembers + IsVisible
│   │
│   ├── Ordering/                      # Member ordering (extracted from ClassContext)
│   │   └── MemberOrderer.cs           # GetOrderedMembersByLineNumber — pure function
│   │
│   ├── Grouping/                      # Group resolution (extracted from ClassContext)
│   │   ├── GroupResolver.cs           # BuildDisplayTree — pure function
│   │   └── TabGroupInitializer.cs     # InitializeTabGroups
│   │
│   ├── Rendering/                     # Rendering components (extracted from ClassContext)
│   │   ├── ClassRenderer.cs           # Draw + SetActiveGroups + DrawItem
│   │   └── SerializedPropertyConnector.cs  # ConnectWithSerializedProperties
│   │
│   ├── PropertyDrawers/               # Standard drawers (inherit PropertyDrawerBase)
│   │   ├── PropertyDrawerBase.cs      # Base — delegates to PropertyDrawPipeline
│   │   ├── ProgressBarPropertyDrawer.cs
│   │   ├── DropdownPropertyDrawer.cs
│   │   ├── OnGUIPropertyDrawer.cs     # Has static RunGUIMethod()
│   │   └── ... (14 more)
│   │
│   ├── PropertyDrawers_SpecialCase/   # Special case drawers (NOT Unity PropertyDrawers)
│   │   ├── SpecialCasePropertyDrawerBase.cs  # Base — delegates to PropertyDrawPipeline
│   │   ├── ReorderableListPropertyDrawer.cs  # Singleton, uses UnityEditorInternal.ReorderableList
│   │   ├── ListPropertyDrawer.cs      # [CustomPropertyDrawer(typeof(IList), true)]
│   │   └── SpecialCaseDrawerAttributeExtensions.cs  # Maps attr → drawer
│   │
│   ├── DecoratorDrawers/              # Decorator drawers (draw above properties)
│   │   ├── InfoBoxDecoratorDrawer.cs  # ✅ FIXED — dynamic text now works
│   │   └── HorizontalLineDecoratorDrawer.cs
│   │
│   ├── PropertyValidators/            # Validators (called during drawing)
│   │   ├── PropertyValidatorBase.cs   # Abstract: ValidateProperty(SerializedProperty)
│   │   ├── MinValuePropertyValidator.cs
│   │   ├── MaxValuePropertyValidator.cs
│   │   ├── RequiredPropertyValidator.cs
│   │   ├── ValidateInputPropertyValidator.cs
│   │   └── ValidatorAttributeExtensions.cs  # Maps attr → validator
│   │
│   └── Utility/
│       ├── NiceEditorGUI.cs           # Thin facade (48 lines) — delegates to renderers
│       ├── PropertyDrawPipeline.cs    # Shared rendering pipeline (visible → validate → enabled → draw → OnValueChanged)
│       ├── ConditionalEvaluator.cs    # Shared conditional evaluation for ShowIf/EnableIf
│       ├── PropertyUtility.cs         # Property introspection, delegates to ConditionalEvaluator
│       ├── ReflectionUtility.cs       # Reflection helpers, Unity type list
│       ├── ButtonUtility.cs           # Button visibility/enabled checks, delegates to ConditionalEvaluator
│       ├── MathematicalParser.cs      # Full recursive descent expression parser (only parser — FormulaParser deleted)
│       ├── NiceExtensions.cs          # Type extension methods (IsClassOrStruct, etc.)
│       └── Rendering/                 # Focused renderers (extracted from NiceEditorGUI)
│           ├── PropertyFieldRenderer.cs   # PropertyField, PropertyField_Layout, GetIndentLength
│           ├── NonSerializedFieldRenderer.cs  # NativeProperty, NonSerializedField, Field_Layout, HandleCollection
│           ├── HelpBoxRenderer.cs         # HelpBox, HelpBox_Layout, CreateHelpBoxStyles
│           ├── ButtonRenderer.cs            # Button rendering
│           └── DropdownRenderer.cs          # Dropdown rendering
│
├── Tests/                             # ← Test assembly (NEW)
│   ├── NiceAttributes.Tests.asmdef
│   └── Editor/
│       ├── SmokeTest.cs               # Framework verification
│       └── MathematicalParserTests.cs # Comprehensive parser tests (371 lines)
│
└── Examples/
    └── ExampleScriptableObject.cs     # Demo of all attributes in action
```

---

## 3. How Rendering Works (Step by Step)

### 3.1 Initialization (OnEnable)

1. Unity calls `NiceInspector.OnEnable()`
2. `ClassContext.CreateContext()` is called with the target type
3. **Member Discovery** (`MemberDiscoverer.GetAllMembers`):
   - Walks the inheritance chain (skipping `UnityEngine.Object`, `ScriptableObject`, `MonoBehaviour`)
   - For each type, gets all fields/properties/methods via reflection
   - Filters by `IsVisible()` — checks `[Hide]`, `[Show]`, serialization rules
   - For class/struct fields with `[Serializable]` or `[Show]`, recursively creates child `ClassContext`
   - Sets `HasNiceAttributes = true` if any `INiceAttribute` is found
4. **Ordering** (`MemberOrderer.Order`):
   - Members with `[CallerLineNumber]` have exact source line numbers
   - Members without line numbers get interpolated values between neighbors of the same type
   - Final sort: `OrderBy(m => m.lineNumber)`
5. **Group Tree Building** (`GroupResolver.BuildDisplayTree`):
   - Creates `GroupInfo` nodes for each group path (e.g., `"root/A/B/C"`)
   - Handles lazy group definition — creates parent groups on demand
   - Generic `[Group]` is overwritten by specific group types
   - Detects group type conflicts (e.g., `[HorizontalGroup("A")]` and `[BoxGroup("A")]` on same group)
   - Inserts members into the list at positions that preserve group hierarchy and source order
6. **Tab Group Initialization** (`TabGroupInitializer.Initialize`):
   - Collects all `TabGroupAttribute` instances under the same parent
   - Creates `TabParent` objects to manage tab state

### 3.2 Serialized Property Connection

After context creation, `SerializedPropertyConnector.Connect()` bridges reflection-based `ClassItem` objects to Unity's `SerializedProperty` system:
- Iterates `SerializedProperty` tree
- Matches each property to a `ClassItem` by name (or backing field name for auto-properties)
- Sets `ClassItem.serializedProperty` for rendering

### 3.3 Rendering (OnInspectorGUI)

1. `ClassContext.Draw()` delegates to `ClassRenderer.Render()`
2. **Group Management** (`SetActiveGroups`):
   - Compares current item's group path with previous item's
   - Opens new groups (calls `BaseGroupAttribute.StartDrawingGroup()`)
   - Closes old groups (calls `BaseGroupAttribute.FinishDrawingGroup()`)
   - Handles collapsed foldouts — skips rendering contents
3. **Per-Item Rendering** (`DrawItem`):
   - **Expanded class/struct:** Draw foldout, recursively call `childContext.Draw()`
   - **Serialized property:** `PropertyFieldRenderer.PropertyField_Layout()`
   - **Non-serialized field:** `NonSerializedFieldRenderer.NonSerializedField_Layout()`
   - **Non-serialized property:** `NonSerializedFieldRenderer.NativeProperty_Layout()`
   - **Method (button):** `ButtonRenderer.Button()`
   - Non-serialized items get a reddish background tint

### 3.4 Property Field Rendering Pipeline

When a property is drawn, it goes through `PropertyDrawPipeline.Execute()`:

```
PropertyDrawPipeline.Execute(rect, property, drawFunction)
    │
    ├── PropertyUtility.IsVisible(property) — early return if hidden
    ├── ValidateProperty() for each ValidatorAttribute
    ├── PropertyUtility.IsEnabled(property) — DisabledScope if false
    ├── drawFunction(rect, property, label)
    └── PropertyUtility.CallOnValueChangedCallbacks(property) — if value changed
```

This pipeline is shared by:
- `PropertyDrawerBase.OnGUI()` (standard Unity PropertyDrawers)
- `SpecialCasePropertyDrawerBase.OnGUI()` (special case drawers)
- `PropertyFieldRenderer.PropertyField_Implementation()` (direct property field rendering)

### 3.5 Unity's CustomPropertyDrawer Integration

Standard drawer attributes use Unity's `[CustomPropertyDrawer]` mechanism:
- `PropertyDrawerBase` inherits from `PropertyDrawer`
- Each concrete drawer has `[CustomPropertyDrawer(typeof(SomeAttribute))]`
- Unity calls `PropertyDrawer.OnGUI()` when the property is drawn via `EditorGUI.PropertyField()`
- `PropertyDrawerBase.OnGUI()` wraps the drawing with the pipeline (visibility, validation, etc.)

**Important:** When `ClassRenderer.DrawItem()` calls `PropertyFieldRenderer.PropertyField_Layout()`, which calls `EditorGUILayout.PropertyField()`, Unity's PropertyDrawer system kicks in and calls the appropriate `[CustomPropertyDrawer]`.

---

## 4. Key Data Structures

### 4.1 ClassItem (nested in ClassContext)

```csharp
public class ClassItem {
    MemberInfo           memberInfo;           // FieldInfo, PropertyInfo, or MethodInfo
    INiceAttribute[]     niceAttributes;       // All NiceAttributes on this member
    float                lineNumber;           // Source line number (interpolated if unknown)
    GroupInfo            group;                // Which group this member belongs to
    SerializedProperty   serializedProperty;   // Unity's serialized property (if applicable)
    string               errorMessage;         // Error/warning text to display
    bool                 foldedOut;            // Foldout state for non-serialized members
    ClassContext         classContext;         // Child context for expanded classes/structs
}
```

### 4.2 GroupInfo (nested in ClassContext)

```csharp
public class GroupInfo {
    readonly string      groupName;            // e.g., "root/A/B"
    BaseGroupAttribute   groupAttribute;       // The attribute that defines this group's type
    GroupInfo[]          groups;               // All ancestor groups (including self)
    TabGroupAttribute.TabParent tabParent;     // If this group is inside a TabGroup
}
```

### 4.3 TabGroupAttribute.TabParent (nested in TabGroupAttribute)

```csharp
public class TabParent {
    public List<TabGroupAttribute> tabGroups;  // All tabs in this tab group
    // Rendering state (selected tab, etc.) managed during OnGUI
}
```

---

## 5. Important Patterns & Conventions

### 5.1 CallerLineNumber for Ordering

All attribute constructors accept `[CallerLineNumber] int lineNumber = 0`. The compiler automatically fills this with the source line where the attribute is used:

```csharp
// In attribute class:
public VerticalGroupAttribute(string groupName, [CallerLineNumber] int lineNumber = 0)
    : base(lineNumber) { }

// User code:
[VerticalGroup("MyGroup")]  // ← lineNumber is auto-set to this line number
public int myField;
```

### 5.2 Group Resolution Algorithm

1. Split group name by `/` → `["A", "B", "C"]`
2. Build path incrementally: `"root/A"` → `"root/A/B"` → `"root/A/B/C"`
3. Create `GroupInfo` for each path segment if it doesn't exist
4. Set `groupAttribute` on the deepest group (overwrite if it's a generic `GroupAttribute`)
5. Detect conflicts: if two different group types claim the same group name, show error

### 5.3 Conditional Evaluation (ShowIf/EnableIf)

All conditional attributes (`ShowIf`, `HideIf`, `EnableIf`, `DisableIf`) implement `IConditionalAttribute` and use `ConditionalEvaluator.Evaluate()`:
1. If `EnumValue` is set, compare against target's enum value (with Flags support)
2. Otherwise, get boolean condition values via `PropertyUtility.GetConditionValues()`
3. Combine multiple conditions with `EConditionOperator.And` or `.Or`
4. Invert if needed (HideIf/DisableIf = inverted=true)

### 5.4 Value Change Callbacks

`[OnValueChanged("MethodName")]` calls the named method after a property value changes:
- Method must have `void` return type and 0 parameters
- `PropertyUtility.CallOnValueChangedCallbacks()` is called after `EditorGUI.EndChangeCheck()`

### 5.5 PropertyDrawPipeline (Shared Rendering Pipeline)

The rendering pipeline (visible → validate → enabled → draw → OnValueChanged) is extracted into `PropertyDrawPipeline.Execute()` and used by:
- `PropertyDrawerBase.OnGUI()` — standard Unity PropertyDrawers
- `SpecialCasePropertyDrawerBase.OnGUI()` — special case drawers
- `PropertyFieldRenderer.PropertyField_Implementation()` — direct property field rendering

This ensures consistent behavior across all property drawing paths and eliminates code duplication.

### 5.6 ConditionalEvaluator (Shared Conditional Evaluation)

The conditional evaluation logic (enum comparison, boolean condition resolution, And/Or combination, inversion) is extracted into `ConditionalEvaluator.Evaluate()` and used by:
- `PropertyUtility.IsVisible()` — for ShowIf/HideIf
- `PropertyUtility.IsEnabled()` — for EnableIf/DisableIf
- `ButtonUtility.IsVisible()` — for button visibility conditions
- `ButtonUtility.IsEnabled()` — for button enabled conditions

---

## 6. Known Issues & Limitations

### 6.1 Remaining Known Issues
- **InfoBox/Header inside groups:** Doesn't work properly with TabGroup, BoxGroup, etc. (acknowledged in README)

### 6.2 Incomplete Features
- **GenericGroup:** Listed as TODO in README
- **Cross-branch group item display:** TODO in original ClassContext.cs
- **Struct boxing in Dropdown:** TODO in original NiceEditorGUI.cs

### 6.3 Visual Polish
- TabGroup / BoxGroup / FoldoutGroup appearance and colors need improvement (acknowledged in README)

### 6.4 Resolved Issues (No Longer Applicable)
- ~~Dynamic InfoBox text~~ — **FIXED** — now resolves actual field/property/method names via reflection
- ~~Collection foldout state~~ — **FIXED** — each collection now has independent foldout state
- ~~GlobalConfig not persisted~~ — **FIXED** — enable/disable state persists via EditorPrefs
- ~~Duplicated rendering pipeline~~ — **FIXED** — extracted to PropertyDrawPipeline
- ~~Duplicated conditional evaluation~~ — **FIXED** — extracted to ConditionalEvaluator
- ~~God classes (ClassContext 689 lines, NiceEditorGUI 510 lines)~~ — **FIXED** — decomposed to focused components
- ~~Dead code (#if false blocks, commented-out code)~~ — **FIXED** — all removed
- ~~Debug artifacts (HELLOOO label, hardcoded stubs)~~ — **FIXED** — all removed
- ~~Redundant math parser (FormulaParser)~~ — **FIXED** — consolidated to MathematicalParser only
- ~~No automated tests~~ — **FIXED** — NUnit test suite with MathematicalParserTests

---

## 7. Build & Test

### 7.1 How to Test
1. Open Unity project with NiceAttributes installed
2. Select `ExampleScriptableObject` asset in the project
3. Verify Inspector renders all attributes correctly
4. Create a new ScriptableObject and test attributes manually

### 7.2 Automated Tests
- **Test assembly:** `NiceAttributes.Tests.asmdef` (references Core + Editor + TestAssemblies)
- **SmokeTest.cs:** Verifies NUnit framework is working
- **MathematicalParserTests.cs:** Comprehensive tests for the expression parser (371 lines)
  - Basic arithmetic, parentheses, variables, functions
  - String concatenation, number formatting, multiple expressions
  - Error handling: invalid tokens, mismatched parentheses, division by zero
  - Negative numbers
- **Run tests:** Window > General > Test Runner > Run All

### 7.3 Compilation
- Unity compiles based on asmdef files
- Core compiles first (no UnityEditor dependency)
- Editor compiles second (depends on Core)
- Tests compile last (depend on Core + Editor)
- No external build system (no MSBuild, no dotnet CLI)

---

## 8. Glossary

| Term | Meaning |
|------|---------|
| **Drawer** | A class that renders a property in the Inspector |
| **Decorator** | A drawer that renders above a property (e.g., InfoBox, HorizontalLine) |
| **Validator** | A class that checks property values and shows warnings/errors |
| **Group** | A container that visually groups multiple properties together |
| **ClassContext** | Thin facade (71 lines) coordinating extracted components |
| **ClassItem** | A single member (field/property/method) with its attributes and rendering state |
| **GroupInfo** | A node in the group hierarchy tree |
| **NiceInspector** | Custom Editor that replaces Unity's default Inspector |
| **NiceEditorWindow** | Base class for custom editor windows with NiceAttributes support |
| **SpecialCase** | Drawers that don't use Unity's PropertyDrawer system (e.g., ReorderableList) |
| **MetaAttribute** | Attributes that modify behavior without directly drawing (Show, Hide, EnableIf) |
| **LineNumber** | Source code line where attribute is used — enables source-order rendering |
| **PropertyDrawPipeline** | Shared rendering pipeline (visible → validate → enabled → draw → OnValueChanged) |
| **ConditionalEvaluator** | Shared conditional evaluation for ShowIf/EnableIf/HideIf/DisableIf |
| **MemberDiscoverer** | Extracted component for reflection-based member enumeration and visibility filtering |
| **MemberOrderer** | Extracted component for line-number-based member ordering (pure function) |
| **GroupResolver** | Extracted component for group tree building and member-to-group assignment (pure function) |
| **ClassRenderer** | Extracted component for the rendering loop and group state management |

---

## 9. Related Files

- `README.md` — Project overview and feature list
- `REVIEW_RULES.md` — Code review guidelines (general purpose)
- `RESEARCH_FINDINGS.md` — Detailed list of all issues found (archived, pre-refactoring)
- `REFACTORING_PLAN.md` — Prioritized implementation plan (archived, pre-refactoring)
