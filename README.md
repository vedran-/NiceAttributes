
# NiceAttributes

NiceAttributes evolved from NaughtyAttributes, focusing on a new optimized core with new extended grouping functionality.
The project's name is a nod to the original, acknowledging the groundwork laid by NaughtyAttributes, licensed under the MIT License.

## Features

- **Optimized for speed:** NiceAttributes is designed to be fast and efficient.
- **Nested groups:** You can nest any group inside any other group.
  - **Lazy defining of nested groups:** Define nested groups like `BoxGroup("X/Y")` and specify groups like `HorizontalGroup("X")` later.
- **Ordering as in code:** Paid special attention to ensuring that the ordering of fields/properties/methods (buttons) is as close as possible to their order in the source code file.


## Installation

To use NiceAttributes in your Unity project, add the following line to your project's manifest.json under dependencies:

```json
"com.vedran.niceattributes": "https://github.com/vedran-/NiceAttributes.git"
```

### Group attributes:

- `TabGroup`
- `HorizontalGroup`
- `VerticalGroup`
- `BoxGroup`
- `Foldout`

All flags have a parameter `LineNumber` - it is automatically set by default, but you can override it with any value to manually set the order/position of the property/field/button you're displaying.

### Show

- Shows any field, property, or even non-Serializable class.
- All non-serialized fields shown in the inspector will have a dark red background, to be distinct from normal, serialized properties.

```csharp
[Show] class PrivateNonSerialized { int a; } // Private, non-serialized class - but it has [Show] attribute

public PrivateNonSerialized a; // Shown in inspector
private PrivateNonSerialized b; // Not shown in inspector - field is private, so it is not visible
[Show] private PrivateNonSerialized c; // Shown in inspector - because the field and class have [Show] attribute
```

### Hide

- Hides field, property - or even Serializable class.

```csharp
[Serializable, Hide] public class PublicSerialized { int a; } // Public, serialized class - but it has [Hide] attribute, so it won't show in Inspector
public PublicSerialized a; // Serialized, but field will not show in the inspector - because class has [Hide] attribute

public PrivateNonSerialized a; // Shown in inspector
private PrivateNonSerialized b; // Not shown in inspector - field is private, so it is not visible
[Show] private PrivateNonSerialized c; // Shown in inspector - because the field and class have [Show] attribute
```

## Credits

- dbrizov for his NaughtyAttributes

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Contact

Project Link: [https://github.com/vedran-/NiceAttributes](https://github.com/vedran-/NiceAttributes)
