using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using GroupRandomizer.Messages;
using GroupRandomizer.Services;

namespace GroupRandomizer.ViewModels
{
    public class RosterViewModel : INotifyPropertyChanged
    {
        private readonly RosterService _rosterService;
        private ObservableCollection<string> _rosters;
        private string _selectedRosterName;
        private ObservableCollection<string> _selectedRoster;
        public ObservableCollection<string> Rosters 
        {
            get { return _rosters; }
            set
            {
                _rosters = value;
                OnPropertyChanged();
            }
        }
        public string SelectedRosterName
        {
            get { return _selectedRosterName; }
            set
            {
                _selectedRosterName = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<string> SelectedRoster
        {
            get { return _selectedRoster; }
            set
            {
                _selectedRoster = value;
                OnPropertyChanged();
            }
        }
        public ICommand SelectRosterCommand { get; private set; }
        public ICommand ShowNewRosterPromptCommand { get; private set; }
        public ICommand ShowAddPersonToRosterPromptCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RosterViewModel()
        {
            _rosterService = new RosterService();

            LoadRostersAsync();

            SelectRosterCommand = new Command<string>(
                execute: async (arg) =>
                {
                    if (arg == null)
                    {
                        SelectedRoster = null;
                    }
                    else
                    {
                        SelectedRoster = new ObservableCollection<string>(await _rosterService.GetRosterAsync(arg));
                    }

                    SelectedRosterName = arg;
                }
                );

            ShowNewRosterPromptCommand = new Command(ShowNewRosterPrompt);

            WeakReferenceMessenger.Default.Register<AddRosterPromptResultMessage>(this, (sender, args) =>
            {
                AddRoster(args.Result);
            });

            ShowAddPersonToRosterPromptCommand = new Command(ShowAddPersonToRosterPrompt);

            WeakReferenceMessenger.Default.Register<AddPersonToRosterPromptResultMessage>(this, (sender, args) =>
            {
                if (SelectedRosterName != null && SelectedRoster != null)
                {
                    AddPersonToSelectedRoster(args.Result);
                }
            });
        }

        public async Task LoadRostersAsync()
        {
            List<string> fileNames = await _rosterService.GetRosterFilesAsync();
            Rosters = new ObservableCollection<string>(fileNames);
        }

        private void AddRoster(string name)
        {
            SelectedRosterName = name;
            SelectedRoster = new ObservableCollection<string>();
            Rosters.Add(name);
            Save();
        }

        private void AddPersonToSelectedRoster(string name)
        {
            if (name.Length > 0 && SelectedRosterName != null && SelectedRoster != null)
            {
                SelectedRoster.Add(name);
                Save();
            }
        }

        private async void Save()
        {
            await _rosterService.SaveRosterAsync(SelectedRosterName, SelectedRoster.ToList());
        }

        public static void ShowNewRosterPrompt()
        {
            WeakReferenceMessenger.Default.Send(new ShowAddRosterPromptMessage());
        }

        public static void ShowAddPersonToRosterPrompt()
        {
            WeakReferenceMessenger.Default.Send(new ShowAddPersonToRosterPromptMessage());
        }
    }
}
