using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        private ObservableCollection<List<string>> _peopleGroups;
        private int _groupSize;
        public ObservableCollection<string> Rosters 
        {
            get { return _rosters; }
            set
            {
                _rosters = value;
                OnPropertyChanged(nameof(Rosters));
            }
        }
        public string SelectedRosterName
        {
            get { return _selectedRosterName; }
            set
            {
                _selectedRosterName = value;
                OnPropertyChanged(nameof(SelectedRosterName));
            }
        }
        public ObservableCollection<string> SelectedRoster
        {
            get { return _selectedRoster; }
            set
            {
                _selectedRoster = value;
                OnPropertyChanged(nameof(SelectedRoster));
                OnPropertyChanged(nameof(GroupButtonIsEnabled));
            }
        }
        public ObservableCollection<List<string>> PeopleGroups
        {
            get { return _peopleGroups; }
            set
            {
                _peopleGroups = value;
                OnPropertyChanged(nameof(PeopleGroups));
            }
        }
        public int GroupSize
        {
            get { return _groupSize; }
            set
            {
                if (_groupSize != value)
                {
                    _groupSize = value;
                    OnPropertyChanged(nameof(GroupSize));
                    OnPropertyChanged(nameof(GroupButtonIsEnabled));
                }
            }
        }
        public ICommand SelectRosterCommand { get; private set; }
        public ICommand ShowNewRosterPromptCommand { get; private set; }
        public ICommand ShowAddPersonToRosterPromptCommand { get; private set; }
        public ICommand GroupPeopleCommand { get; private set; }

        public bool GroupButtonIsEnabled => IsValidTargetGroupSize();

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

            GroupPeopleCommand = new Command(GroupPeople);
            GroupSize = 4;
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

        private bool IsValidTargetGroupSize()
        {
            if (SelectedRoster is null) return false;
            if (SelectedRoster.Count < 1) return false;
            if (GroupSize < 2) return false;

            int remainder = SelectedRoster.Count % GroupSize;
            int minGroupsCount = remainder == 0 ? 0 : GroupSize - remainder;
            int maxGroupsCount = remainder == 0 ? SelectedRoster.Count / GroupSize : (SelectedRoster.Count - (minGroupsCount * (GroupSize - 1))) / GroupSize;

            if (maxGroupsCount < 1) return false;

            return true;
        }
        private void GroupPeople()
        {
            if (SelectedRoster is not null)
            {
                var people = SelectedRoster.ToList();
                Shuffle(people);

                int remainder = SelectedRoster.Count % GroupSize;
                int minGroupsCount = remainder == 0 
                    ? 0 
                    : GroupSize - remainder;
                int maxGroupsCount = remainder == 0 
                    ? SelectedRoster.Count / GroupSize 
                    : (SelectedRoster.Count - (minGroupsCount * (GroupSize - 1))) / GroupSize;
                int groupsCount = minGroupsCount + maxGroupsCount;

                ObservableCollection<List<string>> groups = new();
                int index = 0;
                for (int i = 0; i < groupsCount; i++)
                {
                    int n = i < maxGroupsCount ? GroupSize : GroupSize - 1;
                    List<string> group = new();

                    for (int j = 0; j < n; j++)
                    {
                        group.Add(people[index]);
                        index++;
                    }

                    groups.Add(group);
                }

                PeopleGroups = groups;
            }
        }
        private static void Shuffle<T>(IList<T> list)
        {
            var rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
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
