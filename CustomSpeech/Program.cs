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

            // Choose language
            Console.WriteLine("Wybierz język / Choose language:");
            Console.WriteLine("PL - Polski");
            Console.WriteLine("EN - English");

            string userInput = Console.ReadLine();

            if (userInput.Equals("PL", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Wybrałeś język polski. Witaj!");
                config.SpeechRecognitionLanguage = "pl-PL";
                mp3FilePath = "pl-voice.mp3"; // PL MP3
            }
            else if (userInput.Equals("EN", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("You have chosen English. Welcome!");
                config.SpeechRecognitionLanguage = "en-US";
                mp3FilePath = "en-voice.mp3"; // EN MP3
            }
            else
            {
                Console.WriteLine("Nieznany język / Unknown language. Spróbuj ponownie.");
                return;
            }

            try
            {
                // MP3 to WAV 
                ConvertMp3ToWav(mp3FilePath, wavFilePath);
                Console.WriteLine("Plik MP3 został pomyślnie przekonwertowany na WAV.");

                // WAV recognizer
                using var audioInput = AudioConfig.FromWavFileInput(wavFilePath);
                using var recognizer = new SpeechRecognizer(config, audioInput);

                Console.WriteLine("Rozpoczęcie rozpoznawania mowy...");
                var result = await recognizer.RecognizeOnceAsync();

                // Postprocessing
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
            // Check if ffmpeg is installed
            if (!File.Exists("/usr/bin/ffmpeg") && Environment.OSVersion.Platform == PlatformID.Unix)
            {
                throw new Exception("FFmpeg nie jest zainstalowany");
            }

            // FFMPEG Command
            var process = new Process();
            process.StartInfo.FileName = "ffmpeg";
            process.StartInfo.Arguments = $"-i \"{mp3FilePath}\" \"{wavFilePath}\" -y";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;


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
