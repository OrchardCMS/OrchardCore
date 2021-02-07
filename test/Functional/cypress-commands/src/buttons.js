Cypress.Commands.add('btnCreateClick', function() {
  cy.get('.btn.create').click();
});

Cypress.Commands.add('btnSaveClick', function() {
  cy.get('.btn.save').click();
});
Cypress.Commands.add('btnSaveContinueClick', function() {
  cy.get('.dropdown-item.save-continue').click();
});

Cypress.Commands.add('btnCancelClick', function() {
  cy.get('.btn.cancel').click();
});

Cypress.Commands.add('btnPublishClick', function() {
  cy.get('.btn.public').click();
});
Cypress.Commands.add('btnPublishContinueClick', function() {
  cy.get('.dropdown-item.publish-continue').click();
});

Cypress.Commands.add('btnModalOkClick', function() {
  cy.get("#modalOkButton").click();
});
