using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyChat.Models.Exceptions
{
    public class MessageTimedOutException : Exception
    {
        public MessageTimedOutException(int seconds) : base(String.Format("After {0} seconds of trying to send the message it has timed out", seconds))
        {

        }
    }
}
