using Microsoft.AspNetCore.Components;
using PresalesApp.Web.Client.Helpers;
using PresalesApp.Web.Client.Views;
using PresalesApp.Web.Shared;
using Period = PresalesApp.Web.Client.Helpers.Period;

namespace PresalesApp.Web.Client.Pages
{
    partial class Unpaid
    {
        [CascadingParameter]
        public MessageSnackbar GlobalMsgHandler { get; set; }

        #region Private Members
        private UnpaidProjects? _response;
        private static string _presale_name = string.Empty;
        private bool _is_main_project_include = false;
        private static Period _period = new(new(DateTime.Now.Year, DateTime.Now.Month, 1), Enums.PeriodType.Month);
        private Dictionary<string, object> _btn_attrs = new() { { "disabled", "disabled" } };
        #endregion

        #region UriQuery
        private const string q_start = "Start";
        [SupplyParameterFromQuery(Name = q_start)] public string? Start { get; set; }

        private const string q_end = "End";
        [SupplyParameterFromQuery(Name = q_end)] public string? End { get; set; }

        private const string q_period_type = "Period";
        [SupplyParameterFromQuery(Name = q_period_type)] public string? PeriodType { get; set; }

        private const string q_presale = "Presale";
        [SupplyParameterFromQuery(Name = q_presale)] public string? PresaleName { get; set; }

        private static Dictionary<string, object?> GetQueryKeyValues() => new()
        {
            [q_start] = _period.Start.ToString(Helpers.Helpers.UriDateTimeFormat),
            [q_end] = _period.End.ToString(Helpers.Helpers.UriDateTimeFormat),
            [q_period_type] = _period.Type.ToString(),
            [q_presale] = _presale_name,
        };
        #endregion

        protected override async Task OnInitializedAsync()
        {
            Helpers.Helpers.SetFromQueryOrStorage(value: Start, query: q_start, uri: Navigation.Uri, storage: Storage, param: ref _period.Start);
            Helpers.Helpers.SetFromQueryOrStorage(value: End, query: q_end, uri: Navigation.Uri, storage: Storage, param: ref _period.End);
            Helpers.Helpers.SetFromQueryOrStorage(value: PeriodType, query: q_period_type, uri: Navigation.Uri, storage: Storage, param: ref _period.Type);
            Helpers.Helpers.SetFromQueryOrStorage(value: PresaleName, query: q_presale, uri: Navigation.Uri, storage: Storage, param: ref _presale_name);

            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));
            await UpdateData();
        }

        #region Private Methods
        private async Task UpdateData()
        {
            try
            {
                _response = await AppApi.GetUnpaidProjectsAsync(new UnpaidRequest()
                {
                    IsMainProjectInclude = _is_main_project_include,
                    PresaleName = _presale_name,
                    Period = _period.Translate()
                });
                _btn_attrs = [];
            }
            catch (Exception e)
            {
                _btn_attrs = new() { { "disabled", "disabled" } };
                await GlobalMsgHandler.Show(e.Message);
            }
            StateHasChanged();
        }

        private async Task OnPresaleChanged(string name)
        {
            _presale_name = name;

            Storage.SetItemAsString($"{new Uri(Navigation.Uri).LocalPath}.{q_presale}", _presale_name);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await UpdateData();
        }

        private async Task OnPeriodChanged(Period period)
        {
            _period = period;

            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_start}", _period.Start);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_end}", _period.End);
            Storage.SetItem($"{new Uri(Navigation.Uri).LocalPath}.{q_period_type}", _period.Type);
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameters(GetQueryKeyValues()));

            await UpdateData();
        }

        private async Task DownloadFile() => await _response.Download(js, Localization);

        private async void OnModeChanged(object? obj)
        {
            _is_main_project_include = obj == null ? _is_main_project_include : (bool)obj;
            await UpdateData();
        }
        #endregion
    }
}
