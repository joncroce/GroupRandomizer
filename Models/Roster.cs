using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupRandomizer.Models
{
    public class Roster
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<string> People { get; set; }
    }
}
