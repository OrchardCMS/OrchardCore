/// <reference types="Cypress" />
import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe('Headless Recipe test', function () {
    let tenant;

    before(() => {
        tenant = generateTenantInfo("Headless")
        cy.newTenant(tenant);
    })

    it('Displays the login screen when accessing the root of the Headless theme', function(){
        cy.visit(`${tenant.prefix}`);
        cy.get('h1').should('contain.text', 'Log in');
     })

    it('Headless admin login should work', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin`);
        cy.get('.ta-content').should('contain.text', 'Welcome to Orchard')
    })
});
