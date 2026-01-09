namespace Blazor.VirtualisedGrid.Interfaces;

/// <summary>
/// Interface for providing data to the virtualised grid.
/// </summary>
/// <typeparam name="T">The row type that implements IVirtualisedRow</typeparam>
public interface IVirtualisedGridDataSource<T> where T : IVirtualisedRow
{
    /// <summary>
    /// Gets all rows in the data source.
    /// </summary>
    IEnumerable<T> GetAllRows();

    /// <summary>
    /// Gets the total number of rows in the data source.
    /// </summary>
    int TotalRowCount { get; }

    /// <summary>
    /// Gets a range of rows from the data source.
    /// </summary>
    /// <param name="startIndex">The starting index (0-based).</param>
    /// <param name="count">The number of rows to retrieve.</param>
    /// <returns>A collection of rows.</returns>
    IEnumerable<T> GetRows(int startIndex, int count);
}

