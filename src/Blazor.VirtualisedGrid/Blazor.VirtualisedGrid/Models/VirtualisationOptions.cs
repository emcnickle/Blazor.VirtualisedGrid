namespace Blazor.VirtualisedGrid.Models;

/// <summary>
/// Configuration options for grid virtualization.
/// </summary>
public class VirtualisationOptions
{
    /// <summary>
    /// Gets or sets the number of rows to load on each ReadRows trigger.
    /// Default: 10
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the overflow buffer size for virtualization beyond the default page size.
    /// Default: 20
    /// </summary>
    public int Overflow { get; set; } = 20;

    /// <summary>
    /// Gets or sets a flag indicating if the grid will load on demand (incremental loading).
    /// If false, all rows will load using custom virtualization with skeleton rows.
    /// Default: false
    /// </summary>
    public bool IsLoadOnDemand { get; set; } = false;

    /// <summary>
    /// Gets or sets the CSS class name for the scroll observer element (LoadOnDemand mode).
    /// If not set, will be auto-generated.
    /// </summary>
    public string? ScrollObserverClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class name for the above observer element (Custom virtualization mode).
    /// If not set, will be auto-generated.
    /// </summary>
    public string? AboveObserverClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class name for the below observer element (Custom virtualization mode).
    /// If not set, will be auto-generated.
    /// </summary>
    public string? BelowObserverClass { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for this grid instance.
    /// Used to manage multiple grids on the same page.
    /// If not set, will be auto-generated.
    /// </summary>
    public string? GridId { get; set; }
}

