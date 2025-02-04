using Newtonsoft.Json;
using PythonServerService.Model;
using RASMachineController;
using RASMachineController.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PythonServerService
{
    public partial class PythonServerService : ServiceBase
    {

        private bool CancellationPending = false;

        private List<ServerInfoModel> CurrentServerModels = new List<ServerInfoModel>();

        public PythonServerService()
        {
            InitializeComponent();

            Logger.Debug("INIT", "Python Server Service Initialized, Version: " + Constants.AppVersionString);

        }

        protected override void OnStart(string[] args)
        {
            Logger.Debug("INIT", "Python Server Service Started!");
            LoadConfigurations();

            foreach(var server in CurrentServerModels)
            {
                new Thread(new ThreadStart(async delegate
                {
                    PerformBackgroundTask(server);
                })).Start();
            }
        }

        protected override void OnStop()
        {
            Logger.Debug("EXIT", "Python Server Service Stopped!");
            CancellationPending = true;
        }

        private void LoadConfigurations()
        {
            try
            {
                var jsonData = File.ReadAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\AppSettings.json");

                CurrentServerModels = JsonConvert.DeserializeObject<ServersModel>(jsonData).Servers;

                Logger.Debug("LoadConfigurations", "Loaded All Configurations: " + jsonData);

            } 
            catch (Exception ex) 
            {
                Logger.Error("LoadConfigurations", "Error While Loading Configurations, Error: " + ex.ToString());
            }

        }

        private async void PerformBackgroundTask(ServerInfoModel server)
        {

            Logger.Debug("BACKGROUND", "Python Server Service Background Task Started!");

            while (!this.CancellationPending)
            {

                var isServerActive = await IsServerActive(server.URL);

                Logger.Debug("HTTP", "HTTP Request To Server: " + server.Name + ", URL: " + server.URL + " Returned Active: " + isServerActive);

                if (!isServerActive)
                {
                    LaunchPythonServer(server);
                }

                Thread.Sleep(60000);
            }
        }

        /// <summary>
        /// Launches The Python Server
        /// </summary>
        private void LaunchPythonServer(ServerInfoModel server)
        {
            new Thread(new ThreadStart(delegate
            {

                if (server.ServerProcess != null)
                {
                    Logger.Debug(server.Name + "-EXEC", "Old Server Process Exists, Stopping Now!");
                    try
                    {
                        server.ServerProcess.Kill();
                        Logger.Debug(server.Name + "-EXEC", "Done Stopping Old Server Process!");
                    }
                    catch(Exception ex) {
                        Logger.Error(server.Name + "-EXEC", "Error Stopping Old Server Process, Error: " + ex.ToString());
                    }
                }

                var pythonServerPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + $"\\Servers\\{server.Name}\\";

                Logger.Debug(server.Name + "-EXEC", "Launching Python Server Process In Path: " + pythonServerPath);

                var cmd = "-u " + pythonServerPath + "ServerHandler.py";
                server.ServerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "C:\\Python312\\python.exe",
                        WorkingDirectory = pythonServerPath,
                        Arguments = cmd,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };

                server.ServerProcess.ErrorDataReceived += (sender, e) =>
                {
                    Logger.Debug(server.Name + "-OUTPUT", e.Data);
                };

                server.ServerProcess.OutputDataReceived += (sender, e) =>
                {
                    Logger.Debug(server.Name + "-OUTPUT", e.Data);
                };

                server.ServerProcess.Start();
                server.ServerProcess.BeginErrorReadLine();
                server.ServerProcess.BeginOutputReadLine();

                Logger.Debug(server.Name + "-EXEC", "Done Launching Server Process, Added Error And Output Listeners");

                server.ServerProcess.WaitForExit();
            })).Start();
        }

        public async Task<bool> IsServerActive(string url)
        {
            HttpClient client = null;
            try
            {
                client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                    return true;


            }
            catch (HttpRequestException ex)
            {
                Logger.Error("HTTP", "Error Making Http Request To: " + url + ", Error: " + ex.ToString());
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                }
            }

            return false;
        }

    }
}
