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

        private Period Period = new(new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), Enums.PeriodType.Month);

        private Department department = Department.Any;
        private Position position = Position.Any;
        private Overview? overview;
        private bool only_active = true;
        private readonly PeriodicTimer periodic_timer = new(TimeSpan.FromMinutes(10));
        #region Descriptions
        private string GetTitleInWork() => $"В работе (есть действия за {Period.GetLocalizedPeriodName(Localization)})";
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

        private static string Format(Project project) =>
            $"{project.Number}, {project.Name}, " +
            $"{Helpers.Helpers.ToOneDateString(project.ApprovalByTechDirectorAt, project.ApprovalBySalesDirectorAt)}" +
            $"{Helpers.Helpers.ToDateString(project.PresaleStartAt, " - ")}" +
            $"{(string.IsNullOrEmpty(project.Presale?.Name.GetFirstAndLastName()) ? "" : $", {project.Presale?.Name.GetFirstAndLastName()}")}";

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            periodic_timer?.Dispose();
        }

        private async void RunTimer()
        {
            while (await periodic_timer.WaitForNextTickAsync())
                await UpdateData();
        }

        private async Task UpdateData()
        {
            try
            {
                overview = await AppApi.GetOverviewAsync(new OverviewRequest
                {
                    OnlyActive = only_active,
                    Department = department,
                    Position = position,
                    Period = Period.Translate()
                });
                StateHasChanged();
            }
            catch (Exception e)
            {
                await GlobalMsgHandler.Show(e.Message);
            }
        }

        protected override void OnInitialized() => RunTimer();

        private async void OnStatusChanged(object? obj)
        {
            only_active = obj == null ? only_active : (bool)obj;
            await UpdateData();
        }

        protected override async Task OnParametersSetAsync() => await UpdateData();
    }
}
