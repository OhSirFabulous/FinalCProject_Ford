const fs = require('fs');
const readline = require('readline');
const path = require('path');

const recipes = JSON.parse(fs.readFileSync(path.join(__dirname, 'data', 'recipes.json'), 'utf-8'));

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

function searchRecipes(term) {
  const matches = Object.keys(recipes).filter(key => key.toLowerCase().includes(term.toLowerCase()));
  if (matches.length === 0) {
    console.log("No matches found.");
    rl.close();
    return;
  }
  console.log("\n🔍 Matching Recipes:");
  matches.forEach((match, index) => {
    console.log(`${index + 1}. ${match}`);
  });
  rl.question("\nEnter the number of the recipe to view details: ", input => {
    const index = parseInt(input) - 1;
    if (index >= 0 && index < matches.length) {
      const recipe = recipes[matches[index]];
      console.log(`\n📦 ${matches[index]}:`);
      console.log(`Output: ${recipe.output}`);
      console.log("Ingredients:");
      for (const [item, qty] of Object.entries(recipe.ingredients)) {
        console.log(`- ${item}: ${qty}`);
      }
    } else {
      console.log("Invalid selection.");
    }
    rl.close();
  });
}

// Example usage:
rl.question("Search for a recipe: ", answer => {
  searchRecipes(answer);
});
