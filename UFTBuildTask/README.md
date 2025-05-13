# Integration with OpenText™ Functional Testing
This extension enables you to run Functional Testing tests as part of your Azure DevOps build process. In a build step, run Functional Testing tests stored in the local file system or on an ALM server.    
• When running test stored in the local file system, you can also run mobile tests. In addition, Parallel execution is available for mobile and web tests.    
• When running tests from ALM Lab Management, you can also include a build step that prepares the test environment before running the tests.    
After the build is complete, you can view comprehensive test results. 

# What's new in OpenText™ Functional Testing Azure DevOps extension - version 25.2.1
##### Release date: May 2025
- Restored the missing Extensions tab
- Product name updated in additional locations

# What's new in OpenText™ Functional Testing Azure DevOps extension - version 25.2
##### Release date: April 2025
- Rebranding the OpenText™ family products
  - Digital Lab is now OpenText™ Functional Testing Lab for Mobile and Web
  - UFT One is now OpenText™ Functional Testing
  - Application Lifecycle Management is now OpenText™ Application Quality Management
  - LoadRunner Enterprise is now OpenText™ Enterprise Performance Engineering
- [FS] Generate JUnit report option
- [FS] Normalize test paths to prevent exceeding the maximum length
- [ALM Lab Mgmt] Improve ALM Lab Management task to correctly handle the Cancel pipeline action, and stop the test execution
- Bug fixing, code enhancements and optimizations

# What's new in UFT One Azure DevOps extension - version 24.2
##### Release date: April 2024
- Update ADO extension to use FTToolsLauncher instead of HpToolsLauncher
- [FS] Support testing on Digital Lab Cloud Browsers
- [FS] Enable using the open UFT One visible instance
- [FS] In the UI, highlight the required Digital Lab fields
- [DL] Add proxy fields to Get Digital Lab Resource Task
- [FS][Parallel][DL] Remove the unused Tenant ID field / control and validation logic
- [Parallel] Print all arguments in the console output when calling ParallelRunner.exe
- Bug-fixing, code enhancements and optimizations, etc...

# What's new in UFT One Azure DevOps extension - version 23.4
##### Release date: October 2023
This version includes the following minor changes:
- Company Name rebranding: Micro Focus is now OpenText
- Digital Lab renaming: UFT Mobile / Mobile Center is now Digital Lab
- Add/Update Copyright and License Terms

# What's new in UFT One Azure DevOps extension - version 5.*
##### Release date: April 2023
This plugin update includes the following:

**New features**
- You can customize the format, or pattern, of the timestamp used for reporting in the Extensions tab.
- When running a test saved on your local machine, you can now instruct Azure DevOps to stop the entire test run as soon as one test fails.
- When running tests in parallel, you can connect to UFT Mobile / Digital Lab using access keys for authentication.
- If you abort a UFT One File System Run or UFT One Parallel Test Run job in the middle of a test run, the results of the completed tests are saved in the local build folder.

**Bug fixes / improvements**
- [Parallel] Fixed error message output issues when using seat licenses to run parallel web tests.
- [FS] Fixed the error that occurred when running duplicate tests.
- [FS] Error messages displayed when you run local tests from .mtbx files, are now clearly defined.

# What's new in UFT One Azure DevOps extension - version 4.*
##### Release date: September 2022
This plugin update includes the following:
- Run your GUI Mobile tests from the file system.
  You can directly define Digital Lab connection settings and Mobile Record and Run settings in the pipelines without defining them separately in UFT One.
- Use a Get Digital Lab Resources task to retrieve application and device information from Digital Lab.
- Use a UFT One Parallel Test Run task to run GUI Web and GUI Mobile tests in parallel.

#  Configuration
#### Prerequisites
- UFT One (version  >=**14.00**)
- Powershell (version >=**5.1**)
- JRE installed (required only if use the "OpenText Functional Testing ALM Lab Environment Preparation" task)
- Azure Powershell (for extension version >=**2.0.0**)

#### Setup
1. Install this extension for the relevant Azure DevOps organization
2. From our [GitHub][repository]: Browse a specific **release** (latest: **25.2.1**)
3. From [Azure DevOps][azure-devops]: Have an agent set up (interactive or run as a service) 
4. On your agent machine:    
4.1. Download the resources provided by a specific release (UFT.zip, unpack.ps1 and optionally the .vsix file)    
4.2. Run the *unpack.ps1* script
###### For extension version >=**2.0.0**:
5. From [Azure Portal][azure-portal]: Have available a Resource Group, a Storage Account and a Container (for storing report artifacts)
6. On your agent machine:    
6.1. Install [Azure Powershell] [azure-powershell]    
6.2. Connect to [Azure Portal][azure-connect]

# Extension Functionality
##### OpenText Functional Testing File System Run
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

##### OpenText Functional Testing ALM Run
- Use this task to run tests located on an ALM server, to which you can connect using SSO or a username and password.
- More information is available [here][alm-docs]

##### OpenText Functional Testing ALM Lab Management Run
- Use this task to run ALM server-side functional test-sets or build-verification-suites.
- More information is available [here][alm-lab-docs]

##### OpenText Functional Testing ALM Lab Management Stop Run
- Use this task to stop ALM server-side functional test-sets or build-verification-suites.
- More information is available [here][stop-alm-lab-docs]

##### OpenText Functional Testing ALM Lab Environment Preparation
- Use this task to assign values to AUT Environment Configurations located in ALM.
- More information is available [here][alm-env-docs]

##### OpenText Functional Testing Parallel Test Run
- Use this task to  to trigger a parallel testing task to run GUI Web or GUI Mobile tests in parallel from Azure DevOps Server (formerly known as TFS).
- More information is available [here][parallel-docs]

##### Get OpenText Functional Testing Lab Resources
- Use this task to configure a task that retrieves device and application information from Functional Testing Lab (Digital Lab).
- More information is available [here][get-digital-lab-resources]

# Additional Resources
For assistance or more information on configuring and using this extension, please consult the following resources:
- [GitHub repository][repository]
- [Help Center][docs]
- [Functional Testing Forum][forum]
- [Support][support]

[//]: # (References)
   [docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops.htm>
   [forum]:<https://community.microfocus.com/adtd/uft/f/sws-fun_test_sf/>
   [support]:<https://softwaresupport.softwaregrp.com/>
   [repository]:<https://github.com/MicroFocus/ADM-TFS-Extension/>
   [fs-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-local.htm>
   [alm-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm.htm>
   [alm-lab-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm-lm.htm#mt-item-1>
   [stop-alm-lab-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Default.htm#cshid=azure-stop-run>
   [alm-env-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-run-alm-lm.htm#mt-item-0>
   [azure-devops]:<https://dev.azure.com/>
   [azure-portal]:<http://portal.azure.com/>
   [azure-powershell]:<https://docs.microsoft.com/en-us/powershell/azure/install-az-ps?view=azps-6.0.0>
   [azure-connect]:<https://docs.microsoft.com/en-us/powershell/module/az.accounts/connect-azaccount?view=azps-6.0.0>
   [parallel-docs]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-trigger-parallel-run.htm>
   [get-digital-lab-resources]:<https://admhelp.microfocus.com/uft/en/latest/UFT_Help/Content/UFT_Tools/Azure_DevOps_Extension/uft-azure-devops-getresources.htm>