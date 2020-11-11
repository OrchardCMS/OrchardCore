// https://stackoverflow.com/questions/105034/how-to-create-guid-uuid
const alphabet = "abcdefghijklmnopqrstuvwxyz";
function generateUniqueName() {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDay());
    return alphabet[Math.floor(Math.random() * alphabet.length)] + (date - today).toString(32);
}

export function generateTenantInfo(setupRecipeName, description) {
  var uniqueName = generateUniqueName();
  return {
      name: uniqueName,
      prefix: uniqueName,
      setupRecipe: setupRecipeName,
      description
  }
}
