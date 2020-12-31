# Essentials Plugin Template (c) 2020

## License

Provided under MIT license

## Overview

Fork this repo when creating a new plugin for Essentials. For more information about plugins, refer to the Essentials Wiki [Plugins](https://github.com/PepperDash/Essentials/wiki/Plugins) article.

This repo contains example classes for the three main categories of devices:
* `EssentialsPluginTemplateDevice`: Used for most third party devices which require communication over a streaming mechanism such as a Com port, TCP/SSh/UDP socket, CEC, etc
* `EssentialsPluginTemplateLogicDevice`:  Used for devices that contain logic, but don't require any communication with third parties outside the program
* `EssentialsPluginTemplateCrestronDevice`:  Used for devices that represent a piece of Crestron hardware

There are matching factory classes for each of the three categories of devices.  The `EssentialsPluginTemplateConfigObject` should be used as a template and modified for any of the categories of device.  Same goes for the `EssentialsPluginTemplateBridgeJoinMap`.

This also illustrates how a plugin can contain multiple devices.

## Cloning Instructions

After forking this repository into your own GitHub space, you can create a new repository using this one as the template.  Then you must install the necessary dependencies as indicated below.

## Dependencies

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required. They referenced via nuget. You must have nuget.exe installed and in the `PATH` environment variable to use the following command. Nuget.exe is available at [nuget.org](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

### Installing Dependencies

To install dependencies once nuget.exe is installed, run the following command from the root directory of your repository:
`nuget install .\packages.config -OutputDirectory .\packages -excludeVersion`.
To verify that the packages installed correctly, open the plugin solution in your repo and make sure that all references are found, then try and build it.

### Installing Different versions of PepperDash Core

If you need a different version of PepperDash Core, use the command `nuget install .\packages.config -OutputDirectory .\packages -excludeVersion -Version {versionToGet}`. Omitting the `-Version` option will pull the version indicated in the packages.config file.

### Instructions for Renaming Solution and Files

See the Task List in Visual Studio for a guide on how to start using the templage.  There is extensive inline documentation and examples as well.

For renaming instructions in particular, see the XML `remarks` tags on class definitions
