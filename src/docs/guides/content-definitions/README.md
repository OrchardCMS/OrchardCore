# Understanding Content Definition Stores

`Content Definitions` are a record of the `Content Types`, `Content Parts`, and `Content Fields` used by a tenant.

By default the `Content Definitions` are stored in the database.

When the `File Content Definition` feature is enabled it stores content definitions in a `ContentDefinition.json` file 
at the root of each tenants `App_Data` folder, e.g. `App_Data/Sites/Default/ContentDefinition.json` for the default tenant.

The `File Content Definition` feature can be very useful during the `Development` phase of a project.

As you move your site to a `Production` phase you may wish to disable the feature and store the `Content Definitions` in the database.

To migrate your `ContentDefinition.json` file to the database use the following procedure:

## Step One - Create a Deployment Plan

- Go to the _Configuration -> Import/Export -> Deployment Plans_ menu
- Select _Add Deployment Plan_
- Name your Deployment Plan: 'Content Definitions'
- Select the 'Content Definitions' Deployment Plan
- Select _Add Step_
- Choose the _Update Content Definitions_ Step
- Check the _Include all content types and parts definitions._ checkbox
- Execute your Deployment Plan, and select the _File Download Target_

This will download a file called `ContentDefinitions.zip` to your computer.

## Step Two - Disable the File Content Definition Feature

- Go to the _Configuration -> Features_ menu
- Disable the Feature named `File Content Definition`

## Step Three - Import your Deployment Plan

- Go to the _Configuration -> Import/Export -> Import Package_ menu
- Choose the file you created in Step One, and select _Import_

## Summary

You just learnt how to create a `Deployment Plan` to migrate from the `File Content Definition` feature.

