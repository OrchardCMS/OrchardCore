# Create and install a custom module

## Creating a custom module
Orchard Core offers a way to create a new CMS module. You can find a detailed guide in the official documentation: [Create a new CMS module](https://docs.orchardcore.net/en/latest/getting-started/templates/#create-a-new-cms-module). This guide assumes that a module already exists and focuses on how to install, customize, and enable it.
## Module customization
We can customize how your module appears in the Admin UI by editing the `Manifest.cs` file.

The manifest file allows you to define the following attributes:
- Name
- Description
- Author
- Version
- Category

After modifying the `Manifest.cs` file, for the changes to take effect, follow these steps:
1. Compile the solution
2. Run the application
3. If changes are not reflected, disable and re-enable the module

## Installing the module
To install a custom module in an Orchard Core CMS application, the module project must be referenced by the main CMS web project.

For example, in Visual Studio:
- Open the solution
- Right-click on **Dependencies** in the project named `Orchard Core.Cms.Web`
- Click on **Add Project Reference**
- Select the custom module project
- Click the Accept button

After adding the reference, the main CMS project file should include an entry similar to:

```
<ItemGroup>
<ProjectReference Include="..\MyModule.OrchardCore\MyModule.OrchardCore.csproj" />
</ItemGroup>
```
This reference allows Orchard Core to discover and load the custom module at runtime.

## Enabling the module
Once the module is installed, it can be enabled from the administration panel:
1. Go to the Admin dashboard
2. Navigate to **Tools > Features**
3. Search using the name defined in the `Name` property of the manifest file
4. Click the **Enable** button

If the module does not appear in the list, check the following:    
- The module must contain a manifest file (`Manifest.cs`)
- The CMS web project must reference the module project

