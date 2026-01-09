namespace Blazor.VirtualisedGrid.Services;

/// <summary>
/// Represents the current state of grid virtualization.
/// </summary>
public class VirtualisationState
{
    /// <summary>
    /// Gets or sets the starting index of the first row to be fully rendered during virtualization.
    /// </summary>
    public int VisibleRangeStart { get; set; } = 0;

    /// <summary>
    /// Gets or sets the end index of the last row to be fully rendered during virtualization.
    /// </summary>
    public int VisibleRangeEnd { get; set; }

    /// <summary>
    /// Gets or sets the current total count of visible rows (for LoadOnDemand mode).
    /// </summary>
    public int CurrentCount { get; set; } = 0;

    /// <summary>
    /// Gets or sets a flag indicating if the grid is using LoadOnDemand mode.
    /// </summary>
    public bool IsLoadOnDemand { get; set; } = false;

    /// <summary>
    /// Gets or sets the page size for incremental loading.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Gets or sets the overflow buffer size.
    /// </summary>
    public int Overflow { get; set; } = 20;

    /// <summary>
    /// Initializes the visible range end based on overflow and page size.
    /// </summary>
    public void InitializeRangeEnd()
    {
        VisibleRangeEnd = Overflow + PageSize;
    }
}

