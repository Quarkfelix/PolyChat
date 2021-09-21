using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PolyChat.Models
{
    /// <summary>
    /// dumy class for json converter
    /// </summary>
    public class MSG 
    {
        public string sender = "unknown";
        public DateTime timestamp = new DateTime(2000, 01, 01);
        public string msg = "empty";
        public string ip;


        public MSG(string sender, string ip, String msg, DateTime timestamp)
        {
            this.sender = sender;
            this.ip = ip;
            this.timestamp = timestamp;
            this.msg = msg;
        }
    }
}
