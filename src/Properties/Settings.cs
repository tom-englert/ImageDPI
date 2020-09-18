namespace ImageDPI.Properties
{
    using System.ComponentModel;

    internal partial class Settings
    {
        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(sender, e);

            Save();
        }
    }
}
