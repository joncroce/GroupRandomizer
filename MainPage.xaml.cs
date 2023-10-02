using CommunityToolkit.Mvvm.Messaging;
using GroupRandomizer.Messages;

namespace GroupRandomizer
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();            

            WeakReferenceMessenger.Default.Register<ShowAddRosterPromptMessage>(this, async (sender, args) => 
            {
                var result = await DisplayPromptAsync("Add New Roster", "Please enter a name for your new roster.");
                if (result != null)
                {
                    WeakReferenceMessenger.Default.Send(new AddRosterPromptResultMessage(result));
                }
            });

            WeakReferenceMessenger.Default.Register<ShowAddPersonToRosterPromptMessage>(this, async (sender, args) =>
            {
                var result = await DisplayPromptAsync("Add Person to Roster", "Please enter a name for the person to add to the roster.");
                if (result != null)
                {
                    WeakReferenceMessenger.Default.Send(new AddPersonToRosterPromptResultMessage(result));
                }
            });

            WeakReferenceMessenger.Default.Register<ShowDeleteSelectedRosterAlertMessage>(this, async (sender, args) =>
            {
                bool answer = await DisplayAlert("Confirm Roster Deletion", "Are you sure you want to delete this roster?", "Yes", "No");
                if (answer)
                {
                    WeakReferenceMessenger.Default.Send(new DeleteSelectedRosterAlertResultMessage(answer));
                }
            });
        }
    }
}