using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.Models
{
    public class EndPoint
    {

        public Guid Id { get; set; }

        public bool IsChecked { get; set; }

        public string EndPointUri { get; set; }
    }
}
