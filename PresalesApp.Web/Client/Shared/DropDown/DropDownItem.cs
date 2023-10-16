namespace PresalesApp.Web.Client.Shared.DropDown
{
    public class DropDownItem<T>
    {
        public T Value { get; set; }

        public string Text { get; set; }

        public DropDownItem(T value, string text)
        {
            Value = value;
            Text = text;
        }
    }
}
