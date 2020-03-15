# PA0002 - Piranha does not support single field complex regions

According to documentation, Piranha is not able to handle complex regions with only a single field.
The analyzer will check types of properties marked with the `RegionAttribute` and mark the region properties as violating if they have a single field property marked with the `FieldAttribute`.

| Severity | Category |
|----------|----------|
| Error    | Usage    |

Animation that shows when and how the error is shown:

![][animation]

## Available code fixes
None currently.


[animation]: ./PA0002.gif