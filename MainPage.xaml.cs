using CommunityToolkit.Mvvm.Messaging;
using GroupRandomizer.Messages;

namespace GroupRandomizer
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            BindingContext = new RosterViewModel();
            

            WeakReferenceMessenger.Default.Register<ShowAddRosterPromptMessage>(this, async (sender, args) => 
            {
                var result = await DisplayPromptAsync("Add New Roster", "Please enter a name for your new roster.");
                if (result != null)
                {
                    WeakReferenceMessenger.Default.Send(new AddRosterPromptResultMessage(result));
                }
            });
        }
    }
}