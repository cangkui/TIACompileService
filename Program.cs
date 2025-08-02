using Microsoft.Owin.Hosting;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using TiaCompilerCLI.Configuration;
using TiaCompilerCLI.Services;

namespace TiaCompilerCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            //AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

            var port = AppConfig.GetInt("Service.Port", 9000);
            var host = AppConfig.Get("Service.Host");
            if (string.IsNullOrWhiteSpace(host))
            {
                host = GetLocalIPv4(); // 自动监听局域网 IP
            }
            var baseAddress = $"http://{host}:{port}/";

            // 启动OWIN主机
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine($"TIA Portal API service started, access address: {baseAddress}");

                var inst = TIACompileService.Instance;
                // Register application domain uninstallation events to ensure resource release when services are shut down
                AppDomain.CurrentDomain.ProcessExit += (sender, e) => {
                    Console.WriteLine("The application is exiting, releasing TIA Portal resources...");
                    inst.Dispose();
                };

                // Create HttpClient for testing web API
                HttpClient client = new HttpClient();
                var response = client.GetAsync($"{baseAddress}/api/home").Result;
                Console.WriteLine(response);
                Console.WriteLine("HTTP service initialization successful!");

                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
            }
        }

        private static string GetLocalIPv4()
        {
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                    continue;

                var ipProps = ni.GetIPProperties();
                foreach (var ip in ipProps.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                        !System.Net.IPAddress.IsLoopback(ip.Address))
                    {
                        return ip.Address.ToString();
                    }
                }
            }
            return "127.0.0.1";
        }

        private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name;

            if (assemblyName == "Siemens.Engineering")
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string dllPath = Path.Combine(baseDir, "Siemens.Engineering.dll");

                if (File.Exists(dllPath))
                {
                    return Assembly.LoadFrom(dllPath);
                }
            }

            return null; // 未处理其他依赖
        }

    }

}