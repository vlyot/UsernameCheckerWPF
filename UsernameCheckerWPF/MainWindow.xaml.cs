using System.Text;
using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Newtonsoft.Json;
using Microsoft.Win32;  // ✅ Use WPF file dialogs
using System.IO;
using Newtonsoft.Json.Linq;


namespace UsernameCheckerWPF
{
    public partial class MainWindow : Window
    {
        private static string GetApiKeyFromConfig()
        {
            string configPath = "C://Users//Admin//source//repos//UsernameCheckerWPF//UsernameCheckerWPF//config.json";
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                var jsonObj = JObject.Parse(json);
                string apiKey = jsonObj["OpenAiApiKey"]?.ToString();

                if (string.IsNullOrEmpty(apiKey) || !apiKey.StartsWith("sk-"))
                {
                    System.Windows.MessageBox.Show("Invalid OpenAI API Key! Please check config.json.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
                return apiKey;
            }
            System.Windows.MessageBox.Show("config.json file not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }


        private static readonly string OpenAiApiKey = GetApiKeyFromConfig();
        private List<string> inappropriateUsernames = new List<string>();
        private List<string> usernames = new List<string>();
        private string selectedFilePath = "";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                Title = "Select a Username File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                selectedFilePath = openFileDialog.FileName;
                SelectedFileText.Text = "Selected: " + selectedFilePath;
                usernames = ReadUsernamesFromFile(selectedFilePath);
                if (usernames.Count > 0)
                {
                    System.Windows.MessageBox.Show($"{usernames.Count} usernames loaded.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CheckUsernames.IsEnabled = true;  // ✅ Reference the actual button, not the method

                }
                else
                {
                    System.Windows.MessageBox.Show("No usernames found in the file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async void CheckUsernames_Click(object sender, RoutedEventArgs e)
        {
            ProgressBar.Visibility = Visibility.Visible;
            inappropriateUsernames.Clear();
            ResultsList.Items.Clear();

            await Task.Run(async () =>
            {
                foreach (string username in usernames)
                {
                    bool isInappropriate = await CheckUsernameWithAI(username);
                    if (isInappropriate)
                    {
                        // Update UI on the main thread
                        Dispatcher.Invoke(() =>
                        {
                            inappropriateUsernames.Add(username);
                            ResultsList.Items.Add(username);
                        });
                    }
                }
            });

            ProgressBar.Visibility = Visibility.Hidden;
            SaveResults.IsEnabled = inappropriateUsernames.Count > 0;

            System.Windows.MessageBox.Show($"Checking complete. Found {inappropriateUsernames.Count} inappropriate usernames.", "Results", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void SaveResults_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text File (*.txt)|*.txt|Excel File (*.xlsx)|*.xlsx",
                Title = "Save Inappropriate Usernames"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string savePath = saveFileDialog.FileName;

                if (savePath.EndsWith(".txt"))
                {
                    File.WriteAllLines(savePath, inappropriateUsernames);
                }
                else if (savePath.EndsWith(".xlsx"))
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Inappropriate Usernames");
                        worksheet.Cell(1, 1).Value = "Username";
                        for (int i = 0; i < inappropriateUsernames.Count; i++)
                        {
                            worksheet.Cell(i + 2, 1).Value = inappropriateUsernames[i];
                        }
                        workbook.SaveAs(savePath);
                    }
                }

                System.Windows.MessageBox.Show("Results saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private List<string> ReadUsernamesFromFile(string filePath)
        {
            if (filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    return worksheet.RowsUsed()
                        .Select(row => row.Cell(1).GetValue<string>().Trim())
                        .Where(username => !string.IsNullOrEmpty(username))
                        .ToList();
                }
            }
            else if (filePath.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return File.ReadAllLines(filePath)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrEmpty(line))
                    .ToList();
            }

            return new List<string>();
        }

        private bool ContainsObfuscatedProfanity(string username)
        {
            string[] profanePatterns = {
        "s[e3x@]x", "f[@a]ck", "sh[i1]t", "d[i1]ck", "b[i1]tch",
        "c[u4]m", "p[e3]nis", "p[o0]rn", "h[o0]rny", "n[i1]gga",
        "ass[h0o0]le", "f[a@]g", "b[o0]obs", "h[o0]tgrl", "sl[u4]t",
        "v[i1]agra", "x[x%]x", "s[e3]men", "s[l1]ut", "cr[a4]p",
        "d[i1]ld[o0]", "h[i1]tl[e3]r", "j[i1]zz", "w[i1]f[e3]b[e3]ater"
    };

            return profanePatterns.Any(pattern => Regex.IsMatch(username, pattern, RegexOptions.IgnoreCase));
        }

        private async Task<bool> CheckWithModerationAPI(string username)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {OpenAiApiKey}");

            var requestBody = new { input = username };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/moderations", content); // ✅ Fixed endpoint
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseBody);

                bool flagged = result?.results[0]?.flagged ?? false;
                return flagged;  // ✅ True = inappropriate
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error contacting OpenAI Moderation API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }


        private string DecodeLeetspeak(string username)
        {
            Dictionary<char, char> leetMap = new()
    {
        {'4', 'A'}, {'@', 'A'}, {'3', 'E'}, {'1', 'I'}, {'!', 'I'},
        {'0', 'O'}, {'5', 'S'}, {'7', 'T'}, {'$', 'S'}, {'8', 'B'}
    };

            return new string(username.ToUpper().Select(c => leetMap.ContainsKey(c) ? leetMap[c] : c).ToArray());
        }

        private async Task<bool> CheckUsernameWithAI(string username)
        {
            try
            {
                string decodedUsername = DecodeLeetspeak(username);

                if (ContainsObfuscatedProfanity(decodedUsername))
                {
                    return true;
                }

                if (await CheckWithModerationAPI(decodedUsername))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(OpenAiApiKey))
                {
                    System.Windows.MessageBox.Show("API Key is missing! Ensure it's correctly set in config.json.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                using HttpClient client = new();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {OpenAiApiKey}");  // ✅ Only Authorization header

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    temperature = 0.0,
                    max_tokens = 5,
                    messages = new[]
                    {
                new { role = "system", content = "You are an AI trained to detect offensive usernames. If a username is inappropriate, respond with 'yes'. Otherwise, respond with 'no'." },
                new { role = "user", content = $"Is '{decodedUsername}' inappropriate? Reply only with 'yes' or 'no'." }
            }
                };

                string jsonBody = JsonConvert.SerializeObject(requestBody);

                // ✅ Set Content-Type inside HttpContent, NOT in DefaultRequestHeaders
                HttpContent content = new StringContent(jsonBody, Encoding.UTF8);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    System.Windows.MessageBox.Show($"OpenAI API Error: {response.StatusCode} - {errorMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseBody);

                string reply = result?.choices[0]?.message?.content?.ToString().Trim().ToLower();
                return reply == "yes";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }



    }

}
