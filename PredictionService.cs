using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Newtonsoft.Json;
using RestSharp;
using Microsoft.Maui.Storage;
using Microsoft.Extensions.Configuration;

namespace PredictionAPI
{
    public class PredictionService
    {
        private readonly string apiKey;

        public PredictionService()
        {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY_Project24");

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("API key not set in environment variables. Read the readme file for guideance.");
            }

        }

        private static readonly string endpoint = "https://24240-m1ksp369-westeurope.openai.azure.com/openai/deployments/gpt-35-turbo/chat/completions?api-version=2024-05-01-preview";

        public async Task<string> GetEnergyConsumptionPredictionAsync()//string filename
        {
            // Read CSV content from the embedded resource
            //string promptData = await PrepareInputDataFromFile();
            string promptData = await GetFormattedWeatherData();

            // Prepare the JSON payload for the request
            var data = new
            {
                messages = new[]
                {
                    new { role = "system", content = "You are a prediction model that predicts energy consumption based on previous data" },
                    new { role = "user", content = $"Predict future energy consumption when Temperature is 20, Humidity is 80, Wind Speed is 11 based on the following data:\n{promptData}. I don't want any explanation or text, only a number. Answer in this format 'xxx kWh'" }
                    //new { role = "user", content = $"Predict energy consumption for 2024-10-04 based on the previous data: {promptData}. I want you to answer in this format only: 'xxx kWh' that is the only thing you answer. No 'ok' or 'sure'" }
                },
                max_tokens = 800,
                temperature = 0.7,
                top_p = 0.95,
                frequency_penalty = 0,
                presence_penalty = 0
            };

            // Create the HTTP request
            var client = new RestClient(endpoint);
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("api-key", apiKey);
            request.AddJsonBody(data);

            // Execute the request and process the response
            var response = await client.ExecutePostAsync(request);
            if (response.IsSuccessful)
            {
                var result = JsonConvert.DeserializeObject<dynamic>(response.Content);
                string assistantResponse = result.choices[0].message.content;
                return assistantResponse;
            }
            else
            {
                throw new Exception($"API call failed with status code: {response.StatusCode}\n{response.Content}");
            }
        }

        public async Task<string> PrepareInputDataFromList(List<WeatherEnergyData> weatherDataList)
        {
            // Use StringBuilder to construct the input string
            StringBuilder promptData = new StringBuilder();

            foreach (var record in weatherDataList)
            {
                promptData.AppendLine($"Date: {record.Date}, Temperature: {record.Temperature}°C, Humidity: {record.Humidity}%, Wind Speed: {record.WindSpeed} km/h, Energy Consumption: {record.EnergyConsumption} kWh");
            }

            return promptData.ToString();
        }

        // Define a class to map weather data
        public class WeatherEnergyData
        {
            public string Date { get; set; }
            public int Temperature { get; set; }
            public int Humidity { get; set; }
            public int WindSpeed { get; set; }
            public int EnergyConsumption { get; set; }
        }

        // Example usage with dummy data
        public async Task<string> GetFormattedWeatherData()
        {
            // Create a list of dummy WeatherEnergyData instances
            var weatherDataList = new List<WeatherEnergyData>
                {
                    new WeatherEnergyData { Date = "2024-10-01", Temperature = 15, Humidity = 70, WindSpeed = 10, EnergyConsumption = 200 },
                    new WeatherEnergyData { Date = "2024-10-02", Temperature = 18, Humidity = 65, WindSpeed = 12, EnergyConsumption = 210 },
                    new WeatherEnergyData { Date = "2024-10-03", Temperature = 20, Humidity = 60, WindSpeed = 8, EnergyConsumption = 220 }
                };

            // Pass the list to PrepareInputDataFromList to get the formatted string
            return await PrepareInputDataFromList(weatherDataList);
        }

    }
}
