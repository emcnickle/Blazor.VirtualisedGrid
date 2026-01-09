using Blazor.VirtualisedGrid.Interfaces;
using Microsoft.AspNetCore.Components;

namespace Blazor.VirtualisedGrid.Components;

/// <summary>
/// Default row renderer implementation that throws exceptions to force users to provide custom renderers.
/// </summary>
/// <typeparam name="T">The row type that implements IVirtualisedRow</typeparam>
public abstract class DefaultRowRenderer<T> : IRowRenderer<T> where T : IVirtualisedRow
{
    /// <inheritdoc/>
    public abstract RenderFragment BuildRowFragment(T row);

    /// <inheritdoc/>
    public virtual RenderFragment BuildSkeletonFragment(T row)
    {
        return builder =>
        {
            int seq = 0;

            if (row.Columns.Any())
            {
                foreach (IVirtualisedColumn column in row.Columns)
                {
                    builder.OpenElement(seq++, "td");
                    builder.AddAttribute(seq++, "style", $"width: {column.Width}; height: 28px; background-color: #e5e7eb; border-radius: 4px;");
                    builder.CloseElement();
                }
            }
            else
            {
                builder.OpenElement(seq++, "td");
                builder.AddAttribute(seq++, "style", "width: 100%; height: 28px; background-color: #e5e7eb; border-radius: 4px;");
                builder.CloseElement();
            }
        };
    }


}

