## How to use Openness Project

����ǿ���Ƽ�ʹ�ð�װ��Siemens TIA Portal V19��Windows 10רҵ��ϵͳ����������Բ�����Ŀ��

### �Ⱦ�����

> * ϵͳ���л�����Windows 10 ϵͳ��ǿ���Ƽ�ʹ��רҵ�棩
> * ������л�����Siemens TIA Portal V19��**ע�⣬��װTIA Portal V19ʱ����ع�ѡTIA Openness����ѡ�**
> * ȷ��TIA Portal V19���������ڻ������֤û�й��ڡ�

### ����̳�

1. ����ǰ�û�����TIA Openness�û��顣�򿪼��������=>�����û�����=>�飬ѡ��Siemens TIA Openness�û��飬�Ҽ�����=>��ӵ��飬���Administrator�û��͵�ǰ�û���Ӧ�ò��˳�������ϵͳ����ע����ǰ�û������µ�¼��
2. Ϊ�˱�֤�����ԣ�����ؽ�`TiaImportExample.exe`ͬ��Ŀ¼�µ�`Siemens.Engineering.dll`��`Siemens.Engineering.Hmi.dll`�滻Ϊ����TIA Portal V19�ṩ����Ӧ�ļ����������ļ�һ��λ��TIA Portal V19��װĿ¼�£�Ĭ��λ����`C:\Program Files\Siemens\Automation\Portal V19\PublicAPI\V19`��
3. ��`TiaImportExample.exe`����Ŀ¼�£��Թ���Ա��ݴ�������ʾ��. ����`.\TiaImportExample.exe`���ȴ�TIA Portal V19�������봰�ڣ����ȫ������
4. �������ӡ����������������ʱ˵�����гɹ���

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

> ������ͬһ��������ͨ�� `curl http://192.168.103.245:9000/api/home` ���Խӿڷ����Ƿ����������������Ӧ����ʾ`"Hello, World!"`��  
> ע�⣬��������³����Զ�ɨ����õľ�����IP��ַ�����õ�ַ��Ϊ���������ַ���˿ں�Ĭ��Ϊ9000�������ʹ�õ���VMWare Workstation���뽫���������������Ϊ�Žӣ�����ֱ������ȷ�����������ھ������ܹ����ʵ��������HTTP����
