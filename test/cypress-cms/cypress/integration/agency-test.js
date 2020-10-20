/// <reference types="Cypress" />

import { generateTenantInfo } from 'cypress-orchardcore/utils';

describe('Agency Tests', function () {
    let tenant;

    before(() => {
        tenant = generateTenantInfo("Agency")
        cy.newTenant(tenant);
    })

    it('Displays the home page of the Agency theme', function(){
        cy.visitTenantPage(tenant);
        cy.get('#services').should('contain.text', 'Lorem ipsum dolor sit amet consectetur');
    })

    it('Agency login should work', function(){
        cy.login(tenant);
        cy.visitAdmin(tenant);
        cy.get('.ta-content').should('contain.text', 'Welcome to Orchard')
    })


});
