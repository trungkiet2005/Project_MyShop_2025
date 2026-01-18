using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Project_MyShop_2025.Core.Data;
using Project_MyShop_2025.Core.Models;

namespace Project_MyShop_2025.Views
{
    public sealed partial class ChatbotPage : Page
    {
        private readonly HttpClient _httpClient;
        private readonly List<ChatMessage> _chatHistory = new();
        private string _apiKey = "";
        private const string GEMINI_API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent";
        
        // Database context for querying shop data
        private ShopDbContext? _context;
        private string _shopDataContext = "";

        public ChatbotPage()
        {
            this.InitializeComponent();
            _httpClient = new HttpClient();
            
            // Load saved API key
            LoadApiKey();
            
            // Load shop data for AI context
            _ = LoadShopDataAsync();
        }
        
        private async Task LoadShopDataAsync()
        {
            try
            {
                var app = (App)Application.Current;
                using var scope = app.Services.CreateScope();
                _context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                
                _shopDataContext = await BuildShopDataContextAsync();
                System.Diagnostics.Debug.WriteLine($"Shop data context loaded: {_shopDataContext.Length} chars");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading shop data: {ex.Message}");
                _shopDataContext = "Không thể tải dữ liệu shop.";
            }
        }
        
        private async Task<string> BuildShopDataContextAsync()
        {
            var app = (App)Application.Current;
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
            
            var sb = new StringBuilder();
            sb.AppendLine("\n=== DỮ LIỆU SHOP HIỆN TẠI ===\n");
            
            // Thống kê tổng quan
            var totalProducts = await context.Products.CountAsync();
            var totalCategories = await context.Categories.CountAsync();
            var totalOrders = await context.Orders.CountAsync();
            var totalCustomers = await context.Customers.CountAsync();
            
            sb.AppendLine($"📊 THỐNG KÊ TỔNG QUAN:");
            sb.AppendLine($"- Tổng số sản phẩm: {totalProducts}");
            sb.AppendLine($"- Tổng số danh mục: {totalCategories}");
            sb.AppendLine($"- Tổng số đơn hàng: {totalOrders}");
            sb.AppendLine($"- Tổng số khách hàng: {totalCustomers}");
            
            // Doanh thu
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            
            var todayRevenue = await context.Orders
                .Where(o => o.CreatedAt.Date == today && o.Status == OrderStatus.Paid)
                .SumAsync(o => o.TotalPrice);
            
            var monthRevenue = await context.Orders
                .Where(o => o.CreatedAt >= startOfMonth && o.Status == OrderStatus.Paid)
                .SumAsync(o => o.TotalPrice);
            
            var totalRevenue = await context.Orders
                .Where(o => o.Status == OrderStatus.Paid)
                .SumAsync(o => o.TotalPrice);
            
            sb.AppendLine($"\n💰 DOANH THU:");
            sb.AppendLine($"- Hôm nay ({today:dd/MM/yyyy}): {todayRevenue:N0} VNĐ");
            sb.AppendLine($"- Tháng này: {monthRevenue:N0} VNĐ");
            sb.AppendLine($"- Tổng cộng: {totalRevenue:N0} VNĐ");
            
            // Đơn hàng theo trạng thái
            var ordersByStatus = await context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            
            sb.AppendLine($"\n📦 ĐƠN HÀNG THEO TRẠNG THÁI:");
            foreach (var item in ordersByStatus)
            {
                var statusName = item.Status switch
                {
                    OrderStatus.Created => "Mới tạo",
                    OrderStatus.Paid => "Đã thanh toán",
                    OrderStatus.Cancelled => "Đã hủy",
                    _ => item.Status.ToString()
                };
                sb.AppendLine($"- {statusName}: {item.Count} đơn");
            }
            
            // Danh mục sản phẩm
            var categories = await context.Categories
                .Include(c => c.Products)
                .ToListAsync();
            
            sb.AppendLine($"\n📁 DANH MỤC SẢN PHẨM:");
            foreach (var cat in categories)
            {
                sb.AppendLine($"- {cat.Name}: {cat.Products?.Count ?? 0} sản phẩm");
            }
            
            // Top 10 sản phẩm bán chạy
            var topProducts = await context.OrderItems
                .GroupBy(oi => oi.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(x => x.Quantity) })
                .OrderByDescending(x => x.TotalQty)
                .Take(10)
                .ToListAsync();
            
