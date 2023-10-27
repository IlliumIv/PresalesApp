using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using Department = PresalesApp.Web.Shared.Department;
using Position = PresalesApp.Web.Shared.Position;
using Period = PresalesApp.Web.Client.Helpers.Period;
using PresalesApp.Web.Shared;

namespace PresalesApp.Web.Client.Pages
{
    partial class Spreadsheet
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        #region Private Members
        private static Period _period = new(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), Enums.PeriodType.Month);
        private static Department _department = Department.Any;
        private static Position _position = Position.Any;
        private static bool _only_active_ones = true;
        private readonly PeriodicTimer _periodic_timer = new(TimeSpan.FromMinutes(10));
        private Overview? _overview;
        #region Descriptions
        private string GetTitleInWork() => $"В работе (есть действия за {_period.GetLocalizedPeriodName(Localization)})";
        private readonly string title_assign = "Назначено";
        private readonly string title_won = "Выиграно";
        private readonly string title_loss = "Проиграно";
        private readonly string title_conversion = "Конверсия";
        private readonly string title_abandoned = "Заброшено (нет действий за последние 30 дней)";
        private readonly string title_avg_time_to_win = "Среднее время жизни проекта до выигрыша в днях";
        private readonly string title_avg_time_to_reaction = "Среднее время реакции пресейла в минутах";
        private readonly string title_avg_rank = "Средний ранг проектов";
        private readonly string title_time_spend = "Потрачено времени на проекты, суммарно в часах";
        private readonly string title_avt_time_spend = "Потрачено времени на проекты, в среднем в часах";
        #endregion
        #endregion

        #region UriQuery
        private const string q_start = "Start";
        [SupplyParameterFromQuery(Name = q_start)] public string? Start { get; set; }

        private const string q_end = "End";
        [SupplyParameterFromQuery(Name = q_end)] public string? End { get; set; }

        private const string q_period_type = "Period";
        [SupplyParameterFromQuery(Name = q_period_type)] public string? PeriodType { get; set; }

        private const string q_department = "Department";
        [SupplyParameterFromQuery(Name = q_department)] public string? DepartmentType { get; set; }

        private const string q_position = "Position";
        [SupplyParameterFromQuery(Name = q_position)] public string? PositionType { get; set; }

        private const string q_only_active_ones = "OnlyActive";
        [SupplyParameterFromQuery(Name = q_only_active_ones)] public string? OnlyActiveOnes { get; set; }

        private static Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [q_start] = _period.Start.ToString(Helpers.Helpers.UriDateTimeFormat),
            [q_end] = _period.End.ToString(Helpers.Helpers.UriDateTimeFormat),
            [q_period_type] = _period.Type.ToString(),
            [q_department] = _department.ToString(),
            [q_position] = _position.ToString(),
            [q_only_active_ones] = _only_active_ones.ToString(),
        };
        #endregion

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _periodic_timer?.Dispose();
        }

        protected override async Task OnInitializedAsync()
        {
            Helpers.Helpers.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref _period.Start);
            Helpers.Helpers.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref _period.End);
            Helpers.Helpers.SetFromQueryOrStorage(value: PeriodType, query: q_period_type, uri: Navigation.Uri, storage: Storage, param: ref _period.Type);
            Helpers.Helpers.SetFromQueryOrStorage(value: DepartmentType, query: q_department, uri: Navigation.Uri, storage: Storage, param: ref _department);
            Helpers.Helpers.SetFromQueryOrStorage(value: PositionType, query: q_position, uri: Navigation.Uri, storage: Storage, param: ref _position);
            Helpers.Helpers.SetFromQueryOrStorage(value: OnlyActiveOnes, query: q_only_active_ones, uri: Navigation.Uri, storage: Storage, param: ref _only_active_ones);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
            RunTimer();
            await UpdateData();
        }

        #region Private Methods
        private async Task OnPeriodChanged(Period period)
        {
            _period = period;

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_start}", _period.Start);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_end}", _period.End);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_period_type}", _period.Type);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await UpdateData();
        }

        private async Task OnDepartmentChange(Department department)
        {
            _department = department;

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_department}", _department);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await UpdateData();
        }

        private async Task OnPositionChange(Position position)
        {
            _position = position;

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_position}", _position);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await UpdateData();
        }

        private static string Format(Project project) =>
            $"{project.Number}, {project.Name}, " +
            $"{Helpers.Helpers.ToOneDateString(project.ApprovalByTechDirectorAt, project.ApprovalBySalesDirectorAt)}" +
            $"{Helpers.Helpers.ToDateString(project.PresaleStartAt, " - ")}" +
            $"{(string.IsNullOrEmpty(project.Presale?.Name.GetFirstAndLastName()) ? "" : $", {project.Presale?.Name.GetFirstAndLastName()}")}";

        private async void RunTimer()
        {
            while (await _periodic_timer.WaitForNextTickAsync())
                await UpdateData();
        }

        private async Task UpdateData()
        {
            try
            {
                _overview = await AppApi.GetOverviewAsync(new OverviewRequest
                {
                    OnlyActive = _only_active_ones,
                    Department = _department,
                    Position = _position,
                    Period = _period.Translate()
                });
                StateHasChanged();
            }
            catch (Exception e)
            {
                await GlobalMsgHandler.Show(e.Message);
            }
        }

        private async void OnStatusChange(ChangeEventArgs e)
        {
            _only_active_ones = e?.Value == null ? _only_active_ones : (bool)e.Value;

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_only_active_ones}", _only_active_ones);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await UpdateData();
        }
        #endregion
    }
}
