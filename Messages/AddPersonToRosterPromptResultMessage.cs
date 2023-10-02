using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupRandomizer.Messages
{
    public class AddPersonToRosterPromptResultMessage : MessageBase
    {
        public string Result { get; private set; }
        public AddPersonToRosterPromptResultMessage(string result)
        {
            Result = result;
        }
    }
}
