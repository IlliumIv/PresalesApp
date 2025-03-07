using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen.Blazor;

namespace PresalesApp.Web.Client.Views.Pickers.DropDown;

public class CustomRadzenDropDown<TValue> : RadzenDropDown<TValue>
{
    #region Parameters

    [Parameter]
    public EventCallback<string> PopupId { get; set; }

    #endregion

    #region Private

    #region Members

    private bool _IsOpen;

    #endregion

    #endregion

    #region Public

    #region Methods

    public async Task OpenPopupAsync(string key = "ArrowDown", bool isFilter = false, bool isFromClick = false)
        => await OpenPopup(key, isFilter, isFromClick);

    public async Task ClosePopup(string key)
    {
        var of = false;

        if (key == "Enter")
        {
            of = OpenOnFocus;
            OpenOnFocus = false;
        }

        _IsOpen = false;
        await JSRuntime.InvokeVoidAsync("Radzen.closePopup", PopupID, Reference, nameof(OnClose));

        if (key == "Enter")
        {
            await JSRuntime.InvokeVoidAsync("Radzen.focusElement", UniqueID);
            OpenOnFocus = of;
        }
    }

    public async Task PopupClose()
    {
        _IsOpen = false;
        await JSRuntime.InvokeVoidAsync("Radzen.closePopup", PopupID, Reference, nameof(OnClose));
    }

    public void SetSelectedIndex(int index) => selectedIndex = index;

    #endregion

    #endregion

    protected override async Task OpenPopup(string key = "ArrowDown", bool isFilter = false, bool isFromClick = false)
    {
        _ = Task.Run(() => PopupId.InvokeAsync(PopupID));
        await base.OpenPopup(key, isFilter, isFromClick);
    }
}