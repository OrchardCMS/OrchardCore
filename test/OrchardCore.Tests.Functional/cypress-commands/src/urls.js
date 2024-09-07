Cypress.Commands.add("visitContentPage", ({ prefix }, contentItemId) => {
  cy.visit(`${prefix}/Contents/ContentItems/${contentItemId}`);
});

