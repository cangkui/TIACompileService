using System;
using System.Collections.Generic;
using System.IO;
using Siemens.Engineering;
using Siemens.Engineering.HW.Features;
using Siemens.Engineering.HW;
using Siemens.Engineering.SW;
using Siemens.Engineering.SW.ExternalSources;
using System.Linq;
using TiaCompilerCLI.Configuration;
using Siemens.Engineering.Compiler;
using Siemens.Engineering.SW.Blocks;
using Siemens.Engineering.SW.Types;
using System.Text.RegularExpressions;


namespace TiaCompilerCLI.Services
{
    public sealed class TIACompileService
    {
        private static TIACompileService instance = new TIACompileService();
        public static TIACompileService Instance => instance;

        private readonly TiaPortal tiaPortal;
        private string projectPath;
        private Project project;
        private readonly string _storagePath;

        private readonly PlcSoftware plcSoftware;


        private TIACompileService()
        {
            Console.WriteLine("Start initializing ...");

            tiaPortal = new TiaPortal(TiaPortalMode.WithoutUserInterface);
            projectPath = "";
            
            string configProjectPath = AppConfig.Get("Protal.ProjectPath", projectPath);
            if (!string.IsNullOrEmpty(configProjectPath))
                projectPath = configProjectPath;
            else
                projectPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PortalProject/evaluation/evaluation.ap19");

            _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");

            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }

            if (tiaPortal == null)
            {
                Console.WriteLine("Initializing failed: TIA Portal Failed.");
                throw new Exception("Initializing failed: TIA Portal Failed.");
            }

            project = ProjectOpen(projectPath);
            if (project == null)
            {
                Console.WriteLine("Initializing failed: TIA Project Failed.");
                throw new Exception("Initializing failed: TIA Project Failed.");
            }

            foreach (var plc in GetAllPlcSoftwares())
            {
                if (plc != null)
                {
                    plcSoftware = plc;
                    break;
                }
            }
            if (plcSoftware == null)
            {
                Console.WriteLine("Initializing failed: TIA Project Failed.");
                throw new Exception("Initializing failed: TIA Project Failed.");
            }

            Console.WriteLine($"Opened: {project.Name}");
            Console.WriteLine("Initializing success.");
        }

        public void Dispose()
        {
            project?.Close();
            tiaPortal?.Dispose();
            CleanSCLFiles();
        }

        public void CleanSCLFiles()
        {
            if (!Directory.Exists(_storagePath))
                return;

            var files = Directory.EnumerateFiles(_storagePath, "*.scl")
                .Select(f => new FileInfo(f));

            foreach (var file in files)
            {
                try
                {
                    file.Delete();
                    Console.WriteLine($"Temp files deleted: {file.FullName}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Temp files delete failed: {file.FullName}, Error: {ex.Message}");
                }
            }
        }

        public List<ErrorMessage> GetMessages(CompilerResultMessage curNode, int depth)
        {
            List<ErrorMessage> res = new List<ErrorMessage>();

            foreach(var msg in curNode.Messages)
            {
                if (msg.ErrorCount <= 0)
                    continue;
                List<ErrorMessage> subMsgs = GetMessages(msg, depth + 1);
                res.AddRange(subMsgs);
            }

            if (curNode.ErrorCount > 0 && !string.IsNullOrEmpty(curNode.Description) && !string.IsNullOrEmpty(curNode.Path))
            {
                var path = curNode.Path.Trim();
                try
                {
                    var k = int.Parse(path);
                }
                catch (Exception ex)
                {
                    // If parsing fails, we can still add the error message with a default path
                    path = "-1"; // Default path for errors without a valid path
                    Console.WriteLine($"Cannot parse path: {curNode.Path}, default set to -1.");
                }
                res.Add(new ErrorMessage
                {
                    Path = int.Parse(path),
                    //Path = curNode.Path,
                    ErrorDesc = curNode.Description,
                    IsDef = false
                });
            }

            return res;
        }

