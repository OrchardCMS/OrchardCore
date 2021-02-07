/// <reference types="Cypress" />

import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe('Agency Tests', function () {
    let tenant;

    before(() => {
        tenant = generateTenantInfo("Agency")
        cy.newTenant(tenant);
    })

    it('Displays the home page of the Agency theme', function(){
        cy.visit(`${tenant.prefix}`);
        cy.get('#services').should('contain.text', 'Lorem ipsum dolor sit amet consectetur');
    })

    it('Agency admin login should work', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin`);
        cy.get('.ta-content').should('contain.text', 'Welcome to Orchard')
    })


});
