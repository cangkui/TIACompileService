# TIA Portal Compilation Service (based on TIA Openness, TIA Portal V19)

[👉中文版本](README_zh.md)

We strongly recommend testing and deploying this project on a Windows 10 Pro virtual machine installed with Siemens TIA Portal V19.

### Prerequisites

> * System Environment: Windows 10 (Pro edition is strongly recommended).
> * Software Environment: Siemens TIA Portal V19, which we have fully tested. Note: When installing TIA Portal V19, ensure the TIA Openness feature is checked.
> * Ensure TIA Portal V19 is in the trial period or your license has not expired.

### Deployment Tutorial

1. Add the current user to the TIA Openness user group.
> Tips:  
> 1. Open **Computer Management** > **Local Users and Groups** > **Groups**  
> 2. Select the **Siemens TIA Openness** group, right-click **Properties** > **Add**, include the **Administrator** user and the current user. 
> 3. Apply changes and exit. Restart the system or relog in.  

2. Replace DLLs.
> To ensure compatibility, replace the `Siemens.Engineering.dll` and `Siemens.Engineering.Hmi.dll` in the same directory as `TiaImportExample.exe` with the corresponding files from the local TIA Portal V19 installation.  
> These files are typically located in the TIA Portal V19 installation directory, by default at `C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19`.  

3. Run `.\TiaImportExample.exe` in **Command Prompt(cmd) with administrator privileges**, wait for the TIA Portal V19 permission window to pop up, and click **Allow All**. When the program prints output similar to the following code block, it indicates success.

```
Start initializing ...
Project Name: evaluation
Project Version:
Opened: evaluation
Initializing success.
Controller founded: TiaImportExample.Controllers.HomeController
Controller founded: TiaCompilerCLI.Controllers.TiaApiController
TIA Portal API service started, access address: http://192.168.103.245:9000/
StatusCode: 200, ReasonPhrase: 'OK', Version: 1.1, Content: System.Net.Http.StreamContent, Headers:
{
  Date: Sun, 18 May 2025 14:45:33 GMT
  Server: Microsoft-HTTPAPI/2.0
  Content-Length: 18
  Content-Type: application/json; charset=utf-8
}
HTTP service initialization successful!
Press Enter to exit...
```

> You can test the interface access within the same local area network using `curl http://192.168.103.245:9000/api/home` (or access it by your browser), which should normally return `Hello, World!`.  

> **Note:** The program will automatically scan for available LAN IP addresses and use them as the listening address, with the default port being `9000`. If using VMWare Workstation, you need to set the virtual machine's network to **Bridged (Physical Direct Connection)** mode to ensure the HTTP service deployed on the virtual machine is accessible from the host machine's local area network.
