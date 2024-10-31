namespace PredictionAPI
{
    public partial class App : Application
    {
        public App() //PredictionPage predictionPage
        {
            InitializeComponent();

            //MainPage = new AppShell();
            MainPage = new PredictionPage();
        }
    }
}
