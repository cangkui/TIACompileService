# TIA Portal Compilation Service (based on TIA Openness, TIA Portal V19)

我们强烈推荐使用安装有Siemens TIA Portal V19的Windows 10专业版系统虚拟机来测试部署本项目。

### 先决条件

> * 系统运行环境：Windows 10 系统（强烈推荐使用专业版）
> * 软件运行环境：Siemens TIA Portal V19。**注意，安装TIA Portal V19时请务必勾选TIA Openness功能选项。**
> * 确保TIA Portal V19处于试用期或者许可证没有过期。

### 部署教程

1. 将当前用户加入TIA Openness用户组。打开计算机管理=>本地用户和组=>组，选中Siemens TIA Openness用户组，右键属性=>添加到组，添加Administrator用户和当前用户，应用并退出。重启系统或者注销当前用户后重新登录。
2. 为了保证兼容性，请务必将`TiaImportExample.exe`同级目录下的`Siemens.Engineering.dll`和`Siemens.Engineering.Hmi.dll`替换为本地TIA Portal V19提供的相应文件。这两个文件一般位于TIA Portal V19安装目录下，默认位置在`C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19`。
3. 在`TiaImportExample.exe`所在目录下，以**管理员身份**打开命令提示符. 运行`.\TiaImportExample.exe`，等待TIA Portal V19弹出申请窗口，点击全部允许。
4. 当程序打印出类似于以下内容时说明运行成功。

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

> 可以在同一局域网内通过 `curl http://192.168.103.245:9000/api/home` (或者浏览器) 测试接口访问是否正常，正常情况下应该显示`"Hello, World!"`。  
> 注意，正常情况下程序将自动扫描可用的局域网IP地址并将该地址作为程序监听地址，端口号默认为9000。如果您使用的是VMWare Workstation，请将虚拟机的网络设置为桥接（物理直连）以确保宿主机所在局域网能够访问到程序部署的HTTP服务。
