# Cypress OrchardCore

A collection of cypress commands for interacting with [OrchardCore CMS](https://github.com/OrchardCMS/OrchardCore).

## Installation

Install this package by running `npm install cypress-orchardcore`

## Setup

### Load the package in cypress support index file.

```javascript
// cypress/support/index.js
import 'cypress-orchardcore'
```

### Orchard admin credentials

First thing to do to use this package is to add the credentials to be used to interact with OrchardCore. These credentials must be added to the `cypress.json`  config file under the `orchard` key.

```json
{
  "baseUrl": "https://localhost:5001",
  "orchard": {
    "username":"admin",
    "email": "admin@orchard.com",
    "password": "Orchard1!"
  }
}
```

### Orchard default tenant 

This library assumes that you will be testing OrchardCore by leveraging the [Tenants](https://docs.orchardcore.net/en/latest/docs/glossary/#tenant) feature. You **must** create a test that runs first and sets up the Default tenant using the [Software as a service ](https://docs.orchardcore.net/en/dev/docs/getting-started/starter-recipes/#saas-recipe-with-thetheme) setup recipe.

To do so we suggest you create a test named `integration\000-setup-saas-site.js` with the following contents.

```javascript
/// <reference types="Cypress" />

const sassTenant = {
  name: "Testing SaaS",
  setupRecipe: "SaaS",
}

describe("Default tenant setup using SaaS recipe", function() {
  it("Setup default tenant", function() {
    cy.visit("/");
    cy.siteSetup(sassTenant);
    cy.login()
    // this is required to increase paging in the Tenants list page. Since we will be creating a lot of tenants during out testing.
    cy.setPageSize(sassTenant, "100");
  });
});
```


## Test Structure

Since we need to create tenants to test OrchardCore, it is recommended to use cypress's before() hook to create a tenant to use in your suite of tests. 
We recommend you use 1 tenant for each `describe()`

```javascript
/// <reference types="Cypress" />
import { generateTenantInfo } from 'cypress-orchardcore/dist/utils';

describe("VueForm Tests", function() {    
  let tenant;

  before(() => {
      // generate a tenant for all tests below
      tenant = generateTenantInfo("SetupRecipeName")
      cy.newTenant(tenant);
      cy.login(tenant);
      cy.uploadRecipeJson(tenant, "fixturefile.json");
  })

  it("DoesSomething", function() {
    cy.visitContentPage(tenant, "<contentitemid>");
    //...
  })

  it("DoesSomethingElse", function() {
    
    cy.visitContentPage(tenant, "<contentitemid>");
    //...

  })
});
```

## Available Commands

TODO
