# PA0001 - `FieldType` should not be used as a single field region

According to official documentation, the editors for the following field types are best suited for complex regions.

| Severity | Category |
|----------|----------|
| Info     | Usage    |

| Applicable built-in field types |
|---------------------------------|
| `AudioField`                    |
| `CheckBoxField`                 |
| `DateField`                     |
| `DocumentField`                 |
| `ImageField`                    |
| `MediaField`                    |
| `NumberField`                   |
| `PageField`                     |
| `PostField`                     |
| `StringField`                   |
| `VideoField`                    |

## Available code fixes
### Move `FieldType` to `ExistingRegion`
Moves the single field region into `ExistingRegion` as a field.

Note that this action only appears if the content class has an existing complex region.

Code with suggestion:

![""][before]

Choose code fix:

![""][action]

After code fix:

![""][after]

### ~~Replace with complex region~~
~~Introduces a new complex region and updates the content class to use it.~~

Note: With the introduction of [PA0002] this code fix has been disabled as it generates the exact pattern that [PA0002] marks as an error.

[before]: ./PA0001-before-move.png "Before fix"
[action]: ./PA0001-move-to-existing-region.png "Fix"
[after]: ./PA0001-after-move.png "After fix"

[PA0002]: ../PA0002/README.md