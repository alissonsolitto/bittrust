using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBitcoin;
using QBitNinja.Client;
using NBitcoinCore.Model;

namespace NBitcoinCore.Service
{
    public class AccountService
    {
        public object BalanceAddress(string address)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(address))
                {
                    return new Exception("Address inválido");
                }

                //Criando address BTC
                BitcoinAddress btcAddress = BitcoinAddress.Create(address, StaticConfig.Network);

                //Criando Client QBitNinjaClient
                var client = new QBitNinjaClient(StaticConfig.Network);
                Money SpentCoins = 0, ReceivedCoins = 0;

                //Percorre todas as transações do endereço
                client.GetBalance(btcAddress).Result.Operations.ToList().ForEach(x =>
                {
                    if (x.Amount > 0)
                        ReceivedCoins += x.Amount;
                    else
                        SpentCoins += x.Amount;
                });

                var result = new
                {
                    Received = ReceivedCoins.ToUnit(MoneyUnit.BTC),
                    Spent = SpentCoins.ToUnit(MoneyUnit.BTC),
                    Balance = (ReceivedCoins + SpentCoins).ToUnit(MoneyUnit.BTC)
                };

                return result;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        public object TransactionAll(string address)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(address))
                {
                    return new Exception("Address inválido");
                }

                //Criando address BTC
                BitcoinAddress btcAddress = BitcoinAddress.Create(address, StaticConfig.Network);

                //Criando Client QBitNinjaClient
                var client = new QBitNinjaClient(StaticConfig.Network);
                List<TransactionsAccountModel> lstTransctionAccount = new List<TransactionsAccountModel>();

                //Percorre todas as transações do endereço
                client.GetBalance(btcAddress).Result.Operations.ToList().ForEach(x =>
                {
                    lstTransctionAccount.Add(new TransactionsAccountModel
                    {
                        Amount = x.Amount.ToUnit(MoneyUnit.BTC),
                        BlockId = x.BlockId.ToString(),
                        Confirmations = x.Confirmations,
                        Height = x.Height,
                        TransactionId = x.TransactionId.ToString(),                        
                        FirstSeen = x.FirstSeen,

                        ReceivedCoins = x.ReceivedCoins
                        .Select((obj, index) => new ReceivedCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                        .ToList(),

                        SpentCoins = x.SpentCoins
                        .Select((obj, index) => new SpentCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                        .ToList()
                    });
                });

                return lstTransctionAccount;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }

        public object TransactionAllDetail(string address)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(address))
                {
                    return new Exception("Address inválido");
                }

                //Criando address BTC
                BitcoinAddress btcAddress = BitcoinAddress.Create(address, StaticConfig.Network);

                //Criando Client QBitNinjaClient
                var client = new QBitNinjaClient(StaticConfig.Network);
                List<TransactionsAccountModel> lstTransctionAccount = new List<TransactionsAccountModel>();

                //Percorre todas as transações do endereço
                client.GetBalance(btcAddress).Result.Operations.ToList().ForEach(x =>
                {
                    //Detalhes da transação
                    var tran = client.GetTransaction(x.TransactionId).Result;
                    
                    lstTransctionAccount.Add(new TransactionsAccountModel
                    {
                        Amount = x.Amount.ToUnit(MoneyUnit.BTC),
                        BlockId = x.BlockId.ToString(),
                        Confirmations = x.Confirmations,
                        Height = x.Height,                        
                        TransactionId = x.TransactionId.ToString(),
                        Fee = tran.Fees.ToUnit(MoneyUnit.BTC),
                        FirstSeen = x.FirstSeen,

                        ReceivedCoins = tran.ReceivedCoins
                        .Select((obj, index) => new ReceivedCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                        .ToList(),

                        SpentCoins = tran.SpentCoins
                        .Select((obj, index) => new SpentCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                        .ToList()
                    });
                });

                return lstTransctionAccount;
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }
        }
    }
}
