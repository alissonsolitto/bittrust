using System;
using System.Collections.Generic;
using System.Text;

namespace NBitcoinCore.Model
{
    public class GenerateKeyModel
    {
        public string seed { get; set; }
        public string secretPrivateKey { get; set; }
        public string publicKey { get; set; }
        public string address { get; set; }
    }
}
