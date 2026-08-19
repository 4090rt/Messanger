using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.DataModel
{
    public class DataRegistr
    {
        public string Login { get; set; }
        public string cachePassword { get; set; }
        public DateTime date = DateTime.Now;
        public ReadOnlyMemory<byte> Data { get; set; }
        public string Tnumber { get; set; }
        public string Mail { get; set; }
    }
}
