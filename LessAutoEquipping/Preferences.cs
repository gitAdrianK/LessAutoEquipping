using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LessAutoEquipping
{
    public class Preferences : INotifyPropertyChanged
    {
        private bool _preventAutoEquip = false;

        public bool ShouldPreventAutoEquip
        {
            get => _preventAutoEquip;
            set
            {
                _preventAutoEquip = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
