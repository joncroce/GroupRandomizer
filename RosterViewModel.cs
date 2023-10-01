using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using GroupRandomizer.Messages;
using GroupRandomizer.Models;

namespace GroupRandomizer
{
    public class RosterViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Roster> Rosters { get; set; }
        private Roster _selectedRoster;
        public Roster SelectedRoster
        {
            get => _selectedRoster;
            set
            {
                _selectedRoster = value;
                OnPropertyChanged();
            }
        }

        public ICommand ShowNewRosterPromptCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RosterViewModel()
        {
            Rosters = new ObservableCollection<Roster>();
            LoadRostersFromDatabase();

            if (Rosters.Count > 0)
            {
                SelectedRoster = Rosters[0];
            }

            ShowNewRosterPromptCommand = new Command(ShowNewRosterPrompt);

            WeakReferenceMessenger.Default.Register<AddRosterPromptResultMessage>(this, (sender, args) =>
            {
                AddRoster(args.Result);
            });
        }

        private void LoadRostersFromDatabase()
        {
            var rosters = App.RosterRepo.GetAllRosters();

            if (rosters.Count > 0)
            {
                Rosters.Clear();
                foreach (var roster in rosters)
                {
                    Rosters.Add(roster);
                }

                OnPropertyChanged(nameof(Rosters));
            }

        }

        private void AddRoster(string name)
        {
            App.RosterRepo.AddNewRoster(name);
            LoadRostersFromDatabase();
            SelectRoster(name);
        }

        public static void ShowNewRosterPrompt()
        {
            WeakReferenceMessenger.Default.Send(new ShowAddRosterPromptMessage());
        }

        private void SelectRoster(string name)
        {
            SelectedRoster = Rosters.FirstOrDefault(roster => roster.Name == name);
        }
    }
}
