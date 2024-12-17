using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.TextAnalytics;

namespace P4
{
    class Program
    {
        // Azure Document Intelligence configuration
        private static readonly string formRecognizerEndpoint = "https://<>.cognitiveservices.azure.com/";
        private static readonly string formRecognizerApiKey = "";

        // Azure Text Analytics configuration
        private static readonly string textAnalyticsEndpoint = "https://<>.cognitiveservices.azure.com/";
        private static readonly string textAnalyticsApiKey = "";

        static async Task Main(string[] args)
        {
            string pdfFilePath = "samplestory.pdf";

            if (!File.Exists(pdfFilePath))
            {
                Console.WriteLine("The specified PDF file does not exist.");
                return;
            }

            // Step 1: Extract text from PDF
            string extractedText = await ExtractTextFromPdfAsync(pdfFilePath);
            Console.WriteLine("Extracted Text:\n");
            Console.WriteLine(extractedText);

            // Step 2: Summarize the extracted text
            string summary = await SummarizeTextAsync(extractedText);
            Console.WriteLine("\nAbstractive Summarization Result:\n");
            Console.WriteLine(summary);
        }

        private static async Task<string> ExtractTextFromPdfAsync(string pdfFilePath)
        {
            var client = new DocumentAnalysisClient(new Uri(formRecognizerEndpoint), new AzureKeyCredential(formRecognizerApiKey));

            try
            {
                using var stream = File.OpenRead(pdfFilePath);

                // Analyze PDF using the Read model
                AnalyzeDocumentOperation operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", stream);
                AnalyzeResult result = operation.Value;

                StringBuilder extractedText = new StringBuilder();

                // Extract text line by line from all pages
                foreach (var page in result.Pages)
                {
                    foreach (var line in page.Lines)
                    {
                        extractedText.AppendLine(line.Content);
                    }
                }

                return extractedText.ToString();
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Azure Document Intelligence error: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return string.Empty;
            }
        }

        private static async Task<string> SummarizeTextAsync(string text)
        {
            var client = new TextAnalyticsClient(new Uri(textAnalyticsEndpoint), new AzureKeyCredential(textAnalyticsApiKey));

            try
            {
                List<string> documents = new() { text };

                // Perform abstractive summarization
                AbstractiveSummarizeOperation operation = await client.AbstractiveSummarizeAsync(WaitUntil.Completed, documents);

                StringBuilder summaryBuilder = new StringBuilder();

                await foreach (AbstractiveSummarizeResultCollection documentsInPage in operation.Value)
                {
                    foreach (AbstractiveSummarizeResult documentResult in documentsInPage)
                    {
                        if (documentResult.HasError)
                        {
                            Console.WriteLine($"Error: {documentResult.Error.Message}");
                            continue;
                        }

                        foreach (AbstractiveSummary summary in documentResult.Summaries)
                        {
                            summaryBuilder.AppendLine(summary.Text);
                        }
                    }
                }

                return summaryBuilder.ToString();
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Azure Text Analytics error: {ex.Message}");
                return "Summarization failed.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return "Summarization failed.";
            }
        }
    }
}
