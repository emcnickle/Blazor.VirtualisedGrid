using System.ComponentModel;

namespace Blazor.VirtualisedGrid.Interfaces;

/// <summary>
/// Interface that row items must implement to be used with the virtualised grid.
/// </summary>
public interface IVirtualisedRow
{
    /// <summary>
    /// Gets or sets the index of the row (determines ordering within grid).
    /// </summary>
    int RowIndex { get; set; }

    /// <summary>
    /// Gets the unique identifier for the row.
    /// Used to track pre-built row fragments.
    /// </summary>
    Guid RowIdent { get; }

    /// <summary>
    /// Gets the row type name to uniquely identify the row type.
    /// </summary>
    string RowTypeName { get; }

    /// <summary>
    /// Gets or sets a flag to determine if the row is visible or not.
    /// </summary>
    bool IsHidden { get; set; }

    IEnumerable<IVirtualisedColumn> Columns { get; set; }
}

public interface IVirtualisedColumn
{
    string Width { get; }
    int ColumnIndex { get; }
}

