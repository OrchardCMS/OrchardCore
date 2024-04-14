/// <reference types="Cypress" />

import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe('Migrations Tests', function () {
    let tenant;

    before(() => {
        // generate the migrations tenant
        tenant = generateTenantInfo("Migrations")
        cy.newTenant(tenant);
    })

    it('Displays the home page of the migrations recipe', function () {
        cy.visit(`${tenant.prefix}`);
        cy.get('.container').should('contain.text', 'Testing features having database migrations');
    })

    it('Migrations admin login should work', function () {
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin`);
        cy.get('.menu-admin').should('have.id', 'adminMenu')
    })
});
