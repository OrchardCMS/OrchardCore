# Creating a CMS Application

In this guide you will setup Orchard Core as a Content Management System.

## What you will need

- The current version of the .NET Core SDK. You can download it from here [https://www.microsoft.com/net/download/core](https://www.microsoft.com/net/download/core).

- A text editor and a terminal where you can type dotnet commands.

## Creating an Orchard Core Site

There are different ways to create sites and modules for Orchard Core. You can learn more about them [here](../../templates/README.md). In this guide we will use our "Code Generation Templates".

You can install the latest released templates using this command:

```dotnet new -i OrchardCore.ProjectTemplates::1.0.0-*```

!!! note
    To use the development branch of the template add `--nuget-source https://www.myget.org/F/orchardcore-preview/api/v3/index.json`

Create an empty folder that will contain your site. Open a terminal, navigate to that folder and run this:

```dotnet new occms -n MySite```

This creates a new ASP.NET MVC application in a new folder named `MySite`.

## Setting Up the Site

The application has been created by the template, but it has not been setup yet.

Orchard Core is modular. It means that depending on what modules you include in your application it can be many different things. Which modules are included is determined by the specific `Recipe` selected during setup. 
In order to build a site with all the features of a CMS with are going to use the `Blog` recipe.

Run the application by executing this command:

`dotnet run --project .\MySite\MySite.csproj`

!!! note
    If you are using the development branch of the templates, run `dotnet restore .\MySite\MySite.csproj --source https://www.myget.org/F/orchardcore-preview/api/v3/index.json` before running the application

Your application should now be running and contain the open ports:

```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

Open a browser on <https://localhost:5001>
It should display the setup screen.
Fill the form and select the `Blog` recipe.
For this exercise, you would want to use `Sqlite` as the database engine.

Submit the form. A few seconds later you should be looking at a Blog Site.
In order to configure it and start writing content you can go to <https://localhost:5001/admin>.


## Summary

You just created a full blown CMS site.
