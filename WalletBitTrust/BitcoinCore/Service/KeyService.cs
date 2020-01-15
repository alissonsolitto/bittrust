using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoinCore.Model;
using NBitcoinCore.Parameters;
using NBitcoinCore.Uteis;

namespace NBitcoinCore.Service
{
    public class KeyService
    {
        public GenerateKeyModel GenerateKey(string password)
        {
            try
            {
                if(String.IsNullOrWhiteSpace(password))
                {
                    throw new Exception("Password inválido");
                }

                //Gerando seed
                Mnemonic seed = new Mnemonic(Wordlist.PortugueseBrazil, WordCount.Twelve);
                ExtKey pKey = seed.DeriveExtKey(password);

                var result = new GenerateKeyModel
                {
                    seed = seed.ToString(),
                    //password = password,
                    //rootKey = pKey.ToString(StaticConfig.Network),
                    secretPrivateKey = pKey.PrivateKey.GetEncryptedBitcoinSecret(password, StaticConfig.Network).ToString(),
                    //privateKey = pKey.PrivateKey.ToString(StaticConfig.Network),
                    publicKey = pKey.PrivateKey.PubKey.ToString(),
                    //scriptPubKey = pKey.ScriptPubKey.ToString(),
                    address = pKey.PrivateKey.PubKey.Hash.GetAddress(StaticConfig.Network).ToString()
                };

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public object RecoverySecretKey(RecoveryKeyParam recoveryKey)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(recoveryKey.Password) || String.IsNullOrWhiteSpace(recoveryKey.Seed))
                {
                    return new Exception("Password ou Seed inválidas");
                }

                //Recuperando chave
                Mnemonic seed = new Mnemonic(recoveryKey.Seed);
                ExtKey pKey = seed.DeriveExtKey(recoveryKey.Password);

                var result = new
                {
                    secretPrivateKey = pKey.PrivateKey.GetEncryptedBitcoinSecret(recoveryKey.Password, StaticConfig.Network).ToString(),
                    publicKey = pKey.PrivateKey.PubKey.ToString(),
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        public object RecoveryPrivateKey(RecoveryKeyParam recoveryKey)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(recoveryKey.Password) || String.IsNullOrWhiteSpace(recoveryKey.SecretPrivateKey))
                {
                    return new Exception("Password ou SecretPrivateKey inválidas");
                }

                //Recuperando Private Key
                BitcoinEncryptedSecretNoEC pKey = new BitcoinEncryptedSecretNoEC(recoveryKey.SecretPrivateKey, StaticConfig.Network);
                Key privateKey = pKey.GetKey(recoveryKey.Password);                

                var result = new
                {
                    privateKey = privateKey.ToString(StaticConfig.Network),
                    publicKey = privateKey.PubKey.ToString(),
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        public object RecoveryMasterKey(RecoveryKeyParam recoveryKey)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(recoveryKey.RootKey))
                {
                    return new Exception("RootKey inválida");
                }

                //Recuperando Master Key
                //byte[] rootKeyByte = 
                //byte[] chainCode = new ExtKey().ChainCode;

                //ExtKey masterKey = new ExtKey(new Key(rootKeyByte), chainCode);

                var result = new
                {
                    //rootKey = masterKey.ToString(StaticConfig.Network)
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        public object GenerateAddress(string publicKey)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(publicKey))
                {
                    return new Exception("PublicKey inválida");
                }

                //Recuperando PublicKey
                PubKey pubKey = new PubKey(publicKey);

                var result = new
                {
                    address = pubKey.Hash.GetAddress(StaticConfig.Network).ToString()
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }


        public object GenerateMultAddress(MultAddressParam multAddress)
        {
            try
            {
                if (multAddress.LstPubKey.Count == 0)
                {
                    return new Exception("PubKey's inválidos");
                }

                if (multAddress.SigCount <= 0)
                {
                    return new Exception("SigCount inválido");
                }

                PubKey[] pubKeyArray = multAddress.LstPubKey.Select((pubKey, index) => new PubKey(pubKey)).ToArray();
                
                //Gerando scriptPubKey
                var scriptMultSign = PayToMultiSigTemplate
                    .Instance
                    .GenerateScriptPubKey(multAddress.SigCount, pubKeyArray);

                var result = new
                {
                    address = scriptMultSign.Hash.GetAddress(StaticConfig.Network).ToString()
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        public object GenerateDeriveKey()
        {
            try
            {
                var result = new
                {
                    
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
