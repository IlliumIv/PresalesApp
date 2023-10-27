namespace PresalesApp.Web.Client.Views.Pickers.DropDown
{
    public class DropDownItem<T>(T value, string text)
    {
        public T Value { get; set; } = value;
        public string Text { get; set; } = text;
    }
}