            if (topProducts.Any())
            {
                sb.AppendLine($"\n🏆 TOP 10 SẢN PHẨM BÁN CHẠY:");
                var productIds = topProducts.Select(p => p.ProductId).ToList();
                var products = await context.Products
                    .Where(p => productIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id, p => p.Name);
                
                int rank = 1;
                foreach (var item in topProducts)
                {
                    if (products.TryGetValue(item.ProductId, out var name))
                    {
                        sb.AppendLine($"{rank}. {name}: {item.TotalQty} đã bán");
                    }
                    rank++;
                }
            }
            
            // Sản phẩm sắp hết hàng (quantity < 10)
            var lowStockProducts = await context.Products
                .Where(p => p.Quantity < 10)
                .OrderBy(p => p.Quantity)
                .Take(10)
                .Select(p => new { p.Name, p.Quantity })
                .ToListAsync();
            
            if (lowStockProducts.Any())
            {
                sb.AppendLine($"\n⚠️ SẢN PHẨM SẮP HẾT HÀNG (< 10):");
                foreach (var p in lowStockProducts)
                {
                    sb.AppendLine($"- {p.Name}: còn {p.Quantity} sản phẩm");
                }
            }
            
            // Đơn hàng gần đây
            var recentOrders = await context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();
            
            if (recentOrders.Any())
            {
                sb.AppendLine($"\n🕐 5 ĐƠN HÀNG GẦN NHẤT:");
                foreach (var order in recentOrders)
                {
                    var statusName = order.Status switch
                    {
                        OrderStatus.Created => "Mới",
                        OrderStatus.Paid => "✅ Đã TT",
                        OrderStatus.Cancelled => "❌ Hủy",
                        _ => order.Status.ToString()
                    };
                    sb.AppendLine($"- #{order.Id} | {order.CreatedAt:dd/MM HH:mm} | {order.TotalPrice:N0}đ | {statusName} | {order.Items?.Count ?? 0} SP");
                }
            }
            
            // Khuyến mãi đang hoạt động
            var activePromotions = await context.Promotions
                .Where(p => p.IsActive && p.StartDate <= DateTime.Now && p.EndDate >= DateTime.Now)
                .ToListAsync();
            
            if (activePromotions.Any())
            {
                sb.AppendLine($"\n🎁 KHUYẾN MÃI ĐANG HOẠT ĐỘNG:");
                foreach (var promo in activePromotions)
                {
                    sb.AppendLine($"- {promo.Code}: {promo.Name} ({promo.DiscountValue}% - HSD: {promo.EndDate:dd/MM/yyyy})");
                }
            }
            
            return sb.ToString();
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
            var basePrompt = @"Bạn là trợ lý AI thông minh cho ứng dụng quản lý cửa hàng MyShop 2025.

🎯 NHIỆM VỤ CỦA BẠN:
- Trả lời các câu hỏi về dữ liệu shop (doanh thu, sản phẩm, đơn hàng, khách hàng)
- Phân tích kinh doanh và đưa ra gợi ý cải thiện
- Hỗ trợ quản lý sản phẩm (tồn kho, giá cả, danh mục)
- Hỗ trợ quản lý đơn hàng (theo dõi, xử lý)
- Tư vấn kinh doanh và marketing

📋 QUY TẮC:
1. Sử dụng dữ liệu shop thực tế được cung cấp bên dưới để trả lời
2. Trả lời ngắn gọn, chuyên nghiệp, dễ hiểu
3. Sử dụng emoji phù hợp để tăng tính trực quan
4. Luôn trả lời bằng tiếng Việt
5. Nếu không có dữ liệu, hãy nói rõ và đưa ra gợi ý chung
6. Định dạng số tiền: xxx,xxx VNĐ";

            // Append real shop data
            if (!string.IsNullOrEmpty(_shopDataContext))
            {
                return basePrompt + "\n" + _shopDataContext;
            }
            
            return basePrompt + "\n\n⚠️ Lưu ý: Chưa tải được dữ liệu shop. Vui lòng thử lại sau.";
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

        private async void RefreshData_Click(object sender, RoutedEventArgs e)
        {
            RefreshDataButton.IsEnabled = false;
            LoadingOverlay.Visibility = Visibility.Visible;
            
            try
            {
                await LoadShopDataAsync();
                AddMessageToUI("✅ Đã cập nhật dữ liệu shop mới nhất! Bạn có thể hỏi tôi về thông tin cửa hàng.", false);
            }
            catch (Exception ex)
            {
                AddMessageToUI($"❌ Lỗi khi cập nhật dữ liệu: {ex.Message}", false, true);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                RefreshDataButton.IsEnabled = true;
            }
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
