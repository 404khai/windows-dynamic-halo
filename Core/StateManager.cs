using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WindowsDynamicHalo.Core
{
    public class StateManager : INotifyPropertyChanged
    {
        private static StateManager? _instance;
        public static StateManager Instance => _instance ??= new StateManager();

        private IslandState _currentState = new IslandState();

        public IslandState CurrentState
        {
            get => _currentState;
            private set
            {
                _currentState = value;
                OnPropertyChanged();
            }
        }

        public void SetMode(IslandMode mode)
        {
            if (_currentState.Mode != mode)
            {
                _currentState.Mode = mode;
                OnPropertyChanged(nameof(CurrentState));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
