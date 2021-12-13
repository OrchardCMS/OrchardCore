Cypress.Commands.add("enableFeature", ({ prefix }, featureName) => {
  cy.visit(`${prefix}/Admin/Features`);
  cy.get(`#btn-enable-${featureName}`).click();
});

Cypress.Commands.add("diableFeature", ({ prefix }, featureName) => {
  cy.visit(`${prefix}/Admin/Features`);
  cy.get(`#btn-diable-${featureName}`).click();
});

