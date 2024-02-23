# Cypress e2e testing suite

Run `npm install` followed by `npm run test` to build and host Orchard and run execute the functional tests.

To see use the cypress user interface, run these scripts
```bash
npm run cms:clean // deletes App_Data
npm run cms:host // starts the app on port 5001
# run this in a separate console
npm run cms:cypress // Opens the cypress ui
```

**Note**: Tests will usually fail if not run from a clean state.

## Test data generator

To generate test data, run the `npm run gen:blog` script
