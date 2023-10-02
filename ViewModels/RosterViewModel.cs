using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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
        private ObservableCollection<Person> _selectedRoster;
        private ObservableCollection<Group> _peopleGroups;
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
        public ObservableCollection<Person> SelectedRoster
        {
            get { return _selectedRoster; }
            set
            {
                _selectedRoster = value;
                OnPropertyChanged(nameof(SelectedRoster));
                OnPropertyChanged(nameof(GroupButtonIsEnabled));
            }
        }
        public ObservableCollection<Group> PeopleGroups
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
        public ICommand TogglePersonPresentCommand { get; private set; }

        public bool GroupButtonIsEnabled => IsValidTargetGroupSize();

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class Person : INotifyPropertyChanged
        {
            private string _name;
            private bool _isPresent;

            public string Name { 
                get { return _name; } 
                set { 
                    _name = value;
                    OnPropertyChanged();
                } 
            }
            public bool IsPresent { 
                get { return _isPresent; } 
                set { 
                    _isPresent = value;
                    OnPropertyChanged();
                } 
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            public Person(string name)
            {
                Name = name;
                IsPresent = true;
            }

            public void ToggleIsPresent()
            {
                IsPresent = !IsPresent;
            }
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
                        List<string> names = await _rosterService.GetRosterAsync(arg);
                        List<Person> people = new List<Person>();

                        foreach (var name in names)
                        {
                            people.Add(new Person(name));
                        }
                        
                        SelectedRoster = new ObservableCollection<Person>(people);
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

            TogglePersonPresentCommand = new Command<string>(
                execute: (arg) =>
                {
                    Person person = SelectedRoster.FirstOrDefault((person) => person.Name == arg);

                    person?.ToggleIsPresent();
                }
            );

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
            SelectedRoster = new ObservableCollection<Person>();
            Rosters.Add(name);
            Save();
        }

        private void AddPersonToSelectedRoster(string name)
        {
            if (name.Length > 0 && SelectedRosterName != null && SelectedRoster != null)
            {
                SelectedRoster.Add(new Person(name));
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
        public class Group
        {
            private string _headerText;
            private List<string> _data;

            public string HeaderText { 
                get { return _headerText; }
                set { _headerText = value; }
            }
            public List<string> Data
            {
                get { return _data; }
                set { _data = value; }
            }

            public Group(int number, List<string> data)
            {
                HeaderText = String.Format("Group # {0}", number);
                Data = data;
            }

            public void AddPerson(string person)
            {
                Data.Add(person);
            }
        }
        private void GroupPeople()
        {
            if (SelectedRoster is not null)
            {
                var presentPeople = SelectedRoster.Where(p => p.IsPresent).ToList();
                Shuffle(presentPeople);

                int remainder = presentPeople.Count % GroupSize;
                int minGroupsCount = remainder == 0 
                    ? 0 
                    : GroupSize - remainder;
                int maxGroupsCount = remainder == 0 
                    ? presentPeople.Count / GroupSize 
                    : (presentPeople.Count - (minGroupsCount * (GroupSize - 1))) / GroupSize;
                int groupsCount = minGroupsCount + maxGroupsCount;

                ObservableCollection<Group> groups = new();
                int index = 0;
                for (int i = 0; i < groupsCount; i++)
                {
                    int n = i < maxGroupsCount ? GroupSize : GroupSize - 1;
                    Group group = new(i + 1, new List<string>());

                    for (int j = 0; j < n; j++)
                    {
                        group.AddPerson(presentPeople[index].Name);
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
            await _rosterService.SaveRosterAsync(SelectedRosterName, SelectedRoster.Select(p => p.Name).ToList());
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
