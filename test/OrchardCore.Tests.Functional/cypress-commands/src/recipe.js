Cypress.Commands.add("runRecipe", ({ prefix }, recipeName) => {
  cy.visit(`${prefix}/Admin/Recipes`);
  cy.get(`#btn-run-${recipeName}`).click();
  cy.btnModalOkClick();
});

Cypress.Commands.add("uploadRecipeJson", ({ prefix }, fixturePath) => {
    cy.fixture(fixturePath).then((data) => {
      cy.visit(`${prefix}/Admin/DeploymentPlan/Import/Json`);
      cy.get('.CodeMirror').should('be.visible');
      cy.get("body").then($body => {
        $body.find(".CodeMirror")[0].CodeMirror.setValue(JSON.stringify(data));
      });
      cy.get('.ta-content > form').submit();
      // make sure the message-success alert is displayed
      cy.get('.message-success').should('contain', "Recipe imported");
    });
  });
