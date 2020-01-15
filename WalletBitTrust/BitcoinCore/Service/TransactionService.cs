using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NBitcoin;
using NBitcoin.Policy;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using NBitcoinCore.Model;
using NBitcoinCore.Parameters;
using NBitcoinCore.Service;
using NBitcoinCore.Uteis;

namespace NBitcoinCore.Controllers.Bitcoin
{
    public class TransactionService
    {
        private readonly BitcoinFeesService _bitcoinFeesService;
        private readonly ReceivedValidService _receivedValidService;
        private readonly BroadcastService _broadcastService;

        public TransactionService()
        {
            _bitcoinFeesService = new BitcoinFeesService();
            _receivedValidService = new ReceivedValidService();
            _broadcastService = new BroadcastService();
        }

        //Transaction 1:0; 1:1; 1:N
        public object CreateSingleSignTransaction(TransactionSingleParam transactionParam)
        {
            try
            {
                /// Validações dos parametros de entrada
                foreach (PropertyInfo propertyInfo in transactionParam.GetType().GetProperties())
                {
                    if (!propertyInfo.Name.Equals("AddressOut")) //AddressOut não é obrigatório
                    {
                        var value = propertyInfo.GetValue(transactionParam, null);
                        if ((value == null) || (value.ToString() == String.Empty))
                        {
                            return new Exception("O parâmetro " + propertyInfo.Name + " é inválido.");
                        }
                    }
                }

                /// Criando o objeto BitcoinAddress com o endereço informado                
                BitcoinAddress btcAddressIn = BitcoinAddress.Create(transactionParam.AddressOrigin, StaticConfig.Network);

                /// Recuperando a chave privada do BitcoinSecret para assinar a transação
                BitcoinEncryptedSecretNoEC pKey = new BitcoinEncryptedSecretNoEC(transactionParam.BitcoinSecret, StaticConfig.Network);
                Key privateKey = pKey.GetKey(transactionParam.PasswordSecret);

                /// Buscando as saídas não gastas do endereço que tenha valor maior que zero satoshis
                List<ICoin> lstCoin = _receivedValidService.LstReceivedValid(btcAddressIn);

                if (lstCoin.Count <= 0)
                {
                    return new Exception("O endereço não possui entradas confirmadas com valores maior que zero");
                }

                Coin[] coin = lstCoin.Select((o, i) => new Coin(o.Outpoint, o.TxOut)).ToArray();

                /// Destinatários da transação
                IDestination[] btcAddressOut;
                if (transactionParam.AddressOut != null)
                {
                    btcAddressOut = transactionParam.AddressOut.Select((item, index) => BitcoinAddress.Create(item, StaticConfig.Network)).ToArray();
                }
                else
                {
                    btcAddressOut = new IDestination[0];
                }

                /// Criando o objeto TransactionBuilder
                TransactionBuilder txBuilder = new TransactionBuilder();
                txBuilder.StandardTransactionPolicy.MinRelayTxFee = FeeRate.Zero; // Politica que define a taxa minima de transacao
                txBuilder.DustPrevention = false; // False para permitir transações com poucos satoshis

                Transaction tran = txBuilder
                    // Adiciona sempre todas as saídas para normalizar em uma só (Melhor controle)
                    .AddCoins(coin)
                    // Chaves privadas para assinar a transação
                    .AddKeys(privateKey)
                    // Endereços de saida []
                    .Send(btcAddressOut)
                    //// OP_RETURN Hash do arquivo
                    .Send(TxNullDataTemplate.Instance.GenerateScriptPubKey(Functions.HexStringToBytes(transactionParam.Hashfile)))
                    // Nova entrada com a sobra da transação
                    .SetChange(privateKey.ScriptPubKey) // Protocolo P2PKH
                                                        // Adicionando as taxas, quando é simulação inclui apenas 1 satoshi
                    .SendFees(transactionParam.Simulation ? Money.Satoshis(1) : Money.Satoshis(transactionParam.TxFee))
                    // Cria a transação e assina
                    .BuildTransaction(true);

                if (transactionParam.Simulation)
                {
                    var result = new
                    {
                        transactionBytes = tran.GetSerializedSize()
                    };

                    return result;
                }
                else
                {
                    //Verificar se a transação possui erros
                    txBuilder.Verify(tran, out TransactionPolicyError[] errors);

                    if (errors.Length > 0)
                    {
                        return new Exception(errors.ToString());
                    }
                    else
                    {
                        //Se não houver erros envia a transação para a rede
                        var client = new QBitNinjaClient(StaticConfig.Network);
                        BroadcastResponse broadcastResponse = client.Broadcast(tran).Result;

                        if (!broadcastResponse.Success)
                        {
                            return new Exception(broadcastResponse.Error.Reason);
                        }
                        else
                        {
                            return new
                            {
                                Message = "Transação efetuada com sucesso!",
                                Data = "https://live.blockcypher.com/btc-testnet/tx/" + tran.GetHash().ToString()
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        private int SimulationTransaction(ICoin[] coin, IDestination[] btcAddressOut, byte[] hash, Script scriptPubKey)
        {
            TransactionBuilder txBuilder = new TransactionBuilder();
            txBuilder.StandardTransactionPolicy.MinRelayTxFee = FeeRate.Zero; // Politica que define a taxa minima de transacao
            txBuilder.DustPrevention = false;

            Transaction tran = txBuilder
                    // Adiciona sempre todas as saídas para normalizar em uma só (Melhor controle)
                    .AddCoins(coin)
                    // Endereços de saida []
                    .Send(btcAddressOut)
                    // OP_RETURN Hash do arquivo
                    .Send(TxNullDataTemplate.Instance.GenerateScriptPubKey(hash))
                    // Nova saída com a sobra da transação - Protocolo P2PKH
                    .SetChange(scriptPubKey)
                    // A Taxa é definida na aplicação
                    .SendFees(Money.Satoshis(1))
                    // Cria a transação e NÃO assina
                    .BuildTransaction(true);

            return tran.ToBytes().Length;
        }

        public object RegisterFile(RegisterFileParam fileParam)
        {
            try
            {
                /// Validações dos parametros de entrada
                foreach (PropertyInfo propertyInfo in fileParam.GetType().GetProperties())
                {
                    var value = propertyInfo.GetValue(fileParam, null);
                    if ((value == null) || (value.ToString() == String.Empty))
                    {
                        return new Exception("O parâmetro " + propertyInfo.Name + " é inválido.");
                    }
                }

                /// Criando o objeto BitcoinAddress com o endereço informado                
                BitcoinAddress btcAddressIn = BitcoinAddress.Create(fileParam.AddressOrigin, StaticConfig.Network);

                /// Buscando as saídas não gastas do endereço que tenha valor maior que zero satoshis
                List<ICoin> lstCoin = _receivedValidService.LstReceivedValid(btcAddressIn, Money.Satoshis(10000));

                if (lstCoin.Count <= 0)
                {
                    return new Exception("O endereço não possui entradas confirmadas com valores maior que zero");
                }

                Coin[] coin = lstCoin.Select((o, i) => new Coin(o.Outpoint, o.TxOut)).ToArray();

                /// Destinatários da transação
                IDestination[] btcAddressOut;
                if (fileParam.AddressOut != null)
                {
                    btcAddressOut = fileParam.AddressOut.Select((item, index) => BitcoinAddress.Create(item, StaticConfig.Network)).ToArray();
                }
                else
                {
                    btcAddressOut = new IDestination[0];
                }

                /// Gerando o Hash                
                byte[] hash = Functions.GenerateHashFile(Convert.FromBase64String(fileParam.FileB64));

                /// Simulando a transação
                decimal fee = SimulationTransaction(coin, btcAddressOut, hash, btcAddressIn.ScriptPubKey) * StaticConfig.Fee;

                /// Criando o objeto TransactionBuilder
                TransactionBuilder txBuilder = new TransactionBuilder();
                txBuilder.StandardTransactionPolicy.MinRelayTxFee = FeeRate.Zero; // Politica que define a taxa minima de transacao
                txBuilder.DustPrevention = false; // False para permitir transações com poucos satoshis

                Transaction tran = txBuilder
                    // Adiciona sempre todas as saídas para normalizar em uma só (Melhor controle)
                    .AddCoins(coin)
                    // Endereços de saida []
                    .Send(btcAddressOut)
                    // OP_RETURN Hash do arquivo
                    .Send(TxNullDataTemplate.Instance.GenerateScriptPubKey(hash))
                    // Nova saída com a sobra da transação - Protocolo P2PKH
                    .SetChange(btcAddressIn.ScriptPubKey)
                    // A Taxa é definida na aplicação
                    .SendFees(Money.Satoshis(fee))
                    // Cria a transação e NÃO assina
                    .BuildTransaction(false);

                var result = new
                {
                    hashFile = Functions.HashToString(hash).ToUpper(),
                    transactionHex = tran.ToHex()
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        public object Broadcast(BroadcastParam tranParam)
        {
            try
            {
                /// Validação dos parametros de entrada
                if ((tranParam.Transaction == null) || (tranParam.Transaction.ToString() == String.Empty))
                {
                    return new Exception("O parâmetro Transaction é inválido.");
                }

                Transaction tran = new Transaction(tranParam.Transaction);

                // Envia a transação para a rede
                var client = new QBitNinjaClient(StaticConfig.Network);
                BroadcastResponse broadcastResponse = client.Broadcast(tran).Result;

                if (!broadcastResponse.Success)
                {
                    return new Exception(broadcastResponse.Error.Reason);
                }
                else
                {
                    return new
                    {
                        Message = "Transação efetuada com sucesso!",
                        Data = "https://live.blockcypher.com/btc-testnet/tx/" + tran.GetHash().ToString()
                    };
                }
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        //[HttpPost("CreateMultSign")]
        //public object CreateTransactionMultSign(TransactionParam transactionParam)
        //{
        //    try
        //    {
        //        //Efetuar validações dos parametros de entrada
        //        //..
        //        //..

        //        //Buscar saidas não gastas do endereço
        //        Script redeemScript;

        //        if (transactionParam.PubKey.Count > 0)
        //        {
        //            redeemScript = PayToMultiSigTemplate
        //                .Instance
        //                .GenerateScriptPubKey(
        //                    transactionParam.SigCount,
        //                    transactionParam.PubKey.Select((item, index) => new PubKey(item)).ToArray());
        //        }
        //        else
        //        {

        //        }



        //        List<ICoin> lstCoin = _receivedValidService.LstReceivedValid();

        //        if (lstCoin.Count <= 0)
        //        {
        //            return new Exception("O endereço não possui entradas confirmadas com valores maior que zero");
        //        }

        //        //Montando a transação
        //        TransactionBuilder txBuilder = new TransactionBuilder();
        //        txBuilder.StandardTransactionPolicy.MinRelayTxFee = FeeRate.Zero; //Politica que define a taxa minima de transacao
        //        txBuilder.DustPrevention = false; //False para permitir transações com poucos satoshis

        //        //Moedas de entrada
        //        ScriptCoin[] scriptCoin;
        //        Coin[] coin;

        //        if (transactionParam.SignKey.Count > 0) //Pagamento com script, mais de uma assinatura
        //        {
        //            scriptCoin = lstCoin.Select((o, i) => new Coin(o.Outpoint, o.TxOut).ToScriptCoin(btcAddress.ScriptPubKey)).ToArray();
        //        }
        //        else
        //        {
        //            coin = lstCoin.Select((o, i) => new Coin(o.Outpoint, o.TxOut)).ToArray();
        //        }

        //        //Destinatarios da transação
        //        foreach (var item in transactionParam.Out)
        //        {
        //            txBuilder.Send(BitcoinAddress.Create(item, StaticConfig.Network).ScriptPubKey, Money.Zero);
        //        }


        //Destinatarios da transação
        //BitcoinAddress btcAddressOut = BitcoinAddress.Create(transactionParam.Out[0], StaticConfig.Network);
        //        if(btcAddressIn != btcAddressOut) //Endereço de envio é diferente do destinatário
        //        {
        //            txBuilder.Send(btcAddressOut.ScriptPubKey, Money.Zero);
        //        }


        //        //Estimar a taxa de transação
        //        //var result = _bitcoinFeesService.GetFeeRecommended();

        //        //Buscar as saidas válidas
        //        //BitcoinAddress btcAddress = BitcoinAddress.Create("msZJgrrm1bRGpgaNT4nBpXg6A3qSBd8GYS", StaticConfig.Network);




        //        //Criar a entrada
        //        //Criar as saídas
        //        //Gerar hash do arquivo
        //        //Assinar a transação
        //        //Broadcast transação



        //        var result = new
        //        {

        //        };

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Exception(ex.Message);
        //    }
        //}

        public object TransactionSpecific(string idTransaction)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(idTransaction))
                {
                    return new Exception("ID Transaction inválido");
                }

                //Criando Client QBitNinjaClient
                var client = new QBitNinjaClient(StaticConfig.Network);

                //Percorre todas as transações do endereço
                var tran = client.GetTransaction(uint256.Parse(idTransaction)).Result;

                var result = new
                {
                    BlockId = (tran.Block != null ? tran.Block.BlockId.ToString() : String.Empty),
                    HashPrevBlock = (tran.Block != null ? tran.Block.BlockHeader.HashPrevBlock.ToString() : String.Empty),
                    Height = (tran.Block != null ? tran.Block.Height : 0),
                    Nonce = (tran.Block != null ? tran.Block.BlockHeader.Nonce : 0),
                    Version = (tran.Block != null ? tran.Block.BlockHeader.Version : 0),
                    Difficulty = (tran.Block != null ? tran.Block.BlockHeader.Bits.Difficulty : 0),
                    Confirmations = (tran.Block != null ? tran.Block.Confirmations : 0),
                    BlockTime = (tran.Block != null ? tran.Block.BlockTime : new DateTime(1900, 1, 1)),
                    MedianTimePast = (tran.Block != null ? tran.Block.MedianTimePast : new DateTime(1900, 1, 1)),

                    TransactionId = tran.TransactionId.ToString(),
                    Fee = tran.Fees.ToUnit(MoneyUnit.BTC),
                    FirstSeen = tran.FirstSeen,

                    ReceivedCoins = tran.ReceivedCoins
                        .Select((obj, index) => new ReceivedCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                        .ToList(),

                    SpentCoins = tran.SpentCoins
                        .Select((obj, index) => new SpentCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                        .ToList()
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
