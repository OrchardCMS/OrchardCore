Cypress.Commands.add("login", function({ prefix = ""}={}) {
  const config = Cypress.config('orchard');
  cy.visit(`${prefix}/login`);
  cy.get("#UserName").type(config.username);
  cy.get("#Password").type(config.password);
  cy.get("#UserName").closest('form').submit();
});

Cypress.Commands.add("visitTenantSetupPage", ({ name }) => {
  cy.visit("/Admin/Tenants");
  cy.get(`#btn-setup-${name}`).click();
});

Cypress.Commands.add("siteSetup",  ({ name, setupRecipe }) => {
  const config = Cypress.config('orchard');
  cy.get("#SiteName").type(name);
  cy.get("body").then($body => {
    const elem = $body.find("#RecipeName");
    if (elem) {
      elem.val(setupRecipe);
    }
    const db = $body.find("#DatabaseProvider");
    if (db) {
      db.val("Sqlite");
    }
  });
  cy.get("#UserName").type(config.username);
  cy.get("#Email").type(config.email);
  cy.get("#Password").type(config.password);
  cy.get("#PasswordConfirmation").type(config.password);
  cy.get("#SubmitButton").click();
});
Cypress.Commands.add('newTenant', function(tenantInfo) {
  cy.login();
  cy.createTenant(tenantInfo);
  cy.visitTenantSetupPage(tenantInfo);
  cy.siteSetup(tenantInfo);
});

Cypress.Commands.add("createTenant", ({ name, prefix, setupRecipe, description }) => {
  // We create tenants on the SaaS tenant
  cy.visit("/Admin/Tenants");
  cy.btnCreateClick();
  cy.get("#Name").type(name, {force:true});
  cy.get("#Description").type(`Recipe: ${setupRecipe}. ${description || ''}`, {force:true});
  cy.get("#RequestUrlPrefix").type(prefix, {force:true});
  cy.get("#RecipeName").select(setupRecipe);
  cy.get("#DatabaseProvider").select('Sqlite')
  cy.btnCreateClick();
});
