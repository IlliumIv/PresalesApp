using Microsoft.AspNetCore.Components;
using PresalesApp.Shared;
using Radzen;
using PresalesApp.Web.Client.Extensions;

namespace PresalesApp.Web.Client.Pages;

partial class Unpaid
{
    [Inject]
    private NotificationService _NotificationService { get; set; }

    #region Private

    #region Members

    private IEnumerable<Project>? _ProjectsQueryable;

    private string _PresaleName = string.Empty;

    private bool _Is_MainProjectInclude = false;

    private Period _Period = new(new(DateTime.Now.Year, DateTime.Now.Month, 1),
        CustomTypes.PeriodType.Month);

    #endregion

    #region Methods

    private async Task _UpdateData()
    {
        try
        {
            _ProjectsQueryable = (await PresalesAppApi.GetUnpaidProjectsAsync(new()
            {
                IsMainProjectInclude = _Is_MainProjectInclude,
                PresaleName = _PresaleName,
                Period = _Period.Translate()
            })).Projects;
        }
        catch (Exception e)
        {
            _NotificationService.Notify(NotificationSeverity.Error,
                e.Message);
        }

        StateHasChanged();
    }

    private async Task _OnPresaleChanged(string name)
    {
        _PresaleName = name;

        Storage.SetItemAsString($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PresaleName}", _PresaleName);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _UpdateData();
    }

    private async Task _OnPeriodChanged(Period period)
    {
        _Period = period;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_Start}", _Period.Start);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_End}", _Period.End);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PeriodType}", _Period.Type);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _UpdateData();
    }

    private async Task _OnModeChanged(object? obj)
    {
        _Is_MainProjectInclude = obj == null ? _Is_MainProjectInclude : (bool)obj;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_IncludeMain}",
            _Is_MainProjectInclude);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _UpdateData();
    }

    #endregion

    #endregion

    #region UriQuery

    private const string _Q_Start = "Start";
    [SupplyParameterFromQuery(Name = _Q_Start)]
    public string? Start { get; set; }

    private const string _Q_End = "End";
    [SupplyParameterFromQuery(Name = _Q_End)]
    public string? End { get; set; }

    private const string _Q_PeriodType = "Period";
    [SupplyParameterFromQuery(Name = _Q_PeriodType)]
    public string? PeriodType { get; set; }

    private const string _Q_PresaleName = "Presale";
    [SupplyParameterFromQuery(Name = _Q_PresaleName)]
    public string? PresaleName { get; set; }

    private const string _Q_IncludeMain = "IncludeMain";
    [SupplyParameterFromQuery(Name = _Q_IncludeMain)]
    public string? IncludeMain { get; set; }

    private Dictionary<string, object?> _GetQueryKeyValues() => new()
    {
        [_Q_Start] = _Period.Start.ToString(Helpers.UriDateTimeFormat),
        [_Q_End] = _Period.End.ToString(Helpers.UriDateTimeFormat),
        [_Q_PeriodType] = _Period.Type.ToString(),
        [_Q_PresaleName] = _PresaleName,
        [_Q_IncludeMain] = _Is_MainProjectInclude.ToString(),
    };

    #endregion

    protected override async Task OnInitializedAsync()
    {
        Helpers.SetFromQueryOrStorage(value: Start, query: _Q_Start,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.Start);
        Helpers.SetFromQueryOrStorage(value: End, query: _Q_End,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.End);
        Helpers.SetFromQueryOrStorage(value: PeriodType, query: _Q_PeriodType,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.Type);
        Helpers.SetFromQueryOrStorage(value: PresaleName, query: _Q_PresaleName,
            uri: Navigation.Uri, storage: Storage, param: ref _PresaleName);
        Helpers.SetFromQueryOrStorage(value: IncludeMain, query: _Q_IncludeMain,
            uri: Navigation.Uri, storage: Storage, param: ref _Is_MainProjectInclude);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));
        await _UpdateData();
    }
}
