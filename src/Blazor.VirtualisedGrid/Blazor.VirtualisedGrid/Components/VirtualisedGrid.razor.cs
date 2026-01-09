using Blazor.VirtualisedGrid.Interfaces;
using Blazor.VirtualisedGrid.Models;
using Blazor.VirtualisedGrid.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.ObjectModel;

namespace Blazor.VirtualisedGrid.Components;

/// <summary>
/// A high-performance virtualised grid component that supports dynamic row heights.
/// Supports both LoadOnDemand (incremental loading) and Custom Virtualization (skeleton rows) modes.
/// </summary>
/// <typeparam name="T">The row type that implements IVirtualisedRow</typeparam>
public partial class VirtualisedGrid<T> : ComponentBase, IDisposable where T : IVirtualisedRow
{
    /// <summary>
    /// Gets or sets the data source for the grid.
    /// </summary>
    [Parameter]
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    /// Gets or sets the row renderer for building row fragments.
    /// </summary>
    [Parameter, EditorRequired]
    public IRowRenderer<T> RowRenderer { get; set; } = null!;

    /// <summary>
    /// Gets or sets the virtualization options.
    /// </summary>
    [Parameter]
    public VirtualisationOptions Options { get; set; } = new VirtualisationOptions();

    /// <summary>
    /// Gets or sets whether to show column headers.
    /// </summary>
    [Parameter]
    public bool ShowHeaders { get; set; } = false;

    /// <summary>
    /// Gets or sets the column header text.
    /// </summary>
    [Parameter]
    public IEnumerable<string>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the table element.
    /// </summary>
    [Parameter]
    public string? TableClass { get; set; }

    /// <summary>
    /// Gets or sets the container style.
    /// </summary>
    [Parameter]
    public string? ContainerStyle { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private VirtualisationState _state = new();
    private VirtualisationOptions _options = new();
    private ObservableCollection<T> _rowItems = new();
    private DotNetObjectReference<VirtualisedGrid<T>>? _objectRef;
    private bool _isScrollDetected;
    private CancellationTokenSource _cancellationSource = new();
    private Task? _gridRefreshTask;
    private string _gridId = string.Empty;
    private string _scrollObserver = string.Empty;
    private string _aboveObserver = string.Empty;
    private string _belowObserver = string.Empty;
    private int _totalRowCount = 0;

    protected override void OnInitialized()
    {
        _state.IsLoadOnDemand = _options.IsLoadOnDemand;
        _state.PageSize = _options.PageSize;
        _state.Overflow = _options.Overflow;
        _state.InitializeRangeEnd();

        _gridId = _options.GridId ?? $"VirtualisedGrid-{Guid.NewGuid():N}";
        _scrollObserver = _options.ScrollObserverClass ?? $"ScrollObserver-{_gridId}";
        _aboveObserver = _options.AboveObserverClass ?? $"observerTargetAbove-{_gridId}";
        _belowObserver = _options.BelowObserverClass ?? $"observerTargetBelow-{_gridId}";

        _objectRef = DotNetObjectReference.Create(this);

        if (!_options.IsLoadOnDemand && Data != null)
        {
            _rowItems = new ObservableCollection<T>(Data);
            _state.CurrentCount = _rowItems.Count;
        }

        _totalRowCount = Data?.Count() ?? 0;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (!_options.IsLoadOnDemand && Data != null)
            {
                await ReadAllRows();
            }
        }

        // Initialize observers after render to allow observer elements to render
        await JSRuntime.InvokeVoidAsync("VirtualisedGridObserver.Initialize", 
            _gridId, 
            _objectRef, 
            _scrollObserver, 
            _belowObserver, 
            _aboveObserver, 
            _options.IsLoadOnDemand);
    }

    /// <summary>
    /// JS Invoked method to trigger the ReadRows function.
    /// Triggered when the IntersectionObserver detects the scroll observer target.
    /// </summary>
    [JSInvokable]
    public async Task OnReadRows()
    {
        if (_isScrollDetected) return;
        if (_rowItems.Count >= _totalRowCount) return;

        await ReadRows();
    }

    /// <summary>
    /// JS Invoked method to update the visible rows on the grid.
    /// Used for row virtualization.
    /// Triggered when the scroll intersects with an end target observer.
    /// </summary>
    [JSInvokable]
    public async Task UpdateVisibleRangeEnd(object index)
    {
        if (_isScrollDetected) return;

        _isScrollDetected = true;
        try
        {
            if (int.TryParse(index.ToString(), out int indexVal) && indexVal <= _totalRowCount && indexVal > _state.VisibleRangeEnd - (_options.Overflow / 2))
            {
                _state.VisibleRangeEnd = indexVal + (_options.Overflow / 2);
                if (_state.VisibleRangeEnd > _totalRowCount) _state.VisibleRangeEnd = _totalRowCount + 1;
                _state.VisibleRangeStart = indexVal - _options.PageSize - (_options.Overflow / 2);
                if (_state.VisibleRangeStart < 0) _state.VisibleRangeStart = 0;

                await TriggerScrollRefresh(indexVal);
            }
        }
        finally
        {
            _isScrollDetected = false;
        }
    }

