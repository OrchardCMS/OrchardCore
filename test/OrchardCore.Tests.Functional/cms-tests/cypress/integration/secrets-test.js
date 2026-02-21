/// <reference types="Cypress" />
import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe('Secrets Module Tests', function () {
    let tenant;

    before(() => {
        tenant = generateTenantInfo("SecretsTest");
        cy.newTenant(tenant);
    });

    it('Should enable the Secrets feature', function () {
        cy.login(tenant);
        cy.enableFeature(tenant, 'OrchardCore.Secrets');
    });

    it('Should show Secrets menu item in admin', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin`);
        // Check for Secrets in the menu (under Security or Configuration depending on legacy mode)
        cy.get('#adminMenu').should('contain.text', 'Secrets');
    });

    it('Should display empty secrets list initially', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets`);
        cy.get('.alert-info').should('contain.text', 'Nothing here');
    });

    it('Should navigate to create secret page', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets`);
        cy.get('a.create').click();
        cy.url().should('include', '/Admin/Secrets/Create');
        cy.get('h1').should('contain.text', 'Create Secret');
    });

    it('Should create a new text secret', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets/Create`);

        // Fill in the form
        cy.get('#Name').type('TestApiKey');
        cy.get('#SecretType').select('TextSecret');
        cy.get('#SecretValue').type('my-secret-api-key-value');
        cy.get('#Description').type('Test API key for integration testing');

        // Submit
        cy.get('button.save').click();

        // Should redirect to index with success message
        cy.url().should('include', '/Admin/Secrets');
        cy.get('.message-success').should('be.visible');
    });

    it('Should display the created secret in the list', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets`);

        cy.get('.list-group-item').should('contain.text', 'TestApiKey');
        cy.get('.list-group-item').should('contain.text', 'TextSecret');
        cy.get('.list-group-item').should('contain.text', 'Database');
    });

    it('Should navigate to edit secret page', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets`);

        // Click on the secret name to edit
        cy.get('a').contains('TestApiKey').click();

        cy.url().should('include', '/Admin/Secrets/Edit');
        cy.get('h1').should('contain.text', 'Edit Secret');
        cy.get('#Name').should('have.value', 'TestApiKey');
        cy.get('#Name').should('have.attr', 'readonly');
    });

    it('Should update a secret value', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets/Edit/TestApiKey`);

        // Enter new value
        cy.get('#SecretValue').type('updated-secret-value');

        // Submit
        cy.get('button.save').click();

        // Should redirect to index with success message
        cy.url().should('include', '/Admin/Secrets');
        cy.get('.message-success').should('contain.text', 'updated');
    });

    it('Should create a second secret', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets/Create`);

        cy.get('#Name').type('DatabasePassword');
        cy.get('#SecretType').select('TextSecret');
        cy.get('#SecretValue').type('super-secure-password');

        cy.get('button.save').click();

        cy.url().should('include', '/Admin/Secrets');
        cy.get('.message-success').should('be.visible');
    });

    it('Should display multiple secrets in the list', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets`);

        cy.get('.list-group-item').should('have.length.at.least', 3); // Header + 2 secrets
        cy.get('.list-group-item').should('contain.text', 'TestApiKey');
        cy.get('.list-group-item').should('contain.text', 'DatabasePassword');
    });

    it('Should delete a secret', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets`);

        // Find and click delete for DatabasePassword
        cy.get('.list-group-item')
            .contains('DatabasePassword')
            .parents('.list-group-item')
            .find('a.btn-danger')
            .click();

        // Confirm deletion in the dialog
        cy.get('.modal-dialog button').contains('Yes').click();

        // Should show success message
        cy.get('.message-success').should('contain.text', 'deleted');

        // DatabasePassword should no longer be in the list
        cy.get('.list-group-item').should('not.contain.text', 'DatabasePassword');
    });

    it('Should prevent creating duplicate secret names', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets/Create`);

        // Try to create a secret with an existing name
        cy.get('#Name').type('TestApiKey');
        cy.get('#SecretType').select('TextSecret');
        cy.get('#SecretValue').type('duplicate-value');

        cy.get('button.save').click();

        // Should show validation error
        cy.get('.text-danger').should('contain.text', 'already exists');
    });

    it('Should require secret name', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets/Create`);

        // Leave name empty
        cy.get('#SecretType').select('TextSecret');
        cy.get('#SecretValue').type('some-value');

        cy.get('button.save').click();

        // Should show validation error
        cy.get('.field-validation-error, .text-danger').should('be.visible');
    });

    it('Should cancel and return to list', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin/Secrets/Create`);

        cy.get('a.cancel').click();

        cy.url().should('include', '/Admin/Secrets');
        cy.url().should('not.include', 'Create');
    });
});
