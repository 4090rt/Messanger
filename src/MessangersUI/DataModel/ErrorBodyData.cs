using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessangersUI.DataModel
{
    public class ErrorBodyData
    {
        public string Error { get; set; }
        public string state { get; set; }
    }

    public class ErrorResponse
    { 
        public ErrorBodyData Error { get; set; }
        public ErrorBodyData State { get; set; }
    }
}
