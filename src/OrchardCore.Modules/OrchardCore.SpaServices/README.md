# Spa Services (`OrchardCore.SpaServices`)

## Purpose

Serve static content and render a single page application for headless CMS mode.

## Usage

Once you enable the module, a `spa` directory is created under the site's folder from which the server serves the static files. 
This is the place to put your application's files. If SPA is not your homepage, you must enable it in the Settings under `Single Page Application` 
section in order to render the app. There are two options for bootstraping, one is to use a static file and the other to define a 
`Layout__SPA` template with liquid syntax support.

