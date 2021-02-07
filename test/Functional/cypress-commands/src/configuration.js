Cypress.Commands.add("setPageSize", ({prefix = ""}, size) => {
  cy.visit(`${prefix}/Admin/Settings/general`);
  cy.get('#ISite_PageSize')
    .clear()
    .type(size);
  cy.btnSaveClick();
  // wait until the success message is displayed
  cy.get('.message-success');
});
