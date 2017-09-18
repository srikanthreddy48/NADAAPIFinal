using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NADAAPI.Data
{
    public class JWToken
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
