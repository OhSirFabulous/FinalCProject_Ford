using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using FlipCalc_Ford.Models;
using FlipCalc_Ford.Services;
using FlipCalc_Ford.Utils;

namespace FlipCalc_Ford
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var calculator = new ProfitCalculator();
            var apiKey = "72f14ab8-62c7-4324-9bb7-adc63f458b5c";
            var bazaarService = new BazaarService(apiKey);
            var data = await bazaarService.GetBazaarDataAsync();

            while (true)
            {
                Console.Write("\nEnter command (type 'help' for options): ");
                string input = Console.ReadLine()?.Trim();
                if (string.IsNullOrWhiteSpace(input)) continue;

                var parts = input.Split(' ', 2);
                string command = parts[0].ToLower();
                string argument = parts.Length > 1 ? parts[1] : "";

                switch (command)
                {
                    case "help":
                        Console.WriteLine("\nAvailable commands:\n" +
                            "  help               - Show command list\n" +
                            "  top <count>        - List most profitable recipes\n" +
                            "  compare <item>     - Show NPC and Bazaar prices\n" +
                            "  npc                - Show profitable NPC flips\n" +
                            "  kuudra [quantity]  - Show Kuudra key costs and comparison\n" +
                            "  exit               - Quit the application");
                        break;

                    case "profit":
                        if (calculator.HasRecipe(argument))
                            calculator.ShowRecipeProfit(argument, data, bazaarService);
                        else
                            Console.WriteLine("Recipe not found.");
                        break;

                    case "top":
                        if (int.TryParse(argument, out int count))
                            calculator.ShowTopRecipes(count, data, bazaarService);
                        else
                            Console.WriteLine("Please provide a valid number.");
                        break;

                    case "compare":
                        calculator.CompareItem(argument, data, bazaarService);
                        break;

                    case "npc":
                        calculator.ShowProfitableNpcFlips(data, bazaarService);
                        break;

                    case "kuudra":
                        int qty = 1;
                        if (!string.IsNullOrWhiteSpace(argument) && int.TryParse(argument, out int parsed))
                            qty = parsed;

                        foreach (var tier in new[] { "BURNING", "FIERY", "INFERNAL" })
                            calculator.ShowKuudraKeyCosts(tier, qty, data, bazaarService);
                        break;

                    case "exit":
                        return;

                    default:
                        Console.WriteLine("Unknown command. Type 'help' for a list of commands.");
                        break;
                }
            }
        }
    }
}
