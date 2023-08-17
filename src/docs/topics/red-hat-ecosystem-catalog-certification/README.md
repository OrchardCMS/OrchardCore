# Managing the Orchard Core Red Hat Ecosystem Catalog certification

The [Red Hat Ecosystem Catalog](https://catalog.redhat.com/) is a catalog of software certified to work on the Red Hat platforms, like directly on Red Hat Enterprise Linux (RHEL). Orchard Core is cross-platform and thus works on RHEL as well. To showcase this, and to publicize Orchard Core better to the Red Hat community, we have certified Orchard and created its [Red Hat Ecosystem Catalog profile](https://catalog.redhat.com/software/applications/detail/223797).

## Renewing the certification

The certification is an assurance that Orchard Core works on RHEL. To keep this certification up-to-date, we need to rerun certification for each new Orchard release, as well as RHEL major release. This is not mandatory though, and it's good enough if we do this certification occasionally.

Note that Orchard has to be certified as a non-containerized app. Containerized app certification can't be done with [the Orchard Core Docker image](https://hub.docker.com/r/orchardproject/orchardcore-cms-linux), since Red Hat required [Podman](https://podman.io).

To begin, set up the basics on the [Certified Technology Portal](https://connect.redhat.com/account/dashboard). If you need access to it, please let us know. Then, these are the steps:

1. Create an Azure RHEL (latest major version) Azure VM. You can find this by searching for "Red Hat Enterprise Linux" in the Marketplace, and then setting up the official image provided by Red Hat. Be sure to keep HTTP and HTTPS ports open. (While you don't need an Azure VM, doing it this way makes things simpler than a local virtualized setup, for example.)
2. Connect with [MobaXterm](https://mobaxterm.mobatek.net/) or your terminal of choice, using the VM’s public IP, “azureuser” as the user, and the private key PEM file Azure lets you download when creating the VM.
3. Open up port 80:
    ```console
    sudo systemctl status firewalld
    sudo firewall-cmd --zone=public --add-port=80/tcp --permanent
    sudo firewall-cmd --reload
    ```
4. Open the full OC source that was released, and publish it. Use Release Configuration, self-contained, with the Target Runtime `linux-x64`. You can do this with right click, then "Publish" from Visual Studio as well. Upload the publish folder to the VM (what you can do from the file browser of MobaXterm).
5. Test running the app with:
    ```console
    sudo mkdir -m 777 App_Data
    sudo mkdir -m 777 wwwroot
    sudo chmod 777  ./OrchardCore.Cms.Web
    sudo ./OrchardCore.Cms.Web --urls http://*:80 --wwwroot
    ```
6. Open the IP of the VM in a web browser; you should see the setup screen and should be able to run the setup too.
7. Open a new SSH window while keeping Orchard running, and follow [the non-containerized application certification docs](https://access.redhat.com/documentation/en-us/red_hat_software_certification/8.61/html/red_hat_software_certification_workflow_guide/proc_certification-workflow-for-non-containerized-application_openshift-sw-cert-workflow-onboarding-certification-partners#certification_testing).
  - Run **all commands** prefixed with `sudo` since you can't use the `root` user.
  - If you get "Ignoring request to attach. It is disabled for org ... because of the content access mode setting." then Simple Content Access needs to be disabled under [the Red Hat Customer Portal's Overview page](https://access.redhat.com/management). Then you can try attaching again.
  - Open the output of `rhcert-cli save`. If it contains an "Error:  File system layout NOT in conformation with recommended standards" message, then check if it complains about the root partition size; if it does, then follow [the Azure docs](https://learn.microsoft.com/en-us/azure/virtual-machines/linux/expand-disks?tabs=rhellvm#increase-the-size-of-the-os-disk) to increase it to at least 10 GB.
8. Once the certification is done and confirmed by Red Hat, delete the VM and all the related resources, as well as the system under [the Systems page of the Red Hat Customer Portal](https://access.redhat.com/management/systems).