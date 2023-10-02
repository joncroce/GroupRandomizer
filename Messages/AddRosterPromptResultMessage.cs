﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupRandomizer.Messages
{
    public class AddRosterPromptResultMessage : MessageBase
    {
        public string Result { get; private set; }
        public AddRosterPromptResultMessage(string result)
        {
            Result = result;
        }

    }
}
