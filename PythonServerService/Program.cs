using RASMachineController;
using RASMachineController.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace PythonServerService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

            Logger.Initialize(Constants.AllLoggerFileTypes);

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new PythonServerService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
