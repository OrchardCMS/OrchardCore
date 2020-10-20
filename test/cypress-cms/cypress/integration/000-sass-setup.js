/// <reference types="Cypress" />

describe('Setup SaaS', function () {
    it('Successfully setup the SaaS default tenant', function () {
        cy.visit('/');
        cy.setupSite({
            name: 'Orchard SaaS',
            setupRecipe: 'SaaS',
        });
    });
});
