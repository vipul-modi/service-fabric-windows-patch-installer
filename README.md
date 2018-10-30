---
services: service-fabric
platforms: dotnet, windows
author: vipulm-msft
---

# Service Fabric Windows Patch Installer Application
Service Fabric Windows Patch Installer is a sample application for installing custom patches or software on [Microsoft Azure Service Fabric](https://azure.microsoft.com/services/service-fabric/) clusters.

> Use [Service Fabric Patch Orchestration Service (POA)](https://docs.microsoft.com/en-us/azure/service-fabric/service-fabric-patch-orchestration-application) as a supported mechanism for patching machies in a Service Fabric cluster.

This application should be used for:
- Installing a patch not available through Service Fabric Patch Orchestration Service
- Getting out of a stuck situation or a live site issue
- Installing or verifying the installation of custom software on the machine

## Building and Deploying

[Setup your development environment with Visual Studio 2017](https://docs.microsoft.com/azure/service-fabric/service-fabric-get-started). Build the application by opening the solution file. To deploy on the local cluster, you can hit F5 to debug the sample. If you'd like to try publishing the sample to an Azure cluster:
Right-click on the application project in Solution Explorer and choose Publish. Sign in to the Microsoft account associated with your Azure subscription. Choose the cluster you'd like to deploy to. 

## About this application
The application installs the patch described in the data package of the PatchInstallerService. 

### Patch Installer Service
Patch installer service provides a common framework installing, verifying, and reporting on the patches. 

A patch is described as a data package for the patch installer service. The package consists of an installation script `install.cmd` and an optional verification script `verify.cmd` along with patch binaries. 

The installation and verification of the patches is controlled by settings. Reporting of missing patches is done by reporting custom health event on the node. The health reporting interval and health level can be also configured via settings.

### Creating new patch package
To create a patch package, follow the instructions below.

#### 1. Create patch data package
Create a data package that describes the patch.

- Open Visual Studio solution and go to `PackageRoot` folder of `PatchInstallerService`.

- Create a folder with the name of the patch (for example, see folder `EXAMPLE_PATCH`)

- Add a new data package in the `ServiceManifest.xml`. For example,
    ```
    <DataPackage Name="EXAMPLE_PATCH" Version="1.0.0" />

    ```
#### 2. Add installation and verification scripts
The patch package consists of an installation script `install.cmd` and an optional verification script `verify.cmd` along with patch binaries. 

- Add `install.cmd` file under the data package folder created above. The installation file must have all necessary steps for installing the patch including rebooting the machine if necessary. Add all binaries used by the installation script to the data package as well. 


- Add `verify.cmd` file under the data package folder created above. This file is required for patches not found using `Get-WmiObject -Class Win32_QuickFixEngineering | SELECT HotFixID,InstalledOn` PowerShell command. It file verifies if the patch is installed on the machine or not. If the patch is installed, the verification is successful and the script must return `0` exit code. The script must return a non-zero exit code if the verification fails or if the patch is not found.

If the patch can be found using `Get-WmiObject -Class Win32_QuickFixEngineering | SELECT HotFixID,InstalledOn` PowerShell command, then its name must match the `HotFixID` name returned by this command.

#### 3. Enable verification and installation of the patch
The  `PatchVerificationSettings` and `PatchInstallationSettings` controls the verification and installation of the specific patches. Add the name of the patch and using a boolean value of `true` or `false`, enable or disable the installation and verification. 

If a particular patch requires reboot of the machine, ensure that by default the installation is turned off. You can then enable the installation through an application upgrade (see `EXAMPLE_PATCH` for example) to ensure the avaiability of the deployed application and servies in the cluster.

## Guidelines for contributing patch packages

