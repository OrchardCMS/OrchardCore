'use strict';

Object.defineProperty(exports, '__esModule', { value: true });

// https://stackoverflow.com/questions/105034/how-to-create-guid-uuid

function generateUniqueName() {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    return 't' + (date - today).toString(32);
}

function generateTenantInfo(setupRecipeName, description) {
  var uniqueName = generateUniqueName();
  return {
      name: uniqueName,
      prefix: uniqueName,
      setupRecipe: setupRecipeName,
      description
  }
}

exports.generateTenantInfo = generateTenantInfo;
