using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;

namespace P3
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            string subscriptionKey = ""; 
            string region = "westeurope";                    
            string outputFilePath = "transcript.txt";
            string wavFilePath = "converted.wav";             
            string mp3FilePath = "";                          

            var config = SpeechConfig.FromSubscription(subscriptionKey, region);

            // Wybór języka
            Console.WriteLine("Wybierz język / Choose language:");
            Console.WriteLine("PL - Polski");
            Console.WriteLine("EN - English");

            string userInput = Console.ReadLine();

            if (userInput.Equals("PL", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Wybrałeś język polski. Witaj!");
                config.SpeechRecognitionLanguage = "pl-PL";
                mp3FilePath = "pl-voice.mp3"; // Plik MP3 dla polskiego języka
            }
            else if (userInput.Equals("EN", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("You have chosen English. Welcome!");
                config.SpeechRecognitionLanguage = "en-US";
                mp3FilePath = "en-voice.mp3"; // Plik MP3 dla angielskiego języka
            }
            else
            {
                Console.WriteLine("Nieznany język / Unknown language. Spróbuj ponownie.");
                return;
            }

            try
            {
                // Konwersja MP3 na WAV za pomocą FFmpeg
                ConvertMp3ToWav(mp3FilePath, wavFilePath);
                Console.WriteLine("Plik MP3 został pomyślnie przekonwertowany na WAV.");

                // Rozpoznawanie mowy z pliku WAV
                using var audioInput = AudioConfig.FromWavFileInput(wavFilePath);
                using var recognizer = new SpeechRecognizer(config, audioInput);

                Console.WriteLine("Rozpoczęcie rozpoznawania mowy...");
                var result = await recognizer.RecognizeOnceAsync();

                // Przetwarzanie wyniku rozpoznania
                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"Rozpoznany tekst: {result.Text}");
                    await File.WriteAllTextAsync(outputFilePath, result.Text);
                    Console.WriteLine($"Transkrypcja została zapisana do pliku: {outputFilePath}");
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine("Nie rozpoznano mowy.");
                }
                else
                {
                    Console.WriteLine($"Wystąpił błąd: {result.Reason}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd: {ex.Message}");
            }
        }

        public static void ConvertMp3ToWav(string mp3FilePath, string wavFilePath)
        {
            // Sprawdzenie, czy FFmpeg jest zainstalowany
            if (!File.Exists("/usr/bin/ffmpeg") && Environment.OSVersion.Platform == PlatformID.Unix)
            {
                throw new Exception("FFmpeg nie jest zainstalowany");
            }

            // Budowanie komendy FFmpeg
            var process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = $"-i \"{mp3FilePath}\" \"{wavFilePath}\" -y";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Uruchomienie FFmpeg
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                throw new Exception($"Błąd podczas konwersji FFmpeg: {error}");
            }
        }
    }
}
