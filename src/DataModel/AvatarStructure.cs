using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.DataModel
{
    public struct AvatarStructure
    {
        public string State { get; set; }
        public ReadOnlyMemory<byte> Data { get; set; }
    }
}