        public List<ErrorMessage> GetErrorMessagesFromRoot(CompilerResult root)
        {
            //Console.WriteLine("Start###");
            //Console.WriteLine("Number of compiler errors: {0}", root.ErrorCount);
            
            List<ErrorMessage> errs = new List<ErrorMessage>();
            if (root.ErrorCount <= 0)
                return errs;

            foreach (var compilerResultMessage in root.Messages)
            {
                List<ErrorMessage> msgs = GetMessages(compilerResultMessage, 1);
                if (msgs == null || msgs.Count <= 0)
                    continue;
                errs.AddRange(msgs);
            }

            //var sortedMsgs = errs
            //        .OrderBy(m => m.Path)
            //        .Select(x => x.ToString())
            //        .ToList();
            //Console.WriteLine($"Those is: \n{string.Join("\n--\n", sortedMsgs)}");
            //Console.WriteLine("End###");

            return errs.OrderBy(m => m.Path).ToList();
        }

        public List<ErrorMessage> ParseErrorMessages(string input)
        {
            List<ErrorMessage> errorMessages = new List<ErrorMessage>();
            if (string.IsNullOrWhiteSpace(input))
                return errorMessages;

            Regex regex = new Regex(@"(?:行|Line)\s+(?<line>\d+):\s+(?<error>.+?)(?=\s+(?:行|Line)\s+\d+:|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            MatchCollection matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    int lineNumber;
                    string desc = match.Groups["error"].Value.Trim();
                    if (
                        string.IsNullOrEmpty(desc) || 
                        (!desc.Contains("错误") && !desc.Contains("Error"))
                    )
                    {
                        continue;
                    }
                    if (int.TryParse(match.Groups["line"].Value, out lineNumber))
                    {
                        errorMessages.Add(new ErrorMessage
                        {
                            Path = lineNumber + 1,
                            ErrorDesc = desc,
                            IsDef = true
                        });
                    }
                }
            }

