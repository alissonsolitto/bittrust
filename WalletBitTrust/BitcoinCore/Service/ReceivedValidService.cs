using NBitcoin;
using QBitNinja.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinCore.Service
{
    public class ReceivedValidService
    {
        public List<ICoin> LstReceivedValid(IDestination destination)
        {
            List<ICoin> lstCoin = new List<ICoin>();

            var client = new QBitNinjaClient(Network.TestNet);
            client.GetBalance(destination, true).Result.Operations.ToList().ForEach(x =>
            {
                //A transação precisa estar no bloco, mais de zero confirmações
                if (x.Confirmations > 0)
                {
                    //Busca as moedas recebidas da transação
                    x.ReceivedCoins.ForEach(r =>
                    {
                        if (((Money)r.Amount).Satoshi > 0)
                        {
                            lstCoin.Add(new Coin()
                            {
                                Outpoint = r.Outpoint,
                                TxOut = r.TxOut
                            });
                        }
                    });
                }
            });

            return lstCoin;
        }

        public List<ICoin> LstReceivedValid(IDestination destination, Money value)
        {
            List<ICoin> lstCoin = new List<ICoin>();
            Money amount = Money.Zero;

            var client = new QBitNinjaClient(Network.TestNet);
            client.GetBalance(destination, true).Result.Operations.ToList().ForEach(x =>
            {
                //A transação precisa estar no bloco, mais de zero confirmações
                if (x.Confirmations > 0)
                {
                    if (amount <= value)
                    {
                        //Busca as moedas recebidas da transação
                        x.ReceivedCoins.ForEach(r =>
                        {
                            if (((Money)r.Amount).Satoshi > 0)
                            {
                                lstCoin.Add(new Coin()
                                {
                                    Outpoint = r.Outpoint,
                                    TxOut = r.TxOut
                                });
                            }
                        });
                    }

                    amount += x.Amount;
                }
            });

            return lstCoin;
        }
    }
}
