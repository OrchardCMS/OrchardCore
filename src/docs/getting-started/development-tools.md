# Development Tools

What tools do we recommend to build your app with Orchard Core, or work on Orchard itself? In the end, this is up to your personal preference since as long as you can edit source files and build the app you can use any tool on any platform that .NET Core supports. Below are some tips to get you going for the general editing experience as well as for other useful tools.

## Editors and IDEs

- Visual Studio: The go-to IDE for .NET developers on Windows. Feature-rich and also has a free version. Download the latest Visual Studio (any edition) from <https://www.visualstudio.com/downloads/>.
  - Optionally install the [Lombiq Orchard Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=LombiqVisualStudioExtension.LombiqOrchardVisualStudioExtension) to add some useful utilities to your Visual Studio such as an error log watcher or a dependency injector.
  - Optionally install the [code snippets from the Orchard Dojo Library](https://orcharddojo.net/orchard-resources/CoreLibrary/Utilities/VisualStudioSnippets/) to quickly generate code in some common scenarios during module and theme development.
  - There are some further recommended extensions and other tips on using Visual Studio in the [Orchard Dojo Library](https://orcharddojo.net/orchard-resources/CoreLibrary/DevelopmentGuidelines/DevelopmentEnvironment).
- Visual Studio Code: Free and cross-platform editor that you can get from <https://code.visualstudio.com/>.
- JetBrains Rider: Feature-rich cross-platform IDE with a 30-day free trial that you can get from <https://www.jetbrains.com/rider/download/>.

## Utilities

- [DB Browser for SQLite](https://sqlitebrowser.org/) is a free and open-source tool to browse the SQLite database files created by Orchard. You can use it to open the `yessql.db` files under the tenant folders in `App_Data/Sites` and e.g. browse the tables, run queries, display the JSON documents in a nicely formatted way.
- [smtp4dev](https://github.com/rnwood/smtp4dev) is a small SMTP server that you can run locally to test sending emails. Just install it via `dotnet` and configure Orchard to use it as an SMTP server. It even features a web interface where you can browse processed emails.

Sure! Below is a well-structured **Markdown (.md)** documentation that provides detailed guidance on setting up an Orchard Core CMS environment using .NET CLI and various databases.

---

# **Orchard Core CMS Setup Guide**
A comprehensive guide to setting up **Orchard Core CMS** using the **.NET CLI** with different database options.

## **Table of Contents**
- [1. Prerequisites](#1-prerequisites)
- [2. Installing .NET SDK and CLI](#2-installing-net-sdk-and-cli)
- [3. Setting Up Orchard Core CMS](#3-setting-up-orchard-core-cms)
- [4. Choosing the Right Database](#4-choosing-the-right-database)
- [5. Running and Managing Orchard Core](#5-running-and-managing-orchard-core)
- [6. Deploying Orchard Core CMS](#6-deploying-orchard-core-cms)
- [7. Using .NET CLI Commands](#7-using-net-cli-commands)
- [8. Additional Features](#8-additional-features)

---

## **1. Prerequisites**
Before setting up **Orchard Core CMS**, ensure you have the following:

- **64-bit OS** (Windows, macOS, or Linux)
- **.NET SDK (Latest version)** – [Download Here](https://dotnet.microsoft.com/download)
- **Database Server** (SQLite, SQL Server, MySQL, or PostgreSQL)
- **Git (optional, for source code management)** – [Download Git](https://git-scm.com/)

If you are not familiar with .NET check out the [classes here](https://learn.microsoft.com/en-us/dotnet/fundamentals/)

Check if .NET is installed by running:
```sh
dotnet --version
```

---

## **2. Installing .NET SDK and CLI**
1. Download the latest .NET SDK from [Microsoft's official page](https://dotnet.microsoft.com/download).
2. Install it based on your operating system.
3. Verify the installation:
   ```sh
   dotnet --list-sdks
   ```

---

## **3. Setting Up Orchard Core CMS**
### **A. Installing Orchard Core CMS Templates**
Run the following command to install Orchard Core project templates:
```sh
dotnet new install OrchardCore.ProjectTemplates::2.1.5
```

### **B. Creating an Orchard Core CMS Project**
To create a new CMS project:
```sh
mkdir MyOrchardSite && cd MyOrchardSite
dotnet new occms
```

### **C. Running the Application**
Once inside your project directory, start the CMS:
```sh
dotnet run
```
Access the setup page at **http://localhost:5000**.


---

## **4. Choosing the Right Database**
Orchard Core supports multiple databases. Choose the one that fits your needs.

| Database | Best For | Pros | Cons |
|----------|---------|------|------|
| **SQL Server (MSSQL)** | Enterprise apps, Azure | High performance, enterprise-grade security | High resource usage, licensing required |
| **SQLite** | Small projects, local dev | Lightweight, no setup required | Not suitable for high traffic |
| **MySQL** | General web hosting | Open-source, widely supported | Requires more configuration for scaling |
| **PostgreSQL** | High-performance apps | ACID-compliant, scalable | Higher resource consumption |

### **Setting Up the Database**
- **SQLite (default)** – No configuration needed.
- **SQL Server (MSSQL)**:
  - Install **SQL Server Express** or **SQL Server Developer Edition**.
  - Configure connection strings in `appsettings.json`.
- **MySQL or PostgreSQL**:
  - Install the database server.
  - Create a new database and configure the connection string.

---

## **5. Running and Managing Orchard Core**
### **A. Accessing the Admin Dashboard**
In order to configure it and start writing content you can go to
```
 https://localhost:5001/admin
```
Use your admin credentials to log in.

### **B. Managing Content**
1. Navigate to **Content** → **New**.
2. Select **Page** or another content type.
3. Add content and **Publish**.

### **C. Customizing the Theme**
1. Go to **Design** → **Themes**.
2. Select a theme and click **Enable**.
3. Modify **.liquid** templates for custom UI.

### **D. Enabling Features**
1. Navigate to **Configuration** → **Features**.
2. Enable modules like:
   - **SEO**
   - **Media Library**
   - **Blogging**
   - **Forms**

---

## **6. Deploying Orchard Core CMS**
### **A. Publishing for Deployment**
To prepare the application for production:
```sh
dotnet publish -c Release -o ./publish
```
Upload the `/publish` folder to your server.

### **B. Hosting Options**
- **IIS (Windows Server)**
- **Azure App Services**
- **Docker (Use Orchard Core’s official images)**

---

## **7. Using .NET CLI Commands**
The .NET CLI is essential for managing your project.

### **A. Creating a New Project**
```sh
dotnet new mvc -o MyWebApp
cd MyWebApp
```

### **B. Running the Application**
```sh
dotnet run
```

### **C. Building the Project**
```sh
dotnet build
```

### **D. Publishing the Project**
```sh
dotnet publish -c Release -o ./publish
```

### **E. Managing Dependencies**
```sh
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet restore
dotnet list package
```

### **F. Running Tests**
```sh
dotnet test
```

### **G. Checking Installed SDK Versions**
```sh
dotnet --list-sdks
```

---

## **8. Additional Features**
### **A. Multi-Tenant Support**
1. Enable **Tenants** in **Configuration → Features**.
2. Navigate to **Configuration → Tenants**.
3. Click **Add Tenant** to create multiple sites.

### **B. Using Liquid Templates**
Customize UI using Liquid syntax:
```liquid
<h1>{{ Model.ContentItem.DisplayText }}</h1>
{{ Model.Content.Html | raw }}
```

More guides [here](https://docs.orchardcore.net/en/latest/guides/)