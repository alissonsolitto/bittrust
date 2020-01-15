using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBitcoinCore.Uteis;

namespace NBitcoinCore.Model
{

    public class ReceivedCoinsAccount
    {
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public string ScriptPubKey { get; set; }
        public string Message { get; set; }

        public ReceivedCoinsAccount(Money Amount, Script ScriptPubKey)
        {
            var StaticConfig = new StaticConfig();
            ///MELHORAR ESSA LOGICA DE VALIDAÇÃO

            this.Amount = Amount.ToUnit(MoneyUnit.BTC);
            this.ScriptPubKey = ScriptPubKey.ToString();

            //OP_RETURN
            if (ScriptPubKey.ToOps().ToList().Find(x => x.Code == OpcodeType.OP_RETURN) != null)
            {
                //OP_RETURN 53656e642061646472657373206d756c747369676e20736567776974
                this.Message = ScriptPubKey.ToString().Substring(10);
            }
            //Segwit
            //ScriptPubKey.IsWitness
            else if (ScriptPubKey.ToOps().ToList().Find(x => StaticConfig.Segwit.Contains(x.Code)) != null)
            {
                this.Address = ScriptPubKey.Hash.GetAddress(StaticConfig.Network).ToString();
            }            
            else
            {
                //Pagamento para uma chave publica
                if(ScriptPubKey.GetDestinationPublicKeys().Length == 1)
                {
                    foreach (var item in ScriptPubKey.GetDestinationPublicKeys())
                    {
                        this.Address = item.GetAddress(StaticConfig.Network).ToString();
                    }
                }
                else
                {
                    this.Address = ScriptPubKey.GetDestinationAddress(StaticConfig.Network).ToString();
                }                
            }
        }
    }

    public class SpentCoinsAccount
    {
        public decimal Amount { get; set; }
        public string Address { get; set; }
        public string ScriptPubKey { get; set; }

        public SpentCoinsAccount(Money Amount, Script ScriptPubKey)
        {
            var StaticConfig = new StaticConfig();

            this.Amount = Amount.ToUnit(MoneyUnit.BTC);
            this.ScriptPubKey = ScriptPubKey.ToString();

            //Segwit
            if (ScriptPubKey.ToOps().ToList().Find(x => StaticConfig.Segwit.Contains(x.Code)) != null)
            {
                this.Address = ScriptPubKey.Hash.GetAddress(StaticConfig.Network).ToString();
            }
            else
            {
                //Pagamento para uma chave publica
                if (ScriptPubKey.GetDestinationPublicKeys().Length == 1)
                {
                    foreach (var item in ScriptPubKey.GetDestinationPublicKeys())
                    {
                        this.Address = item.GetAddress(StaticConfig.Network).ToString();
                    }
                }
                else
                {
                    this.Address = ScriptPubKey.GetDestinationAddress(StaticConfig.Network).ToString();
                }                
            }
        }
    }

    public class TransactionsAccountModel
    {
        public decimal Amount { get; set; }
        public string BlockId { get; set; }
        public int Confirmations { get; set; }
        public int Height { get; set; }        
        public string TransactionId { get; set; }
        public decimal Fee { get; set; }
        public List<ReceivedCoinsAccount> ReceivedCoins { get; set; }
        public List<SpentCoinsAccount> SpentCoins { get; set; }
        public DateTimeOffset FirstSeen { get; set; }

        public TransactionsAccountModel()
        {
            ReceivedCoins = new List<ReceivedCoinsAccount>();
            SpentCoins = new List<SpentCoinsAccount>();
        }
    }
    
}
