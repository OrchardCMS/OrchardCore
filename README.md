Brochard
========

Orchard CMS running on ASP.Net VNext (DNX)

Getting Started
---------------

First off, follow the instructions here https://github.com/aspnet/home inorder to install DNVM. Next install Visual Studio 2015, or what ever you flavour of editor is. 

Dont forget to run 'dnvm upgrade -u' from the command line, and to make sure you are pointing at the aspnetvnext nuget url, not the master nuget url. (At the moment we are Beta5)

Next you want to clone the Repo. 'git clone https://github.com/OrchardCMS/Brochard.git' and checkout the master branch.

Load up the solution in Visual studio and do a compile.

Next navigate to 'D:\Brochard\src\OrchardVNext.Web' or whereever your retrospective folder is on the command line in Administrative mode.

run.. 'dnx . web' -> Hey you just kicked up the Orchard host.

Then in your browser, call the url... http://local.orchardvnext.test1.com/home/index

Note: 
You may need to add local.orchardvnext.test1.com to your host file.
