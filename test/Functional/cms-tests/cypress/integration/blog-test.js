/// <reference types="Cypress" />

import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe('Blog Tests', function () {
    let tenant;

    before(() => {
        // generate the blog tenant
        tenant = generateTenantInfo("Blog")
        cy.newTenant(tenant);
    })

    it('Displays the home page of the blog recipe', function(){
        cy.visit(`${tenant.prefix}`);
        cy.get('.subheading').should('contain.text', 'This is the description of your blog');
    })

    it('Blog admin login should work', function(){
        cy.login(tenant);
        cy.visit(`${tenant.prefix}/Admin`);
        cy.get('.ta-content').should('contain.text', 'Welcome to Orchard');
    })
});
