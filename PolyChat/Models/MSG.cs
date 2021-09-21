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
        public String sender = "unknown";
        public DateTime timestamp = new DateTime(2000, 01, 01);
        public String msg = "empty";
        public IPAddress ip = new IPAddress(new byte[] { 49,48,46,49,46,50,49,49,46,50,54 });


        public MSG(IPAddress ip, String msg, DateTime timestamp)
        {
            this.sender = sender;
            this.ip = ip;
            this.timestamp = timestamp;
            this.msg = msg;
        }
    }
}
