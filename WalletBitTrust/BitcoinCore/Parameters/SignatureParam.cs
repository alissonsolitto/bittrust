using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinCore.Parameters
{
    public class CreateSignatureParam
    {
        public string Seed { get; set; }
        public string Password { get; set; }
        public string Signature { get; set; }
    }

    public class VerifySignatureParam
    {
        public string Address { get; set; }
        public string Signature { get; set; }
        public string SignatureSign { get; set; }
    }
}
