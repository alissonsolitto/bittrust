using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoinCore.Parameters;

namespace NBitcoinCore.Service
{    
    public class SignatureService
    {
        public object CreateSignature(CreateSignatureParam createSignatureParam)
        {
            try
            {
                //Verificar parametros de entrada

                var seed = new Mnemonic(createSignatureParam.Seed, Wordlist.PortugueseBrazil);
                ExtKey pKey = seed.DeriveExtKey(createSignatureParam.Password);

                var bitcoinPrivateKey = pKey.PrivateKey.GetBitcoinSecret(StaticConfig.Network);

                var result = new
                {
                    Signature = createSignatureParam.Signature,
                    SignatureSign = bitcoinPrivateKey.PrivateKey.SignMessage(createSignatureParam.Signature)
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        public object VerifySignature(VerifySignatureParam createSignatureParam)
        {
            try
            {
                //Verificar parametros de entrada

                var pubKeyAddress = new BitcoinPubKeyAddress(createSignatureParam.Address);
                bool isValid = pubKeyAddress.VerifyMessage(createSignatureParam.Signature, createSignatureParam.SignatureSign);

                var result = new
                {
                    isValid
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }
    }
}
