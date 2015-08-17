Brochard
========

Orchard CMS running on ASP.Net VNext (DNX)

Getting Started
---------------

First off, follow the instructions here https://github.com/aspnet/home in order to install DNVM. Next install Visual Studio 2015, or what ever you flavour of editor is.

Next you want to clone the Repo. 'git clone https://github.com/OrchardCMS/Brochard.git' and checkout the master branch.

Run the bootstrap.cmd file included in the repository to bootstrap dnx.

Load up the solution in Visual studio and do a build.

Next navigate to 'D:\Brochard\src\OrchardVNext.Web' or where ever your retrospective folder is on the command line in Administrative mode.

run.. 'dnx web' -> Hey you just kicked up the Orchard host.

Then in your browser, call the url... http://localhost:5001/setup/index
