/// <reference types="Cypress" />

import { generateTenantInfo } from 'cypress-orchardcore/utils';

describe('Blog Tests', function () {
    let tenant;

    before(() => {
        // generate the blog tenant
        tenant = generateTenantInfo("Blog")
        cy.newTenant(tenant);
    })

    it('Displays the home page of the blog recipe', function(){
        cy.visitTenantPage(tenant);
        cy.get('.subheading').should('contain.text', 'This is the description of your blog');
    })

    it('Blog login should work', function(){
        cy.login(tenant);
        cy.visitAdmin(tenant);
        cy.get('.ta-content').should('contain.text', 'Welcome to Orchard');
    })
});
