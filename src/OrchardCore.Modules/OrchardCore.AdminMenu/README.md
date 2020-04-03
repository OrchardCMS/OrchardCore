# Admin Menu (OrchardCore.AdminMenu)

The Admin Menu module provides a way for users to create custom admin menus through the Admin UI.

## General Concepts

There are two basic concepts:

1. **Admin Menu**: A tree of Admin Nodes whose root is at the first level of the Admin Menu. There can be one or several of them.  

2. **Admin Node**: Each one of the nodes that form an Admin Menu. An AdminNode can contain other Admin Nodes. Each admin node results in one or more menu items rendered on TheAdmin menu.

These trees of menu items are merged with the standard admin menu that Orchard Core provides out of the box. In this document when we refer to that menu that is provided by Orchard Core out of the box we use the term **TheAdmin Menu**.

You can disable an Admin Menu and it won't be shown.

You can disable an Admin Node and neither it nor their descendents will be shown.


## How to create Admin Menu

1. Ensure the Admin Menu module is enabled.

2. Go to Configuration: Admin Menu.

3. Create a new Admin Menu, and start adding Admin Nodes to it. The Link Admin Node is the simplest type and it's perfect to test the feature.

4. As you keep adding Admin Nodes you will see them rendered automatically on TheAdmin Menu.


##  Provided Admin Node Types 

At the time of writing this document there are 3 Admin Node Types provided out of the box by Orchard Core:

1. **Link Admin Node**: It provides a simple menu item. The user can add a text, a url and a Font Awesome icon class so that the current admin theme can use them when rendering the menu item. At the moment TheAdmin theme is using that icon class only for first level menu items.
This link is the only one provided by the OrchardCore.AdminMenu module itself.

2. **Content Types Admin Node**: It provides a list of menu items containing a menu item for each content type. The links point to the Index action in the Contents Controller.
This node type is provided by the OrchardCore.Contents module.

3. **Lists Admin Node**: It provides menu items pointing to the Edit Page of those Content Items that include a part list on them.
For example, if you have a Blog Content Type, and several Blog Content Items, it will provide a link for each existing Blog.
This node type is provided by the OrchardCore.Lists module.

Note that each one of these nodes can have other nodes nested on it. The nesting is done through drag and drop on the UI.


## How are the Admin Menu rendered as admin menu items.

### How it works without the Admin Menu Module.
The Admin Menu that OrchardCore provides out of the box it's built broadly speaking like this:
1. NavigationManager retrieves all classes that implement INavigationProvider. There are many of them through many modules with the file name of "AdminMenu.cs".

2. On each AdminMenu the NavigationManager calls the BuildNavigationAsync method, passing a builder to it. The builder is the object where each AdminMenu can add their own menuItems.

3. Once all the AdminMenu classes finished adding their own menu items to the builder, the NavigationManager uses the info on the builder to "render" the full menu.

### What changes when the Admin Menu is Enabled.

1. The AdminMenu module declares it's own INavigationProvider and so it will be called too by NavigationManager. The name of that INavigationProvider is AdminMenuNavigationProvidersCoordinator.

2. The coordinator retrieves all AdminMenu stored on the database and for each one of them call a BuildTreeAsync method, where each node add recursively its own menu items to the builder.


## Deployment Plan Step and Recipe Step 
The module provides an Admin Menu Deployment Step. So an admin user can expend some time configuring a custom admin menu, add it to a deployment plan, export a json file, and use the generated json on a setup recipe. This way the sites that are built using that recipe will have the admin menu as the user prepared it.

## Permissions 
There are two kind of permissions associated with the module: 

1. Manage Admin Menus. It its about being able to create edit and delete admin menus from the admin.

2. View Admin Menus. It enables the possibility to show or hide an admin menu per role. You can do that from the standard Edit Roles page


## Developing Custom Admin Node Types

Any module can add it's own custom admin node types so that they can be used by users to build custom admin menus.

Commonly the steps that you follow in order to do that are:

1. Add a class that inherits from `AdminNode`. On this class add the specific properties that you want for your node type. This is the info that will go into the database.

2. Add a Driver to handle the display and edit of your admin node on the Admin. This won't handle the actual rendering of the admin menu. Drivers are only about the views required to create and edit the admin menu.

3. Optionally, you could implement a ViewModel to move info between the edit views and the driver.

4. Add a class that implements IAdminNodeNavigationBuilder. Its BuildNavigationAsync() method will be called by the AdminMenuNavigationProvidersCoordinator class when it is time to render the menu.

5. Create the views required to create and edit the admin nodes based on your node type.


By convention you should store all these non-view classes on a "AdminNodes" folder. This is optional.

By convention you have to store the views on a "Items" folder inside the "Views" folder. This is required.

Don't forget to register the corresponding classes on the Startup class.


### Code Snippets based on the LinkAdminNode

This is the LinkAdminNode.cs

```csharp

    public class LinkAdminNode : AdminNode
    {
        [Required]
        public string LinkText { get; set; }

        public string LinkUrl { get; set; }

        public string IconClass { get; set; } = "far fa-circle";
    }
```

This is how LinkAdminNodeBuilder builds a link. 

This class is  responsible for:

* Converting the admin node info in the database to menuItems and adding them to the global builder.

* Calling the same BuildNavigationAsync() method on each of their AdminNode's children.

This pattern ensures that at the end of the process the full tree will be processed.

```csharp
        public Task BuildNavigationAsync(MenuItem menuItem, 
                NavigationBuilder builder, 
                IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
        {
            // cast the received item to the concrete admin node type we are handling.
            var ltn = menuItem as LinkAdminNode;

            if ((ltn == null) ||( !ltn.Enabled))
            {
                return Task.CompletedTask;
            }

            // this is the standard Orchard Core way of adding menuItems to a builder 
            builder.Add(new LocalizedString(ltn.LinkText, ltn.LinkText), async itemBuilder => {

                // Add the actual link
                itemBuilder.Url(ltn.LinkUrl);
                AddIconPickerClassToLink(ltn.IconClass, itemBuilder);
                

                // Let children admin nodes build themselves inside this MenuItem
                foreach (var childTreeNode in menuItem.Items)
                {
                    try
                    {
                        var treeBuilder = treeNodeBuilders
                                .Where(x => x.Name == childTreeNode.GetType().Name)
                                .FirstOrDefault();
                        
                        await treeBuilder.BuildNavigationAsync(childTreeNode,itemBuilder,treeNodeBuilders);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, 
                            "An exception occurred while building the '{MenuItem}' child Menu Item.",
                            childTreeNode.GetType().Name);
                    }
                }
            });

            return Task.CompletedTask;
        }

```

## CREDITS

### Font Awesome Icon Picker
 
<https://farbelous.github.io/fontawesome-iconpicker/>

Originally written by (c) 2016 Javi Aguilar

Licensed under the MIT License
<https://github.com/farbelous/fontawesome-iconpicker/blob/master/LICENSE>


### jQuery UI Nested Sortable
 
v 2.1a / 2016-02-04
<https://github.com/ilikenwf/nestedSortable>

Depends on:
jquery.ui.sortable.js 1.10+

Copyright (c) 2010-2016 Manuele J Sarfatti and contributors
Licensed under the MIT License
<http://www.opensource.org/licenses/mit-license.php>
 