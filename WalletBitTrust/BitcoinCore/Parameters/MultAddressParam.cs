using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinCore.Parameters
{
    public class MultAddressParam
    {
        /// <summary>
        /// Lista de chaves públicas para gerar o endereço
        /// </summary>
        public List<string> LstPubKey { get; set; }
        /// <summary>
        /// Quantidade de chaves que precisam assinar a transação para gastar os fundos
        /// </summary>
        public int SigCount { get; set; }
    }
}
