using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PolyChat.Models
{
    public class DialogButton
    {
        public string Text;
        public Action Action;

        public DialogButton(string text, Action action)
        {
            Text = text;
            Action = action;
        }
    }
}
