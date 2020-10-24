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

    // skipping this test for now. Will improve it with better selectors in a seperate PR
    it.skip('Can add blog post and it displays in the frontend', function () {
        cy.login(tenant);
        cy.visitAdmin(tenant);
        cy.fixture('blog-posts').then(blogs => {
            blogs.forEach(blog => {
                cy.get('#adminMenu').find('.icon-class-fa-rss > .title').click();
                // create blog post button
                cy.get('.btn-success').contains('Create').click();

                cy.get('#TitlePart_Title').type(blog.name);

                cy.get('.simplemde-editor > .CodeMirror textarea').type(blog.text, { force: true });

                // click publish button
                cy.get('button.publish-button').click();

                cy.visitTenantPage(tenant, `blog/${blog.name}`);

                cy.get('h1').should('contain.text', blog.name);
                cy.get(':nth-child(3) > .row > .col-lg-8 > p').should('contain.text', blog.text);
            });
        });
    });
});
