using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.JSInterop;
using PresalesApp.CustomTypes;
using PresalesApp.Shared;
using PresalesApp.Web.Client.Extensions;
using Radzen;
using System.Globalization;
using System.Reflection;
using static PresalesApp.Web.Client.Extensions.Comparsion;

namespace PresalesApp.Web.Client.Views.QuickGridExtension;

public partial class GridComponent<TGridItem>
{
    #region Injections

    [Inject]
    private IJSRuntime _JSRuntime { get; set; }

    [Inject]
    private DialogService _DialogService { get; set; }

    #endregion

    #region Parameters

    [Parameter, EditorRequired]
    public IEnumerable<TGridItem>? Items { get; set; }

    [Parameter]
    public string[] Columns { get; set; } = [];

    [Parameter]
    public bool ShowSettings { get; set; }

    [Parameter]
    public bool ShowFilters { get; set; }

    [Parameter]
    public RenderFragment? Settings { get; set; }

    #endregion

    #region Private

    #region Members

    private RenderFragment _ColumnsFragment;

    private RenderFragment _SettingsFragment;

    private Dictionary<string, _Filter> _Filters;

    private EventCallback _OnColumnsChanged
        => new(this, (Action<string[]>)((columns) => Columns = columns));

    private IQueryable<TGridItem>? _FilteredItems
    {
        get
        {
            if (Items is null) return null;

            var result = Items.AsQueryable();


            foreach (var property in Helpers.GetProperties<TGridItem>())
            {
                if (!string.IsNullOrEmpty(_Filters[property.Name].Value))
                    result = result?.Where(item => _GetFilter(item, property));
            }

            var summary = (TGridItem)Activator.CreateInstance(typeof(TGridItem), new object[] {});

            foreach (var property in Helpers.GetProperties<TGridItem>())
            {
                if (property.PropertyType.Name == nameof(DecimalValue))
                    summary?.GetType()?.GetField(property.Name)?.SetValue(summary, result?.Sum(x => (decimal)x.GetType().GetField(property.Name).GetValue(x)));
            }

            TGridItem[] summaryArray = [summary];

            return result?.Concat(summaryArray);
        }
    }

    #endregion

    #region Methods

    private static IEnumerable<ComparsionType> _GetHumanComparsionTypes(int skip = 1, int skipLast = 1)
        => System.Enum.GetValues<ComparsionType>().Cast<ComparsionType>().Skip(skip).SkipLast(skipLast);

    private static string _GetSortIcon(ColumnBase<TGridItem>? column)
        => (column as CustomColumnBase<TGridItem>)?.Grid.SortByColumn == column
                            ? (column as CustomColumnBase<TGridItem>)?.Grid.SortByAscending ?? false
                                ? "keyboard_double_arrow_up"
                                : "keyboard_double_arrow_down"
                            : "";

    private bool _GetFilter(TGridItem item, PropertyInfo property)
        => property.PropertyType.Name switch
        {
            nameof(DecimalValue) => Comparator((decimal)(property.GetValue(item) as DecimalValue),
                _Filters[property.Name].Value.ParseOrDefault(), _Filters[property.Name].Comparsion switch
                {
                    ComparsionType.GreaterThan => Comparsion.GreaterThan,
                    ComparsionType.GreaterThanOrEqual => Comparsion.GreaterThanOrEqual,
                    ComparsionType.LessThan => Comparsion.LessThan,
                    ComparsionType.LessThanOrEqual => Comparsion.LessThanOrEqual,
                    ComparsionType.Equal => Comparsion.Equal,
                    ComparsionType.NotEqual => Comparsion.NotEqual,
                    _ => throw new NotImplementedException()
                }),
            nameof(Timestamp) => Comparator((property.GetValue(item) as Timestamp ?? new Timestamp()).ToDateTime().ToLocalTime(),
                DateTime.ParseExact(_Filters[property.Name].Value, Helpers.DateTimeFormat, CultureInfo.CurrentCulture),
                _Filters[property.Name].Comparsion switch
                {
                    ComparsionType.GreaterThan => Comparsion.GreaterThan,
                    ComparsionType.GreaterThanOrEqual => Comparsion.GreaterThanOrEqual,
                    ComparsionType.LessThan => Comparsion.LessThan,
                    ComparsionType.LessThanOrEqual => Comparsion.LessThanOrEqual,
                    ComparsionType.Equal => Comparsion.Equal,
                    ComparsionType.NotEqual => Comparsion.NotEqual,
                    _ => throw new NotImplementedException()
                }),
            _ => (property.GetValue(item)!.ToString() ?? string.Empty)
                .Contains(_Filters[property.Name].Value, StringComparison.CurrentCultureIgnoreCase)
        };

