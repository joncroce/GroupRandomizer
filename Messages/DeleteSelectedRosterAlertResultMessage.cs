using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupRandomizer.Messages
{
    public class DeleteSelectedRosterAlertResultMessage
    {
        public bool Answer { get; private set; }
        public DeleteSelectedRosterAlertResultMessage(bool answer)
        {
            Answer = answer;
        }
    }
}