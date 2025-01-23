using Microsoft.AspNetCore.Components;
using PresalesApp.CustomTypes;
using PresalesApp.Shared;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;

namespace PresalesApp.Web.Client.Pages;

partial class Spreadsheet
{
    [CascadingParameter]
    public MessageSnackbar GlobalMsgHandler { get; set; }

    #region Private Members

    private Helpers.Period _Period = new(new DateTime(DateTime.Now.Year,
        DateTime.Now.Month, 1).ToUniversalTime(),
        CustomTypes.PeriodType.Month);

    private Department _Department = Department.Any;
    
    private Position _Position = Position.Any;
    
    private bool _OnlyActiveOnes = true;
    
    private readonly PeriodicTimer _PeriodicTimer = new(TimeSpan.FromMinutes(10));
    
    private GetOverviewResponse? _OverviewResponse;

    #region Descriptions

    private string _GetTitleInWork() => $"В работе (есть действия за " +
        $"{_Period.GetLocalizedPeriodName(Localization)})";

    private readonly string _TitleAssign = "Назначено";

    private readonly string _TitleWon = "Выиграно";

    private readonly string _TitleLoss = "Проиграно";

    private readonly string _TitleConversion = "Конверсия";

    private readonly string _TitleAbandoned = "Заброшено (нет действий за последние 30 дней)";

    private readonly string _TitleAvgTimeToWin = "Среднее время жизни проекта до выигрыша в днях";

    private readonly string _TtitleAvgTimeToReaction = "Среднее время реакции пресейла в минутах";
    
    private readonly string _TitleAvgRank = "Средний ранг проектов";

    private readonly string _TitleTimeSpend = "Потрачено времени на проекты, суммарно в часах";

    private readonly string _TitleAvtTimeSpend = "Потрачено времени на проекты, в среднем в часах";
    
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

    private const string _Q_DepartmentType = "Department";
    [SupplyParameterFromQuery(Name = _Q_DepartmentType)]
    public string? DepartmentType { get; set; }

    private const string _Q_PositionType = "Position";
    [SupplyParameterFromQuery(Name = _Q_PositionType)]
    public string? PositionType { get; set; }

    private const string _Q_OnlyActiveOnes = "OnlyActive";
    [SupplyParameterFromQuery(Name = _Q_OnlyActiveOnes)]
    public string? OnlyActiveOnes { get; set; }

    private Dictionary<string, object?> _GetQueryKeyValues() => new()
    {
        [_Q_Start] = _Period.Start.ToString(Helper.UriDateTimeFormat),
        [_Q_End] = _Period.End.ToString(Helper.UriDateTimeFormat),
        [_Q_PeriodType] = _Period.Type.ToString(),
        [_Q_DepartmentType] = _Department.ToString(),
        [_Q_PositionType] = _Position.ToString(),
        [_Q_OnlyActiveOnes] = _OnlyActiveOnes.ToString(),
    };
    #endregion

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _PeriodicTimer?.Dispose();
    }

    protected override async Task OnInitializedAsync()
    {
        Helper.SetFromQueryOrStorage(value: Start, query: _Q_Start,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.Start);
        Helper.SetFromQueryOrStorage(value: End, query: _Q_End,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.End);
        Helper.SetFromQueryOrStorage(value: PeriodType, query: _Q_PeriodType,
            uri: Navigation.Uri, storage: Storage, param: ref _Period.Type);

        Helper.SetFromQueryOrStorage(value: DepartmentType, query: _Q_DepartmentType,
            uri: Navigation.Uri, storage: Storage, param: ref _Department);
        Helper.SetFromQueryOrStorage(value: PositionType, query: _Q_PositionType,
            uri: Navigation.Uri, storage: Storage, param: ref _Position);
        Helper.SetFromQueryOrStorage(value: OnlyActiveOnes, query: _Q_OnlyActiveOnes,
            uri: Navigation.Uri, storage: Storage, param: ref _OnlyActiveOnes);

        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));
        _RunTimer();
        await _UpdateData();
    }

    #region Private Methods

    private async Task _OnPeriodChanged(Helpers.Period period)
    {
        _Period = period;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_Start}", _Period.Start);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_End}", _Period.End);
        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PeriodType}", _Period.Type);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _UpdateData();
    }

    private async Task _OnDepartmentChange(Department department)
    {
        _Department = department;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_DepartmentType}", _Department);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _UpdateData();
    }

    private async Task _OnPositionChange(Position position)
    {
        _Position = position;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{_Q_PositionType}", _Position);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _UpdateData();
    }

    private static string _Format(Project project)
        => $"{(string.IsNullOrEmpty(project.Presale?.Name.GetFirstAndLastName())
            ? ""
            : $"{project.Presale?.Name.GetFirstAndLastName()}, ")}" +
              $"{project.Number}, {project.Name}, " +
              $"{Helper.ToOneDateString(project.ApprovalByTechDirectorAt,
                  project.ApprovalBySalesDirectorAt)}" +
              $"{Helper.ToDateString(project.PresaleStartAt, " - ")}";

    private async void _RunTimer()
    {
        while (await _PeriodicTimer.WaitForNextTickAsync())
            await _UpdateData();
    }

    private async Task _UpdateData()
    {
        try
        {
            _OverviewResponse = await PresalesAppApi.GetOverviewAsync(new()
            {
                OnlyActive = _OnlyActiveOnes,
                Department = _Department,
                Position = _Position,
                Period = _Period.Translate()
            });
            StateHasChanged();
        }
        catch (Exception e)
        {
            GlobalMsgHandler.Show(e.Message);
        }
    }

    private async void _OnStatusChange(ChangeEventArgs e)
    {
        _OnlyActiveOnes = e?.Value == null ? _OnlyActiveOnes : (bool)e.Value;

        Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}" +
            $".{_Q_OnlyActiveOnes}",_OnlyActiveOnes);
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(_GetQueryKeyValues()));

        await _UpdateData();
    }

    #endregion
}
