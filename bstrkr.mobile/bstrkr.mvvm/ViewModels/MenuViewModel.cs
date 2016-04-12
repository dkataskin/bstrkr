namespace bstrkr.mvvm.viewmodels
{
    public class MenuViewModel : BusTrackerViewModelBase
    {
        private string _title;
        private MenuSection _section;

        public string Title
        {
            get { return _title; }
            set
            {
                if (!string.Equals(_title, value))
                {
                    _title = value;
                    this.RaisePropertyChanged(() => this.Title);
                }
            }
        }

        public MenuSection Section
        {
            get { return _section; }
            set
            {
                if (_section != value)
                {
                    _section = value;
                    this.Id = (int)value;
                    this.RaisePropertyChanged(() => this.Section);
                }
            }
        }
    }
}