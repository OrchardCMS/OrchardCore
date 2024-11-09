# Cypress e2e testing suite

## Prerequisistes

Install the required packages by running the following command:

```bash
npm install 
```

## Available commands

- `npm run cms:test` runs the CMS tests (usually the only command to run)
- `npm run mvc:test` runs the MVC tests
- `npm run cms:cypress` runs the cms tests in the cypress UI
- `npm run mvc:cypress` run the mvc tests in the cypress UI
- `npm run cms:build` build or rebuild Orchard Core
- `npm run cms:host` starts Orchard Core
- `npm run gen:blog` generates randomg data for the blog recipe

These commands are defined in `packages.json`

## Creating a new CMS test

### Adding a new set of tests

If the tests require a fresh tenant, create a new file in  `./cms-tests/cypres/integration`. The files in this folder are executed in alphabetical order.

### Adding a test to an existing scenario

Open any file from `./cms-tests/cypres/integration` and add some assertions.

## Cypress commands for Orchard Core

Each test scenarios file uses commands specific to Orchard Core like `loging`, `createTenant`, ...
They are defined in the `./cypress-commands/src` folder.

However to use them a `rollup` script aggregates them in the `.cypress-commands/dist` folder. To regenerate them use the `npm run build` from within the `./cypress-commands` folder.
Both these folders need to be checked in source control (`./src` and `./dist`).

## NPM package

The `./cypress-commands` contains an npm package that was [released on npmjs.com](https://www.npmjs.com/package/cypress-orchardcore) by [@jptissot](https://github.com/jptissot) with the intent of allowing other OC applications use the testing framework.
