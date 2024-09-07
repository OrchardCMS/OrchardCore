/// <reference types="Cypress" />

describe('MVC Tests', function () {
   
    it('should display "Hello World"', function(){
        cy.visit('/')
        cy.get('body').should('contain.text', 'Hello World');
    })
});
