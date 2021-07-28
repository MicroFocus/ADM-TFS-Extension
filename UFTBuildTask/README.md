# Integration with UFT One
This extension enables you to run UFT One tests as part of your Azure DevOps build process. In a build step, run UFT One tests stored in the local file system or on an ALM server. When running tests from ALM Lab Management, you can also include a build step that prepares the test environment before running. After the build is complete, you can view comprehensive test results. 
#
# What's new in  Azure DevOps extension - version 2.0.0
##### Release date: July 2021
- Upload your tests results into Azure Portal containers and have them available after each individual build run
- View run metrics and failure details
- UI enhancements
- UFT One ALM Lab Management task improvement
- The "Failed Tests" report is generated as part of the run, without the need to add a *PublishTestResults* task (File System Run only)
- Bug fixes
#
#
#  Configuration
#### Prerequisites
- UFT One (version  >=**14.00**)
- Powershell (version **4** or **5.1** for extension version >=**2.0.0**)
- JRE installed
- Azure Powershell (for extension version >=**2.0.0**)

#### Setup
1. Install this extension for the relevant Azure DevOps organization
2. From our [GitHub][repository]: Browse a specific **release** (latest: 2.0.0)
3. From [Azure DevOps][azure-devops]: Have an agent set up (interactive or run as a service) 
4. On your agent machine:
4.1. Download the resources provided by a specific release (UFT.zip, unpack.ps1 and optionally the .vsix file)
4.2. Run the *unpack.ps1* script
###### For extension version >=**2.0.0**:
5. From [Azure Portal][azure-portal]: Have available a Resource Group, a Storage Account and a Container (for storing report artifacts)
6. On your agent machine:
6.1. Install [Azure Powershell] [azure-powershell]
6.2. Connect to [Azure Portal][azure-connect]
#
#
# Extension Functionality
##### UFT One File System Run
- Use this task to run tests located in your file system by specifying the tests' names, folders that contain tests, or an MTBX file (code sample below).
``` xml 
<Mtbx>
    <Test name="Test-Name-11" path="Test-Path-1">
    </Test>
    <Test name="Test-Name-2" path="Test-Path-2">
    </Test>
</Mtbx>
```
- More information is available [here][fs-docs]

##### UFT One ALM Run
- Use this task to run tests located on an ALM server, to which you can connect using SSO or a username and password.
- More information is available [here][alm-docs]

##### UFT One ALM Lab Management Run
- Use this task to run ALM server-side functional test setssuites.
- More information is available [here][alm-lab-docs]

##### UFT One ALM Lab Environment Preparation
- Use this task to assign values to AUT Environment Configurations located in ALM.
- More information is available [here][alm-env-docs]
#
#
# Additional Resources
For assistance or more information on configuring and using this extension, please consult the following resources:
- [GitHub repository][repository]
- [Help Center][docs]
- [UFT One Forum][forum]
- [Support][support]

[//]: # (References)
   [docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops.htm>
   [forum]:<https://community.microfocus.com/adtd/uft/f/sws-fun_test_sf/>
   [support]:<https://softwaresupport.softwaregrp.com/>
   [repository]:<https://github.com/MicroFocus/ADM-TFS-Extension/>
   [fs-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-local.htm>
   [alm-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm.htm>
   [alm-lab-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm-lm.htm#mt-item-1>
   [alm-env-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm-lm.htm#mt-item-0>
   [azure-devops]:<https://dev.azure.com/>
   [azure-portal]:<http://portal.azure.com/>
   [azure-powershell]:<https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-6.0.0>
   [azure-connect]:<https://docs.microsoft.com/en-us/powershell/module/az.accounts/connect-azaccount?view=azps-6.0.0>