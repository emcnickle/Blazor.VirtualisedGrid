using Microsoft.AspNetCore.Components;

namespace Blazor.VirtualisedGrid.Interfaces;

/// <summary>
/// Interface for rendering rows in the virtualised grid.
/// </summary>
/// <typeparam name="T">The row type that implements IVirtualisedRow</typeparam>
public interface IRowRenderer<T> where T : IVirtualisedRow
{
    /// <summary>
    /// Builds a render fragment for a fully rendered row.
    /// </summary>
    /// <param name="row">The row to render.</param>
    /// <returns>A render fragment for the row.</returns>
    RenderFragment BuildRowFragment(T row);

    /// <summary>
    /// Builds a render fragment for a skeleton/placeholder row.
    /// Used for rows outside the visible range during custom virtualization.
    /// </summary>
    /// <param name="row">The row to create a skeleton for.</param>
    /// <returns>A render fragment for the skeleton row.</returns>
    RenderFragment BuildSkeletonFragment(T row);
}

