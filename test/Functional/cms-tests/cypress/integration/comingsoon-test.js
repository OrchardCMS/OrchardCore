/// <reference types="Cypress" />
import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe('ComingSoon Recipe test', function () {
    let tenant;

    before(() => {
        tenant = generateTenantInfo("ComingSoon")
        cy.newTenant(tenant);
    })

    it('Displays the home page of the ComingSoon theme', function(){
        cy.visit(`${tenant.prefix}`);
        cy.get('h1').should('contain.text', 'Coming Soon');
        cy.get('p').should('contain.text', "We're working hard to finish the development of this site. Our target launch date is December 2021! Sign up for updates using the form below!");
    })

    it('ComingSoon admin login should work', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin`);
        cy.get('.ta-content').should('contain.text', 'Welcome to Orchard')
    })
});
