using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class DataModel
    {
        public string type { get; set; }
        public Data data { get; set; }
        public Settings settings { get; set; }

        public class Data
        {
            public long to { get; set; }
            public long from { get; set; }
            public string message { get; set; }
        }

        public class Settings
        {
            public long id { get; set; }
        }
    }
}
