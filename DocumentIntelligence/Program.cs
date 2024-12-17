using System;
using System.IO;
using System.Text.RegularExpressions;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;

class Program
{
    static async Task Main(string[] args)
    {
        
        string key = "";
        string endpoint = "https://<>.cognitiveservices.azure.com/";


        string filePath = "";

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Console.WriteLine("Podana ścieżka jest nieprawidłowa lub plik nie istnieje.");
            return;
        }

        try
        {
            var client = new DocumentAnalysisClient(new Uri(endpoint), new AzureKeyCredential(key));


            using FileStream fileStream = new FileStream(filePath, FileMode.Open);


            AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", fileStream);
            AnalyzeResult result = operation.Value;

            Console.WriteLine("Analiza dokumentu faktury:");
            string fullText = ExtractFullText(result);
            Console.WriteLine(fullText);
            ExtractInvoiceDetails(fullText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd: {ex.Message}");
        }
    }

    static string ExtractFullText(AnalyzeResult result)
    {
        string fullText = "";
        foreach (DocumentPage page in result.Pages)
        {
            foreach (DocumentLine line in page.Lines)
            {
                fullText += line.Content + "\n";
            }
        }
        return fullText;
    }

    static void ExtractInvoiceDetails(string text)
    {
        Match nameMatch = Regex.Match(text, @"[A-Z][a-z]+ [A-Z][a-z]+");
        if (nameMatch.Success)
        {
            Console.WriteLine($"Imię i nazwisko: {nameMatch.Value}");
        }
        else
        {
            Console.WriteLine("Nie znaleziono imienia i nazwiska.");
        }

        Match totalMatch = Regex.Match(text, @"Total: \$([0-9,.]+)", RegexOptions.IgnoreCase);
        if (totalMatch.Success)
        {
            Console.WriteLine($"Łączna suma zapłaty: ${totalMatch.Groups[1].Value}");
        }
        else
        {
            Console.WriteLine("Nie znaleziono łącznej sumy zapłaty.");
        }
    }
}
