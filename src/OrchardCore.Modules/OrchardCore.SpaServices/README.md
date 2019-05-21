# Spa Services (`OrchardCore.SpaServices`)

## Purpose

Serve static content and render a single page application.

## Usage

Once you enable the module, a `spa` directory is created under the `app_data` folder and the server serves the files underneath. 
This is the place to put your application's files. In the Settings, you must set the Set Home Route options, in order to render the SPA. 
There are two options for the bootstrap of the app, one is to use a static file and the other to define a `Layout__SPA` template and use liquid syntax.

