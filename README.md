# Blazor.VirtualisedGrid

A high-performance Blazor grid component with virtualization support for dynamic row heights. This package provides efficient rendering of large datasets by only rendering visible rows and using IntersectionObserver API for scroll detection.

## Features

- **LoadOnDemand Mode**: Incremental loading of rows as user scrolls
- **Custom Virtualization Mode**: Renders skeleton rows for non-visible items, fully renders visible range
- **Dynamic Row Heights**: Supports rows with varying heights (no fixed height assumptions)
- **Dual Grid Support**: Works with both HTML tables and Telerik Grid (via extension package)
- **Multi-Targeting**: Supports .NET 7.0 and .NET 8.0

## Installation (coming soon)

```bash
dotnet add package Blazor.VirtualisedGrid
```

## Quick Start

### 1. Implement IVirtualisedRow

Your row model must implement `IVirtualisedRow`:

```csharp
public class MyRow : IVirtualisedRow
{
    public int RowIndex { get; set; }
    public Guid RowIdent { get; } = Guid.NewGuid();
    public string RowTypeName => "MyRow";
    public bool IsHidden { get; set; }
    
    // Your custom properties
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
}
```

Your row can define option IVirtualisedColumn objects. These allow you to define column widths to give a more accurate skeleton row for unrendered elements.

### 2. Implement IRowRenderer

Create a renderer for your rows:

```csharp
public class MyRowRenderer : IRowRenderer<MyRow>
{
    public RenderFragment BuildRowFragment(MyRow row)
    {
        return builder =>
        {
            builder.OpenElement(0, "td");
            builder.AddContent(1, row.Name);
            builder.CloseElement();
            builder.OpenElement(2, "td");
            builder.AddContent(3, row.Value);
            builder.CloseElement();
        };
    }

    public RenderFragment BuildSkeletonFragment(MyRow row)
    {
        return builder =>
        {
            builder.OpenElement(0, "td");
            builder.AddAttribute(1, "colspan", "2");
            builder.AddContent(2, "Loading...");
            builder.CloseElement();
        };
    }
}
```

### 3. Use the Component

```razor
@using Blazor.VirtualisedGrid.Components
@using Blazor.VirtualisedGrid.Models

<VirtualisedGrid T="MyRow"
                 Data="@myRows"
                 RowRenderer="@new MyRowRenderer()"
                 Options="@new VirtualisationOptions { IsLoadOnDemand = true }"
                 ShowHeaders="true"
                 Headers="@new[] { "Name", "Value" }" />
```

## Virtualization Modes

### LoadOnDemand Mode

Incremental loading - rows are loaded as the user scrolls near the bottom:

```csharp
var options = new VirtualisationOptions
{
    IsLoadOnDemand = true,
    PageSize = 10,      // Rows to load per batch
    Overflow = 20       // Buffer size
};
```

### Custom Virtualization Mode

All rows are in memory, but only visible rows are fully rendered. Non-visible rows show skeleton placeholders:

```csharp
var options = new VirtualisationOptions
{
    IsLoadOnDemand = false,
    PageSize = 10,
    Overflow = 20
};
```

## Configuration Options

```csharp
public class VirtualisationOptions
{
    public int PageSize { get; set; } = 10;              // Rows per load
    public int Overflow { get; set; } = 20;              // Buffer size
    public bool IsLoadOnDemand { get; set; } = false;    // Mode selection
    public string? ScrollObserverClass { get; set; }     // Custom observer class
    public string? AboveObserverClass { get; set; }       // Custom observer class
    public string? BelowObserverClass { get; set; }       // Custom observer class
    public string? GridId { get; set; }                   // Custom grid ID
}
```

## JavaScript Setup

The package includes the required JavaScript file (`virtualised-grid-observer.js`). Ensure it's loaded in your `_Host.cshtml` or `index.html`:

```html
<script src="_content/Blazor.VirtualisedGrid/js/virtualised-grid-observer.js"></script>
```

## Telerik Grid Support

For Telerik Grid integration, use the `Blazor.VirtualisedGrid.Telerik` extension package (coming soon).

## How It Works

1. **IntersectionObserver API**: Uses browser IntersectionObserver to detect when scroll targets enter/exit viewport
2. **Range Calculation**: Maintains a visible range of rows to fully render
3. **Skeleton Rows**: Non-visible rows render as lightweight skeletons to maintain scroll height
4. **Dynamic Loading**: LoadOnDemand mode fetches additional rows as needed

## Performance Benefits

- **Reduced DOM Elements**: Only renders visible rows + buffer
- **Smooth Scrolling**: Skeleton rows maintain scroll position
- **Memory Efficient**: LoadOnDemand mode keeps memory footprint low
- **Dynamic Heights**: No fixed height assumptions, works with any row content

## License

MIT License

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

