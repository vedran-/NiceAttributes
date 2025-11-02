# NiceAttributes

NiceAttributes evolved from [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes), focusing on a new optimized core with new extended grouping functionality.
The project's name is a nod to the original, acknowledging the groundwork laid by NaughtyAttributes, licensed under the MIT License.

## Features

- **Optimized for speed:** NiceAttributes is designed to be fast. It should not slow down your project in any way. 
- **Nested groups:** You can nest any group inside any other group.
    - **Lazy defining of nested groups:** Define nested groups like `[BoxGroup("X/Y")]` and specify parent group type later, with e.g. `[HorizontalGroup("X")]`.
- **Ordering as in code:** Paid special attention to ensuring that the ordering of fields, properties, and methods (buttons) is as close as possible to their order in the source code file.
  That means that buttons defined next to a field will be displayed next to that field in the inspector, instead on bottom of the inspector, as it is with other similar libraries.


- **[Show]** and **[Hide]** attributes: Show or hide any field, property, or even non-Serializable class in the Inspector.
- **[TabGroup]**, **[HorizontalGroup]**, **[VerticalGroup]**, **[BoxGroup]**, **[Foldout]** grouping attributes.
- **Dynamic Text** - You can use formula inside text. That formula can use local **fields** and **properties**, which are treated as variables, or even call local **methods**, which are treated as functions.

- **Custom GUI** - You can display custom GUI in the Inspector using the `[OnGUI]` attribute.
- **InfoBox** - You can display an InfoBox in the Inspector above some visible Method, Field, or Property.
  - InfoBox types: Info, Warning, Error
  - Text in the InfoBox can be dynamic.

- **NiceEditorWindow** - You can create custom editor windows by extending `NiceEditorWindow` class.
That way you don't even have to create `OnGUI()` method - just create fields and properties like you would for any other class, and they will be displayed in the editor window.

- **Generic [Group]** - if you have multiple variables in the same group, then all of them
except one can use [Group] as a generic placeholder for a group, and just one of them needs
to use [HorizontalGroup], [VerticalGroup], [TabGroup], or any other grouping type, to define the actual group type.

Example:

[Group("A")] int a, b;
[Group("A")] string c;
[HorizontalGroup("A")] float d;
In this example, all the variables a, b, c and d will be grouped together. The group type is defined by the HorizontalGroup attribute on the variable d.
And if we want to switch the whole group to another type, we just need to change the HorizontalGroup attribute to VerticalGroup, TabGroup, or any other group type.

TODO:
- GenericGroup


### OnGUI

- `[OnGUI]` - Displays a custom GUI in the Inspector.

### InfoBox

Displays InfoBox in the Inspector above some visible Method, Field or Property.
InfoBox types: Info, Warning, Error
Text in the InfoBox can be dynamic.



## Installation

Two ways to include NiceAttributes in your project:

### 1. Unity Package Manager
To use NiceAttributes in your Unity project, add the following line to your project's `Packages/manifest.json` under dependencies:

```json
    "com.nightrider.niceattributes": "git+https://github.com/vedran-/NiceAttributes.git",
```

### 2. Manual

Download the latest release from the [Releases](https://github.com/vedran-/NiceAttributes/releases) page and import the package into your Unity project.

### Group attributes:

- `TabGroup`
- `HorizontalGroup`
- `VerticalGroup`
- `BoxGroup`
- `Foldout`

All flags have a parameter `LineNumber` - it is automatically set by default, but you can override it with any value to manually set the order/position of the property/field/button you're displaying.

### `Show`

- Shows any field, property, or even non-Serializable class.
- All non-serialized fields shown in the inspector will have a dark red background, to be distinct from serialized properties.

```csharp
[Show] class PrivateNonSerialized { int a; } // Private, non-serialized class - but it has [Show] attribute

public PrivateNonSerialized a; // Shown in inspector
private PrivateNonSerialized b; // Not shown in inspector - field is private, so it is not visible
[Show] private PrivateNonSerialized c; // Shown in inspector - because the field and class have [Show] attribute
```

### `Hide`

- Hides field, property - or even Serializable class.

```csharp
[Serializable, Hide] public class PublicSerialized { int a; } // Public, serialized class - but it has [Hide] attribute, so it won't show in Inspector
public PublicSerialized a; // Serialized, but field will not show in the inspector - because class has [Hide] attribute

public PrivateNonSerialized a; // Shown in inspector
private PrivateNonSerialized b; // Not shown in inspector - field is private, so it is not visible
[Show] private PrivateNonSerialized c; // Shown in inspector - because the field and class have [Show] attribute
```

## Credits

- Denis Brizov for his [NaughtyAttributes](https://github.com/dbrizov/NaughtyAttributes)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

Project Link: [https://github.com/vedran-/NiceAttributes](https://github.com/vedran-/NiceAttributes)

