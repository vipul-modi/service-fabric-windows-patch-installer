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

### Deploying the patch


### Creating new patch package


## Guidelines for contributing patch packages

