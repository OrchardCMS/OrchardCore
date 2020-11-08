'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

// https://stackoverflow.com/questions/105034/how-to-create-guid-uuid
function uuidv4() {
  return ([1e7]+1e3+4e3+8e3+1e11).replace(/[018]/g, c =>
    (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
  );
}

function generateTenantInfo(setupRecipeName, description) {
  var uniqueName = uuidv4();
  return {
      name: uniqueName,
      prefix: uniqueName,
      setupRecipe: setupRecipeName,
      description
  }
}

exports.generateTenantInfo = generateTenantInfo;
