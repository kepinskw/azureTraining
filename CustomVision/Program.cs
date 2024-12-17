﻿using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.Images;
using System.Text;
using System.Text.Json;
using System;
using System.IO;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


internal class Program
{
    static void Main(string[] args)
    {
        //  API OpenAI 
        string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("Brak klucza API. Ustaw zmienną środowiskową OPENAI_API_KEY.");
            return;
        }

        // DALLE  Init
        ImageClient client = new("dall-e-3", apiKey);

        string prompt = "Samolot";

        ImageGenerationOptions options = new()
        {
            Quality = GeneratedImageQuality.Standard,
            Size = GeneratedImageSize.W1024xH1792,
            Style = GeneratedImageStyle.Natural,
            ResponseFormat = GeneratedImageFormat.Bytes
        };

        string fileName = $"{Guid.NewGuid()}.png";

        try
        {

            GeneratedImage image = client.GenerateImage(prompt, options);
            BinaryData bytes = image.ImageBytes;


            using FileStream stream = File.OpenWrite(fileName);
            bytes.ToStream().CopyTo(stream);

            Console.WriteLine($"Obraz został zapisany jako {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd podczas generowania obrazu: {ex.Message}");
            return;
        }


        string endpoint = "https://<>>.cognitiveservices.azure.com/";
        string predictionKey = Environment.GetEnvironmentVariable("CUSTOM_VISION_PREDICTION_KEY"); 
        if (string.IsNullOrEmpty(predictionKey))
        {
            Console.WriteLine("Brak klucza API Custom Vision. Ustaw zmienną środowiskową CUSTOM_VISION_PREDICTION_KEY.");
            return;
        }

        Guid projectId;
        try
        {
            projectId = new Guid("<>"); // Zastąp prawidłowym GUID projektu
        }
        catch
        {
            Console.WriteLine("Nieprawidłowy ID projektu.");
            return;
        }

        string iterationName = "Iteration1";
        byte[] imageBytes;
        if (!File.Exists(fileName))
        {
            Console.WriteLine("Image file not found. Please check the path.");
            return;
        }

        try
        {
            imageBytes = File.ReadAllBytes(fileName);
            Console.WriteLine($"Image successfully loaded from disk. Size: {imageBytes.Length} bytes.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while loading image from disk: {ex.Message}");
            return;
        }


        CustomVisionPredictionClient visionPredictionClient = new CustomVisionPredictionClient(new ApiKeyServiceClientCredentials(predictionKey))
        {
            Endpoint = endpoint
        };

        using (Stream imageStream = new MemoryStream(imageBytes))
        {
            // Perform the image classification 
            var result = visionPredictionClient.ClassifyImage(projectId, iterationName, imageStream); 
            if (result == null)
                {
                    Console.WriteLine("Error: The prediction result is null.");
                    return;
                }

                if (result.Predictions == null || result.Predictions.Count == 0)
                {
                    Console.WriteLine("No predictions were returned by the model.");
                    return;
                }

                Console.WriteLine("Predictions:");
                foreach (var prediction in result.Predictions)
                {
                    Console.WriteLine($"  Tag: {prediction.TagName}");
                    Console.WriteLine($"  Probability: {prediction.Probability:P1}");
        //         }
                
            MainAsync(args).GetAwaiter().GetResult();

        }


        
    
    static async Task MainAsync(string[] args)
    {
        
        string subscriptionKey = ""; 
        string endpoint = "https://<>.cognitiveservices.azure.com/"; 
        string inputImagePath = ""; 
        string outputImagePath = ""; 

        if (!File.Exists(inputImagePath))
        {
            Console.WriteLine("Obraz wejściowy nie istnieje. Sprawdź ścieżkę.");
            return;
        }

        try
        {
            
            byte[] imageBytes = File.ReadAllBytes(inputImagePath);
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            
            string uri = $"{endpoint}computervision/imageanalysis:segment?api-version=2023-02-01-preview&mode=backgroundRemoval";

            using var content = new ByteArrayContent(await File.ReadAllBytesAsync(inputImagePath));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var response = await client.PostAsync(uri, content);
            
            if (response.IsSuccessStatusCode)
            {
                var outputImage = await response.Content.ReadAsByteArrayAsync();
                await File.WriteAllBytesAsync(outputImagePath, outputImage);
                Console.WriteLine($"The background of the image has been successfully.");
            }
            else
            {
                Console.WriteLine($"Error removing background: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd: {ex.Message}");
        }
    }
}
