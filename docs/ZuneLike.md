## Properties

### ZuneLike.BackgroundColor

Set user control backgroud color. **Default** is black.

### ZuneLike.Columns

Define how many columns it contains. **Default** is 18 and it must double *ZuneLike.LargestSize* at least.

### ZuneLike.GridLength

Define each image's size. **Default** is 80.

### ZuneLike.Interval

Define the interval(milliseconds) to filp image. **Default** is 10 seconds.

### ZuneLike.LargestSize

Define largest image's size whose value is **at least 2 and the default is 4.**

### ZuneLike.Rows

Define how many rows it contains. **Default** is 10 and it must double *ZuneLike.LargestSize* at least.

## Methods

### ZuneLike.InitializeGrid(int, int)

The first param represents rows, the second represents columns. They have **default** values 10 and 18.

### ZuneLike.SetUris(System.Collections.Generic.IEnumerable<System.Uri>)

Set images URIs.

### ZuneLike.Render()

Render the whole interface. Remember to call it last.

## Example

In XAML:
```xml
<Grid>
    <ScrollViewer HorizontalScrollBarVisibility="Auto">
        <controls:ZuneLike x:Name="zunelike"></controls:ZuneLike>
    </ScrollViewer>
</Grid>
```

In csharp:
```csharp
private void Generate()
{
    zunelike.InitializeGrid();
    zunelike.Interval = 10_000;
    zunelike.SetUris(GetUris());
    zunelike.Render();
}
```