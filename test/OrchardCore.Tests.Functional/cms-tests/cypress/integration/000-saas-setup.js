/// <reference types="Cypress" />	

const sassSite = {	
    name: "Testing SaaS",	
    setupRecipe: "SaaS",	
  }	


describe('Setup SaaS', function () {	
    it('Successfully setup the SaaS default tenant', function () {	
        cy.visit('/');	
        cy.siteSetup(sassSite);	
        cy.login(sassSite);	
        cy.setPageSize(sassSite, "100");	
    });	
});
