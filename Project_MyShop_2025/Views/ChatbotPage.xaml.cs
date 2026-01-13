using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Project_MyShop_2025.Views
{
    public sealed partial class ChatbotPage : Page
    {
        private readonly HttpClient _httpClient;
        private readonly List<ChatMessage> _chatHistory = new();
        private string _apiKey = "";
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public ChatbotPage()
        {
            this.InitializeComponent();
            _httpClient = new HttpClient();
            
            // Load saved API key
            LoadApiKey();
        }

        private void LoadApiKey()
        {
            // Try to load from .env file first
            try
            {
                var envPath = System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".env");
                if (System.IO.File.Exists(envPath))
                {
                    var lines = System.IO.File.ReadAllLines(envPath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("GEMINI_API_KEY=") && !line.StartsWith("#"))
                        {
                            var key = line.Substring("GEMINI_API_KEY=".Length).Trim();
                            if (!string.IsNullOrEmpty(key) && key != "your_api_key_here")
                            {
                                _apiKey = key;
                                return;
                            }
                        }
                    }
                }
            }
            catch { /* Ignore .env file errors */ }

            // Fall back to local settings
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("GeminiApiKey"))
            {
                _apiKey = localSettings.Values["GeminiApiKey"] as string ?? "";
                if (ApiKeyBox != null)
                    ApiKeyBox.Password = _apiKey;
            }
        }

        private void SaveApiKey_Click(object sender, RoutedEventArgs e)
        {
            _apiKey = ApiKeyBox.Password?.Trim() ?? "";
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["GeminiApiKey"] = _apiKey;
            
            // Close flyout
            if (SettingsButton.Flyout is Flyout flyout)
                flyout.Hide();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // Flyout opens automatically
            ApiKeyBox.Password = _apiKey;
        }

        private async void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !IsShiftPressed())
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private bool IsShiftPressed()
        {
            var shiftState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift);
            return shiftState.HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        }

        private async void QuickPrompt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string prompt)
            {
                MessageInput.Text = prompt;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            var message = MessageInput.Text?.Trim();
            if (string.IsNullOrEmpty(message)) return;

            // Check API key
            if (string.IsNullOrEmpty(_apiKey))
            {
                await ShowError("Please set your Gemini API key in settings first.");
                return;
            }

            // Hide welcome card
            WelcomeCard.Visibility = Visibility.Collapsed;

            // Add user message to UI
            AddMessageToUI(message, true);
            MessageInput.Text = "";

            // Add to history
            _chatHistory.Add(new ChatMessage { Role = "user", Content = message });

            // Show loading
            LoadingOverlay.Visibility = Visibility.Visible;
            SendButton.IsEnabled = false;

            try
            {
                var response = await CallGeminiAPI(message);
                
                // Add AI response to UI
                AddMessageToUI(response, false);
                
                // Add to history
                _chatHistory.Add(new ChatMessage { Role = "model", Content = response });
            }
            catch (Exception ex)
            {
                AddMessageToUI($"Sorry, I encountered an error: {ex.Message}", false, true);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                SendButton.IsEnabled = true;
            }
        }

        private async Task<string> CallGeminiAPI(string message)
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = GetSystemPrompt() + "\n\nUser: " + message }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topK = 40,
                    topP = 0.95,
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{GEMINI_API_URL}?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonDocument.Parse(responseJson);
            
            var text = responseObj.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "Sorry, I couldn't generate a response.";
        }

        private string GetSystemPrompt()
        {
            return @"You are a helpful AI assistant for a shop management application called MyShop 2025. 
You help shop owners with:
- Product management (inventory, pricing, categories)
- Order management (creating, tracking, fulfilling orders)
- Sales reports and analytics
- Business advice and tips
- General questions about running a small business

Be friendly, professional, and concise. Use emojis sparingly to make responses engaging.
If asked about specific data, explain that you don't have access to the actual shop data but can provide general guidance.
Always respond in the same language the user uses.";
        }

        private void AddMessageToUI(string message, bool isUser, bool isError = false)
        {
            var messageContainer = new Grid { Margin = new Thickness(0, 4, 0, 4) };
            
            var messageBorder = new Border
            {
                CornerRadius = new CornerRadius(16, 16, isUser ? 4 : 16, isUser ? 16 : 4),
                Padding = new Thickness(16, 12, 16, 12),
                MaxWidth = 600,
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            if (isUser)
            {
                // User message - gradient background
                messageBorder.Background = new LinearGradientBrush
                {
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(1, 1),
                    GradientStops =
                    {
                        new GradientStop { Color = GetColorFromHex("#8B5CF6"), Offset = 0 },
                        new GradientStop { Color = GetColorFromHex("#EC4899"), Offset = 1 }
                    }
                };
            }
            else if (isError)
            {
                messageBorder.Background = new SolidColorBrush(GetColorFromHex("#FEE2E2"));
            }
            else
            {
                // AI message - white background
                messageBorder.Background = new SolidColorBrush(Microsoft.UI.Colors.White);
                messageBorder.BorderBrush = new SolidColorBrush(GetColorFromHex("#E2E8F0"));
                messageBorder.BorderThickness = new Thickness(1);
            }

            var messageText = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new SolidColorBrush(isUser ? Microsoft.UI.Colors.White : 
                    (isError ? GetColorFromHex("#DC2626") : GetColorFromHex("#0F172A")))
            };

            messageBorder.Child = messageText;
            messageContainer.Children.Add(messageBorder);
            MessagesPanel.Children.Add(messageContainer);

            // Scroll to bottom
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ChangeView(null, ChatScrollViewer.ScrollableHeight, null);
        }

        private void ClearChat_Click(object sender, RoutedEventArgs e)
        {
            _chatHistory.Clear();
            MessagesPanel.Children.Clear();
            
            // Show welcome card again
            MessagesPanel.Children.Add(WelcomeCard);
            WelcomeCard.Visibility = Visibility.Visible;
        }

        private async Task ShowError(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private static Windows.UI.Color GetColorFromHex(string hex)
        {
            hex = hex.Replace("#", "");
            byte a = 255;
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }
    }

    public class ChatMessage
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }
}
