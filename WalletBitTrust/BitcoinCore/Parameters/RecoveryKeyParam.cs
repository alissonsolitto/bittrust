using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinCore.Parameters
{
    public class RecoveryKeyParam
    {
        /// <summary>
        /// Conjunto de 12 palavras aleatórias
        /// </summary>
        public string Seed { get; set; }
        /// <summary>
        /// Senha da chave
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// Chave privada criptografada com AES na rede do Bitcoin
        /// </summary>
        public string SecretPrivateKey { get; set; }
        /// <summary>
        /// Chave mestra BIP 32 https://programmingblockchain.gitbooks.io/programmingblockchain/content/key_generation/key_generation.html#hd-wallet-bip-32
        /// </summary>
        public string RootKey { get; set; }
    }
}
