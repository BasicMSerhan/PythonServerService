using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PythonServerService.Helpers.Model
{
    public class LoggingItem
    {

        public DateTime DateCreated { get; set; }

        public string Type { get; set; }

        public string Tag { get; set; }

        public string Message { get; set; }

    }
}
