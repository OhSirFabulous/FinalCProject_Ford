public class Recipe
{
    public int Output { get; set; } = 1;
    public Dictionary<string, int> Ingredients { get; set; } = new();
}
