using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LocalChat.Server.Encrypted
{
    public class ServerDetails
    {
        public string ServerPfx { get; set; }
        public string ExportPassword { get; set; }
    }
}