            return errorMessages;
        }

        public bool CheckIfCanGen(
            bool check, 
            ref List<ErrorMessage> errs, 
            string blockName, 
            string filePath
        ) {
            GenerateBlockOption option = check ? GenerateBlockOption.None : GenerateBlockOption.KeepOnError;
            PlcBlock block = null;
            PlcExternalSource externalSource = null;
            bool ret = false;

            try
            {
                PlcExternalSource originExternalSource = plcSoftware.ExternalSourceGroup.ExternalSources.Find(blockName);
                originExternalSource?.Delete();
                externalSource = plcSoftware.ExternalSourceGroup.ExternalSources.CreateFromFile(blockName, filePath);
                IList<IEngineeringObject> engineeringObjects = externalSource.GenerateBlocksFromSource(option);

                //var compiler = plcSoftware.GetService<ICompilable>();

                Console.WriteLine($"There are {engineeringObjects.Count} objs.");
                foreach (IEngineeringObject obj in engineeringObjects)
                {
                    CompilerResult compilerResult = null;
                    if (obj is PlcBlock)
                    {
                        block = (PlcBlock)obj;
                        var compiler = block.GetService<ICompilable>();
                        if (compiler != null)
                            compilerResult = compiler.Compile();
                    }
                    else if (obj is PlcType)
                    {
                        PlcType plcType = (PlcType)obj;
                        var compiler = plcType.GetService<ICompilable>();
                        if (compiler != null)
                            compilerResult = compiler.Compile();
                    }

                    if (compilerResult != null)
                    {
                        List<ErrorMessage> errors = GetErrorMessagesFromRoot(compilerResult);
                        errs.AddRange(errors);
                        var errors_str = string.Join("\n", errors.Select(p => p.ToString()));
                        Console.WriteLine($"CheckIfCanGen success, Get errors:\n{errors_str}");
                    }
                }
                ret = true;
            }
            catch (Exception exception)
            {
                if (check)
                {
                    List<ErrorMessage> defErrors = ParseErrorMessages(exception.Message);
                    if (defErrors.Count == 0)
                    {
                        defErrors.Add(new ErrorMessage
                        {
                            Path = -1,
                            ErrorDesc = exception.Message,
                            IsDef = true
                        });
                    }
                    errs.AddRange(defErrors);
                    Console.WriteLine($"Except Error: {exception.Message}");
                    var errors_str = string.Join("\n", defErrors.Select(p => p.ToString()));
                    Console.WriteLine($"CheckIfCanGen failed, Get def errors:\n{errors_str}");
                    ret = false;
                }
                else
                {
                    ret = true;
                }
            }
            finally
            {
                block?.Delete();
                externalSource?.Delete();
            }
            return ret;
        }

        public ResponseData Process(string blockName, string code)
        {
            //CleanSCLFiles();
            if (!string.IsNullOrEmpty(code))
            {

                string fileName = $"{blockName}_{DateTime.Now:yyyyMMdd_HHmmss_fff}.scl";
                string filePath = Path.Combine(_storagePath, fileName);
                File.WriteAllText(filePath, code);

                Console.WriteLine($"File has been temporarily storaged in: {filePath}");
                var resultMessage = new ResponseData();

                List<ErrorMessage> errs = new List<ErrorMessage>();

                bool ifCanbeGen = CheckIfCanGen(true, ref errs, blockName, filePath);
                if (!ifCanbeGen)
                {
                    CheckIfCanGen(false, ref errs, blockName, filePath);
                }

                Console.WriteLine($"################# Task {blockName} ################# \nHas {errs.Count} errors.");
                if (errs.Count > 0)
                {
                    resultMessage.Success = false;
                    var errsString = string.Join("\n", errs.Select(er => er.ToString()) );
                    Console.WriteLine($"These errors are: \n{errsString}");
                    resultMessage.Errors = errs;
                }
                else
                {
                    resultMessage.Success = true;
                    resultMessage.Errors = new List<ErrorMessage>();
                    resultMessage.Result = "Compile Success.";
                }
                Console.WriteLine("\n");
                return resultMessage;
            }

            return new ResponseData { Success = false, Result = "Processing failed." };
        }

        public Project ProjectOpen(string projectPath)
        {
            try
            {
                // Refer Connecting to the TIA Portal section
                FileInfo f = new FileInfo(projectPath);
                Project project = tiaPortal.Projects.Open(f);

                if (project != null)
                {
                    Console.WriteLine("Project Name: " + project.Name);
                    Console.WriteLine("Project Version: " + project.Version);
                }
                return project;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception of type {ex.GetType()} occurred: {ex.Message}");
                return null;
            }
        }

        public PlcSoftware GetPlcSoftware(Device device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device), "Parameter is null");
            foreach (var devItem in device.DeviceItems)
            {
                var target = ((IEngineeringServiceProvider)devItem).GetService<SoftwareContainer>();
                if (target != null && target.Software is PlcSoftware)
                    return (PlcSoftware)target.Software;
            }
            return null;
        }

        public IEnumerable<PlcSoftware> GetAllPlcSoftwares()
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project), "Parameter is null");

            foreach (var device in project.Devices)
            {
                var ret = GetPlcSoftware(device);
                if (ret != null)
                    yield return ret;
            }
        }
    }


    public class ResponseData
    {
        public bool Success { get; set; }
        public string? Result { get; set; }
        public List<ErrorMessage> Errors { get; set; }
    }

    public class ErrorMessage
    {
        public int Path { get; set; }
        public string ErrorDesc { get; set; }
        public bool IsDef { get; set; }

        public override string ToString()
        {
            return $"Path: {Path}, Error Desc: {ErrorDesc}, Is Def Error: {IsDef}";
        }
    }
}
