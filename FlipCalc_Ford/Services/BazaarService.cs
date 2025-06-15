using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using FlipCalc_Ford.Models;

namespace FlipCalc_Ford.Services
{
    public class BazaarService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private const string ApiKey = "72f14ab8-62c7-4324-9bb7-adc63f458b5c";

        public BazaarService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<Dictionary<string, BazaarItem>> GetBazaarDataAsync()
        {

            var items = new Dictionary<string, BazaarItem>();
            try
            {
                string url = $"https://api.hypixel.net/skyblock/bazaar?key={ApiKey}";
                var response = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(response);

                if (!doc.RootElement.TryGetProperty("products", out var products))
                {
                    Console.WriteLine("Failed to parse 'products' from API response.");
                    return items;
                }

                foreach (var item in products.EnumerateObject())
                {
                    var id = item.Name;
                    var quickStatus = item.Value.GetProperty("quick_status");

                    double buyPrice = quickStatus.GetProperty("buyPrice").GetDouble();
                    double sellPrice = quickStatus.GetProperty("sellPrice").GetDouble();

                    items[id] = new BazaarItem(id, buyPrice, sellPrice);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching Bazaar data: {ex.Message}");
            }

            return items;
        }

        public bool TryGetBazaarPrice(Dictionary<string, BazaarItem> data, string itemName, out double buyPrice, out double sellPrice)
        {
            buyPrice = 0;
            sellPrice = 0;

            if (data.TryGetValue(itemName.ToUpper(), out BazaarItem item))
            {
                buyPrice = item.BuyPrice;
                sellPrice = item.SellPrice;
                return true;
            }

            return false;
        }
    }
}
