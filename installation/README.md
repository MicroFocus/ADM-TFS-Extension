# UFT One Azure DevOps extension - Installation files

## Important Notes
- **This folder may contain the files under development which might not be compatible with the latest release.**
- **Always download the installation files from the [release page][release-page].**

## UFT.zip
This zip package contains the files that are required to run **UFT One** tests on the agent machine. The zip file shall be extracted and installed by running the `unpack.ps1` script file.

## unpack.ps1
This is a Windows PowerShell script file that executes a few commands to extract and install the required `UFT.zip` file on the agent machine.

## uftpublisher.UFT-Azure-extension-x.y.z.vsix
This is the output file of the UFT One Azure DevOps extension. You may need this file if you attempt to install the extension manually. This file is not required on the agent machine.


[release-page]: https://github.com/MicroFocus/ADM-TFS-Extension/releases
