using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

class RecipeTester
{
    static Dictionary<string, Recipe> Recipes;

    static void LoadRecipes()
    {
        string jsonPath = Path.Combine("data", "recipes.json");
        string json = File.ReadAllText(jsonPath);
        Recipes = JsonSerializer.Deserialize<Dictionary<string, Recipe>>(json);
    }

    static void ShowSearchMenu(string searchTerm)
    {
        var matches = new List<string>();

        foreach (var key in Recipes.Keys)
        {
            if (Regex.IsMatch(key, Regex.Escape(searchTerm), RegexOptions.IgnoreCase))
            {
                matches.Add(key);
            }
        }

        if (matches.Count == 0)
        {
            Console.WriteLine("No matches found.");
            return;
        }

        Console.WriteLine("\n🔍 Matching Recipes:");
        for (int i = 0; i < matches.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {matches[i]}");
        }

        Console.Write("\nEnter the number of the recipe to view details: ");
        if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= matches.Count)
        {
            string selectedKey = matches[selection - 1];
            Recipe selected = Recipes[selectedKey];

            Console.WriteLine($"\n📦 {selectedKey}:");
            Console.WriteLine($"Output: {selected.output}");
            Console.WriteLine("Ingredients:");
            foreach (var pair in selected.ingredients)
            {
                Console.WriteLine($"- {pair.Key}: {pair.Value}");
            }
        }
        else
        {
            Console.WriteLine("Invalid selection.");
        }
    }

    public class Recipe
    {
        public int output { get; set; }
        public Dictionary<string, int> ingredients { get; set; }
    }
}
