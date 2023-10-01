namespace GroupRandomizer
{
    public partial class App : Application
    {
        public static RosterRepository RosterRepo { get; private set; }
        public App(RosterRepository rosterRepo)
        {
            InitializeComponent();

            MainPage = new AppShell();

            RosterRepo = rosterRepo;
        }
    }
}