    private object? _GetSort(System.Type propertyType, object? value)
        => propertyType.Name switch
        {
            nameof(Timestamp) => (value as Timestamp)?.ToDateTime(),
            nameof(Presale) => (value as Presale)?.Name.GetFirstAndLastName() ?? Localization["PresaleNotAssignedYetMessage"],
            _ => value
        };

    private string _GetHeaderTitle(string propertyName)
        => propertyName switch
        {
            _ => Localization[$"{typeof(TGridItem).Name}{propertyName}Title"]
        };

    private string _GetСontentConverter(System.Type propertyType, object? value, string? numberGroupSeparator = null)
        => propertyType.Name switch
        {
            nameof(Timestamp) => $"{(value as Timestamp)?.ToDateTime().ToPresaleTime().ToString(Helpers.DateTimeFormat)}",
            nameof(Presale) => (value as Presale)?.Name.GetFirstAndLastName()
                ?? Localization["PresaleNotAssignedYetMessage"],
            nameof(DecimalValue) => (value as DecimalValue).ToCurrencyString(groupSeparator: numberGroupSeparator),
            _ => propertyType.IsEnum
                ? Localization[$"{propertyType.Name}{System.Enum.Parse(
                    propertyType, value?.ToString() ?? "")}Text"]
                : $"{value?.ToString()}"
        };

    private async Task _OnDropDownPopupOpen(string popupId)
    {
        var width = _GetHumanComparsionTypes().Max(x => Localization[$"ComparsionType{x}Text"].Value.Length);
        width *= 9;
        await Helpers.SetElementMinWidthById(_JSRuntime, popupId, $"{width}px");
    }

    private void _OnComparsionTypeChange(object args, string propertyName)
    {
        if (args is ComparsionType type)
            _Filters[propertyName].Comparsion = type;
    }

    private void _BindedSet(string propertyName, ChangeEventArgs args)
        => _Filters[propertyName].Value = args.Value?.ToString() ?? string.Empty;

    private void _BindedSet(string propertyName, DateTime? args)
    {
        var some = $"{args?.ToString()}";
        _Filters[propertyName].Value = $"{args?.ToString()}";
    }

    private async Task _DownloadFile()
    {
        if (_FilteredItems is not null)
        {
            var text = string.Empty;

            foreach (var property in Helpers.GetProperties<TGridItem>())
            {
                if (!Columns.Contains(property.Name))
                    continue;
                text += $"{_GetHeaderTitle(property.Name)};";
            }

            text = text[..^1];
            text += "\n";

            foreach (var item in _FilteredItems)
            {
                foreach (var property in Helpers.GetProperties<TGridItem>())
                {
                    if (!Columns.Contains(property.Name))
                        continue;
                    text += $"{_GetСontentConverter(property.PropertyType,
                        property.GetValue(item), numberGroupSeparator: string.Empty)};";
                }

                text = text[..^1];
                text += "\n";
            }

            await Helpers.DownloadReport(text, _JSRuntime,
                $"{(MarkupString)Localization[$"{typeof(TGridItem).Name}ReportFileName", DateTime.Now].Value}.csv");
        }
    }

    private void _ShowFiltersToggle()
    {
        ShowFilters = !ShowFilters;

        if (!ShowFilters)
            _Filters = _Filters.ToDictionary(x => x.Key, x => new _Filter(string.Empty, _Filters[x.Key].Comparsion));
    }

    private void _ShowSettingsToggle() => ShowSettings = !ShowSettings;

    private async Task _OpenColumnsSelector()
        => await _DialogService.OpenAsync<ColumnsSelector<Project>>(Localization["SelectColumnsButtonText"],
            new()
            {
                { nameof(ColumnsSelector<Project>.Values), Columns },
                { nameof(ColumnsSelector<Project>.OnSelectCallback), _OnColumnsChanged }
            }, ColumnsSelector<Project>.DefaultDialogOptions);

    #endregion

    #endregion

    protected override void OnInitialized()
    {
        _SettingsFragment = Settings ?? _RenderEmpty;
        _ColumnsFragment = _RenderColumns;
    }

    protected override void OnParametersSet()
        => _Filters = Helpers.GetProperties<TGridItem>()
           .ToDictionary(x => x.Name, x => new _Filter(string.Empty, x.PropertyType.Name switch
           {
               nameof(DecimalValue) or nameof(Timestamp) => ComparsionType.GreaterThanOrEqual,
               _ => ComparsionType.Contains
           }));

    private class _Filter(string value, ComparsionType comparsionType)
    {
        public string Value { get; set; } = value;

        public ComparsionType Comparsion { get; set; } = comparsionType;
    }
}
