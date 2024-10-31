namespace PredictionAPI;

public partial class PredictionPage : ContentPage
{
    private readonly PredictionService _predictionService = new PredictionService();

    public PredictionPage() //PredictionService predictionService
    {
        InitializeComponent();
        //_predictionService = predictionService; // Dependency injected
    }

    private async void OnGetPredictionClicked(object sender, EventArgs e)
    {
        try
        {
            // Pass the path of the CSV file
            string prediction = await _predictionService.GetEnergyConsumptionPredictionAsync();
            predictionLabel.Text = prediction;
        }
        catch (Exception ex)
        {
            predictionLabel.Text = $"Error: {ex.Message}";
        }
    }
}