    /// <summary>
    /// JS Invoked method to update the visible rows on the grid.
    /// Used for row virtualization.
    /// Triggered when the scroll intersects with a start target observer.
    /// </summary>
    [JSInvokable]
    public async Task UpdateVisibleRangeStart(object index)
    {
        if (_isScrollDetected) return;

        _isScrollDetected = true;
        try
        {
            if (int.TryParse(index.ToString(), out int indexVal) && indexVal >= 0 && indexVal < _state.VisibleRangeStart + (_options.Overflow / 2))
            {
                _state.VisibleRangeStart = indexVal - (_options.Overflow / 2);
                if (_state.VisibleRangeStart < 0) _state.VisibleRangeStart = 0;
                _state.VisibleRangeEnd = indexVal + _options.PageSize + (_options.Overflow / 2);
                if (_state.VisibleRangeEnd < _options.Overflow + _options.PageSize) _state.VisibleRangeEnd = _options.Overflow + _options.PageSize;

                await TriggerScrollRefresh(indexVal);
            }
        }
        finally
        {
            _isScrollDetected = false;
        }
    }

    private IEnumerable<T> GetOrderedRows()
    {
        var rows = _options.IsLoadOnDemand ? _rowItems.ToList() : (Data?.ToList() ?? new List<T>());
        return rows.OrderBy(x => x.RowIndex);
    }

    private bool IsRowWithinRange(T row)
    {
        return row.RowIndex >= _state.VisibleRangeStart && row.RowIndex <= _state.VisibleRangeEnd + 1;
    }

    private bool IsEvenIndex(T row)
    {
        return row.RowIndex % 2 == 0;
    }

    private bool IsWithinVisibleStartRange(T row)
    {
        return _state.VisibleRangeStart > 0 && row.RowIndex <= _state.VisibleRangeStart + (_options.Overflow / 2);
    }

    private bool IsWithinVisibleEndRange(T row)
    {
        return row.RowIndex <= _totalRowCount - (_options.Overflow / 2) && row.RowIndex >= _state.VisibleRangeEnd - (_options.Overflow / 2);
    }

    private async Task ReadRows(int? targetRow = null)
    {
        if (_isScrollDetected) return;

        _isScrollDetected = true;
        try
        {
            if (targetRow == null)
            {
                int limitCount = _state.CurrentCount + _options.PageSize;
                if (_state.CurrentCount == 0) limitCount += _options.Overflow;
                limitCount = (limitCount + _state.CurrentCount) > _totalRowCount ? _totalRowCount - _state.CurrentCount : limitCount;

                if (Data != null && _state.CurrentCount < _totalRowCount)
                {
                    var newRows = Data.Skip(_state.CurrentCount).Take(limitCount).ToList();
                    foreach (var row in newRows)
                    {
                        _rowItems.Add(row);
                    }
                }
                _state.CurrentCount = _rowItems.Count;
            }
            else
            {
                if (Data != null && targetRow.Value < Data.Count())
                {
                    var newRow = Data.ElementAt(targetRow.Value);
                    _rowItems.Add(newRow);
                    _state.CurrentCount = _rowItems.Count;
                }
            }

            StateHasChanged();
            await Task.Delay(1);
        }
        finally
        {
            _isScrollDetected = false;
        }
    }

    private Task ReadAllRows()
    {
        if (_totalRowCount == _state.CurrentCount) return Task.CompletedTask;

        if (Data != null)
        {
            _rowItems = new ObservableCollection<T>(Data);
            _state.CurrentCount = _rowItems.Count;
        }

        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task TriggerScrollRefresh(int indexVal)
    {
        if (_gridRefreshTask != null)
        {
            _cancellationSource.Cancel();
            _cancellationSource.Dispose();
            _cancellationSource = new CancellationTokenSource();
        }

        _gridRefreshTask = GridRefresh(_cancellationSource.Token, indexVal);
        return Task.CompletedTask;
    }

    private async Task GridRefresh(CancellationToken cancellationToken, int rowIndex)
    {
        await Task.Delay(250, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        StateHasChanged();
    }

    private string GetContainerStyle()
    {
        return ContainerStyle ?? string.Empty;
    }

    private string GetHeaderStyle(string header)
    {
        return string.Empty;
    }

    private int TotalRowCount => _totalRowCount;

    public void Dispose()
    {
        _objectRef?.Dispose();
        _cancellationSource?.Dispose();
        _ = JSRuntime.InvokeVoidAsync("DisposeVirtualisedGridObserver", _gridId);
    }

    public virtual RenderFragment BuildAboveObserver(T row)
    {
        return BuildObserver(row, _aboveObserver);
    }

    public virtual RenderFragment BuildBelowObserver(T row)
    {
        return BuildObserver(row, _belowObserver);
    }

    public virtual RenderFragment BuildScrollObserver(T row)
    {
        return BuildObserver(row, _scrollObserver);
    }

    private RenderFragment BuildObserver(T row, string observer)
    {
        return builder =>
        {
            int seq = 0;

            builder.OpenElement(seq++, "td");
            builder.AddAttribute(seq++, "style", "width: 1px; height: 1px");
            builder.OpenElement(seq++, "span");
            builder.AddAttribute(seq++ , "class", observer);
            builder.AddAttribute(seq++, "id", row.RowIndex);
            builder.CloseElement();
            builder.CloseElement();

        };
    }   
}

