using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonServerService.Model
{

    public class ServersModel
    {
        public List<ServerInfoModel> Servers { get; set; }
    }

    public class ServerInfoModel
    {
        public string Name { get; set; }

        public string URL { get; set; }

        public string Description { get; set; }

        public Process ServerProcess { get; set; } = null;

    }
}
