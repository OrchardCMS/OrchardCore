# Cypress E2E Testing Suite

## Prerequisites

To get started, install the required packages by running:

```bash
npm install
```

## Available Commands

The following commands are defined in `package.json`:

- **Run CMS Tests**: `npm run cms:test` - Executes CMS-specific tests. *(This is usually the primary command to use)*
- **Run MVC Tests**: `npm run mvc:test` - Runs tests for MVC.
- **Run CMS Tests in UI**: `npm run cms:cypress` - Launches CMS tests in the Cypress interactive UI.
- **Run MVC Tests in UI**: `npm run mvc:cypress` - Launches MVC tests in the Cypress UI.
- **Build Orchard Core**: `npm run cms:build` - Builds or rebuilds the Orchard Core environment.
- **Host Orchard Core**: `npm run cms:host` - Starts the Orchard Core server.
- **Generate Blog Data**: `npm run gen:blog` - Generates sample data for the blog recipe.

## Creating a New CMS Test

### Adding a New Test Suite

For tests requiring a fresh tenant, create a new file in `./cms-tests/cypress/integration`. Tests in this folder execute in alphabetical order.

### Adding a Test to an Existing Suite

To add tests to an existing scenario, open any file in `./cms-tests/cypress/integration` and include the necessary assertions.

## Cypress Commands for Orchard Core

Each test scenario uses custom commands tailored for Orchard Core, such as `login` and `createTenant`. These commands are located in `./cypress-commands/src`.

To update these commands, use the `rollup` script to bundle them into the `./cypress-commands/dist` folder:

```bash
cd ./cypress-commands
npm run build
```

Both the `src` and `dist` folders should be included in source control.

## NPM Package

The `./cypress-commands` folder contains an npm package, [cypress-orchardcore](https://www.npmjs.com/package/cypress-orchardcore), published by [@jptissot](https://github.com/jptissot). This package allows other Orchard Core applications to leverage the testing framework.
