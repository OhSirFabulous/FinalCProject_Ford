using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FlipCalc_Ford.Models;
using FlipCalc_Ford.Services;

namespace FlipCalc_Ford.Utils
{
    public class ProfitCalculator
    {
        private readonly Dictionary<string, Recipe> _recipes;
        private readonly Dictionary<string, double> _npcPrices;

        private readonly Dictionary<string, (int redSand, int mycelium, int blaze, int coinCost)> _kuudraKeyData = new()
        {
            { "HOT", (2, 2, 0, 10000) },
            { "WARM", (4, 4, 0, 100000) },
            { "BURNING", (8, 8, 1, 500000) },
            { "FIERY", (16, 16, 2, 1000000) },
            { "INFERNAL", (32, 32, 4, 3000000) }
        };

        public ProfitCalculator()
        {
            _recipes = LoadRecipes("data/recipes.json") ?? new();
            _npcPrices = LoadNpcPrices("data/npc_prices.json") ?? new();
        }

        private Dictionary<string, Recipe>? LoadRecipes(string path)
        {
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<Dictionary<string, Recipe>>(json, options);
        }

        private Dictionary<string, double>? LoadNpcPrices(string path)
        {
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, double>>(json);
        }

        public bool HasRecipe(string name)
        {
            return _recipes.ContainsKey(name.ToUpper());
        }

        public void ShowRecipeProfit(string name, Dictionary<string, BazaarItem> data, BazaarService bazaarService)
        {
            name = name.ToUpper();
            if (!_recipes.TryGetValue(name, out var recipe))
            {
                Console.WriteLine("Recipe not found.");
                return;
            }

            double totalCost = 0;
            Console.WriteLine($"\n{name}:");
            Console.WriteLine("Ingredients:");

            foreach (var ingredient in recipe.Ingredients)
            {
                var item = ingredient.Key.ToUpper();
                var qty = ingredient.Value;
                double price = bazaarService.TryGetBazaarPrice(data, item, out var buy, out _) ? buy : _npcPrices.GetValueOrDefault(item, 0);
                totalCost += price * qty;
                Console.WriteLine($"- {item}: {qty} x {price:N0} = {price * qty:N0}");
            }

            double profit = bazaarService.TryGetBazaarPrice(data, name, out _, out var outputSell) ? outputSell - totalCost : -totalCost;
            Console.WriteLine($"Total Cost: {totalCost:N0}");
            Console.WriteLine($"Profit: {profit:N0}");
        }

        public void ShowTopRecipes(int count, Dictionary<string, BazaarItem> data, BazaarService bazaarService)
        {
            var profits = new List<(string name, double profit)>();

            foreach (var pair in _recipes)
            {
                string name = pair.Key;
                var recipe = pair.Value;

                double totalCost = 0;
                foreach (var ingredient in recipe.Ingredients)
                {
                    var item = ingredient.Key.ToUpper();
                    var qty = ingredient.Value;
                    double price = bazaarService.TryGetBazaarPrice(data, item, out var buy, out _) ? buy : _npcPrices.GetValueOrDefault(item, 0);
                    totalCost += price * qty;
                }

                if (bazaarService.TryGetBazaarPrice(data, name, out _, out var outputSell))
                {
                    profits.Add((name, outputSell - totalCost));
                }
            }

            var top = profits.OrderByDescending(p => p.profit).Take(count);
            Console.WriteLine("\nTop Recipes:");
            foreach (var entry in top)
                Console.WriteLine($"{entry.name}: {entry.profit:N0} coins profit");
        }

        public void CompareItem(string item, Dictionary<string, BazaarItem> data, BazaarService bazaarService)
        {
            item = item.ToUpper();
            bool found = bazaarService.TryGetBazaarPrice(data, item, out var bazaar, out var sell);
            double npc = _npcPrices.GetValueOrDefault(item, -1);

            Console.WriteLine($"\n{item} Comparison:");
            Console.WriteLine($"Bazaar Buy Price: {(found ? bazaar.ToString("N0") : "N/A")}");
            Console.WriteLine($"Bazaar Sell Price: {(found ? sell.ToString("N0") : "N/A")}");
            Console.WriteLine($"NPC Price: {(npc == -1 ? "N/A" : npc.ToString("N0"))}");
        }

        public void ShowProfitableNpcFlips(Dictionary<string, BazaarItem> data, BazaarService bazaarService)
{
    var flips = new List<(string item, double npc, double sell, double profit)>();

    foreach (var item in _npcPrices.Keys)
    {
        double npcPrice = _npcPrices[item];
        if (bazaarService.TryGetBazaarPrice(data, item, out var buy, out var sell))
        {
            double profit = npcPrice - sell;
            if (profit > 0)
                flips.Add((item, npcPrice, sell, profit));
        }
    }

    var sorted = flips.OrderByDescending(f => f.profit);
    Console.WriteLine("\nProfitable NPC Flips:");
    foreach (var f in sorted)
        Console.WriteLine($"{f.item}: Buy for {f.sell:N0}, sell to NPC for {f.npc:N0} - Profit: {f.profit:N0}");
}


        public void ShowKuudraKeyCosts(string tierInput, int quantity, Dictionary<string, BazaarItem> data, BazaarService bazaarService)
        {
            string tier = tierInput.ToUpper();
            if (!_kuudraKeyData.TryGetValue(tier, out var reqs))
            {
                Console.WriteLine("Invalid Kuudra key tier.");
                return;
            }

            int red = reqs.redSand * quantity;
            int myc = reqs.mycelium * quantity;
            int blaze = reqs.blaze * quantity;
            int coinCost = reqs.coinCost * quantity;

            double redPrice = bazaarService.TryGetBazaarPrice(data, "ENCHANTED_RED_SAND", out var redBuy, out _) ? redBuy : 0;
            double mycPrice = bazaarService.TryGetBazaarPrice(data, "ENCHANTED_MYCELIUM", out var mycBuy, out _) ? mycBuy : 0;
            double blazePrice = bazaarService.TryGetBazaarPrice(data, "CORRUPTED_NETHER_STAR", out var blazeBuy, out _) ? blazeBuy : 0;

            double cheapestSandOrMycTotal = Math.Min(redPrice * red, mycPrice * myc);
            double blazeTotal = blazePrice * blaze;

            double total = cheapestSandOrMycTotal + blazeTotal + coinCost;

            Console.WriteLine($"\n{tier} Kuudra Key x{quantity}:");
            Console.WriteLine($"- Cheapest of: {red} Enchanted Red Sand @ {redPrice:N0} = {redPrice * red:N0}");
            Console.WriteLine($"           or: {myc} Enchanted Mycelium @ {mycPrice:N0} = {mycPrice * myc:N0}");
            if (blaze > 0)
                Console.WriteLine($"- {blaze} Corrupt Nether Star @ {blazePrice:N0} = {blazeTotal:N0}");
            Console.WriteLine($"- {coinCost:N0} coins (Base)");
            Console.WriteLine($"Total Cost (using cheaper option): {total:N0}");
        }
    }
}
