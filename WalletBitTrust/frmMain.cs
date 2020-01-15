using NBitcoin;
using NBitcoin.Policy;
using NBitcoinCore.Model;
using NBitcoinCore.Uteis;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WalletBitTrust
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        public static OpcodeType[] Segwit = new[]
        {
            OpcodeType.OP_0,
            OpcodeType.OP_1,
            OpcodeType.OP_2,
            OpcodeType.OP_3,
            OpcodeType.OP_4,
            OpcodeType.OP_5,
            OpcodeType.OP_6,
            OpcodeType.OP_7,
            OpcodeType.OP_8,
            OpcodeType.OP_9,
            OpcodeType.OP_10
        };

        /// <summary>
        /// Definicação da rede global, TesteNet or Main
        /// </summary>
        public static Network Network = Network.TestNet;

        /// <summary>
        /// Definicação Global de Fee
        /// </summary>
        public static decimal Fee = 10;

        private int SimulationTransaction(ICoin[] coin, IDestination[] btcAddressOut, byte[] hash, Key pKey)
        {
            TransactionBuilder txBuilder = Network.TestNet.CreateTransactionBuilder();
            txBuilder.StandardTransactionPolicy.MinRelayTxFee = FeeRate.Zero; // Politica que define a taxa minima de transacao
            txBuilder.DustPrevention = false;

            Transaction tran = txBuilder
                    // Adiciona sempre todas as saídas para normalizar em uma só (Melhor controle)
                    .AddCoins(coin)
                    // Chaves privadas para assinar a transação
                    .AddKeys(pKey)
                    // Endereços de saida []
                    .Send(btcAddressOut)
                    // OP_RETURN Hash do arquivo
                    .Send(TxNullDataTemplate.Instance.GenerateScriptPubKey(hash))
                    // Nova saída com a sobra da transação - Protocolo P2PKH
                    .SetChange(pKey.PubKey.Hash.ScriptPubKey)
                    // A Taxa é definida na aplicação
                    .SendFees(Money.Satoshis(1))
                    // Cria a transação e NÃO assina
                    .BuildTransaction(true);

            return tran.ToBytes().Length;
        }

        public void ConsoleText(string txt)
        {
            richText.AppendText($"{txt}\n\n");
        }

        public void ConsoleClear()
        {
            richText.Clear();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            ConsoleClear();

            Mnemonic mnemo = new Mnemonic(Wordlist.PortugueseBrazil, WordCount.Twelve);
            var pKey = mnemo.DeriveExtKey(txtSenhaKey.Text);

            ConsoleText($"[===== SEED =====]\n{mnemo.ToString()}");
            ConsoleText($"[===== ADDRESS =====]\n{pKey.PrivateKey.PubKey.GetAddress(Network.TestNet).ToString()}");
            ConsoleText($"[===== PUBLIC KEY =====]\n{pKey.PrivateKey.PubKey}");
            ConsoleText($"[===== BITCOIN SECRET =====]\n{pKey.PrivateKey.GetBitcoinSecret(Network.TestNet)}");
            ConsoleText($"[===== BITCOIN SECRET Encrypted =====]\n{pKey.PrivateKey.GetEncryptedBitcoinSecret(txtSenhaKey.Text, Network.TestNet)}");
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ConsoleClear();

            Mnemonic seed = new Mnemonic(txtSeedChaveRecovery.Text);
            var pKey = seed.DeriveExtKey(txtSenhaRecovery.Text);

            ConsoleText($"[===== SEED =====]\n{txtSeedChaveRecovery.Text}");
            ConsoleText($"[===== ADDRESS =====]\n{pKey.PrivateKey.PubKey.GetAddress(Network.TestNet).ToString()}");
            ConsoleText($"[===== PUBLIC KEY =====]\n{pKey.PrivateKey.PubKey}");
            ConsoleText($"[===== BITCOIN SECRET =====]\n{pKey.PrivateKey.GetBitcoinSecret(Network.TestNet)}");
            ConsoleText($"[===== BITCOIN SECRET Encrypted =====]\n{pKey.PrivateKey.GetEncryptedBitcoinSecret(txtSenhaKey.Text, Network.TestNet)}");
        }

        private void BtnBalance_Click(object sender, EventArgs e)
        {
            ConsoleClear();

            //Criando address BTC
            BitcoinAddress btcAddress = BitcoinAddress.Create(txtAddressBalance.Text, Network.TestNet);

            //Criando Client QBitNinjaClient
            var client = new QBitNinjaClient(Network.TestNet);
            Money SpentCoins = 0, ReceivedCoins = 0;

            //Percorre todas as transações do endereço
            client.GetBalance(btcAddress).Result.Operations.ToList().ForEach(x =>
            {
                if (x.Amount > 0)
                    ReceivedCoins += x.Amount;
                else
                    SpentCoins += x.Amount;
            });

            ConsoleText($"[===== Received =====]\n{ReceivedCoins.ToUnit(MoneyUnit.BTC)}");
            ConsoleText($"[===== Spent =====]\n{SpentCoins.ToUnit(MoneyUnit.BTC)}");
            ConsoleText($"[===== Balance =====]\n{(ReceivedCoins + SpentCoins).ToUnit(MoneyUnit.BTC)}");
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                ConsoleClear();

                //Recuperando chaves
                Mnemonic seed = new Mnemonic(textBox8.Text);
                var pKey = seed.DeriveExtKey(textBox2.Text);
                var btcAddress = pKey.PrivateKey.PubKey.GetAddress(Network.TestNet);

                //Abrir arquivo e criar hash
                OpenFileDialog dialog = new OpenFileDialog();
                StringBuilder result = new StringBuilder();

                dialog.Filter = "All Files | *.*";
                dialog.Multiselect = false;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    //File to bytes
                    String path = dialog.FileName;
                    byte[] bytesFile = File.ReadAllBytes(path);
                    byte[] hash = Functions.GenerateHashFile(bytesFile);
                    string hashBitTrust = Functions.ConvertStringToHex("[BITTRUST]" + Functions.HashToString(hash));
                    
                    ConsoleText($"[===== FILE HASH =====]\n{Functions.HashToString(hash)}");
                    ConsoleText($"[===== FILE HASH BITTRUST =====]\n{hashBitTrust}");
                    
                    /// Buscando as saídas não gastas do endereço que tenha valor maior que zero satoshis
                    List<ICoin> lstCoin = new List<ICoin>();
                    Money amount = Money.Zero;

                    var client = new QBitNinjaClient(Network.TestNet);
                    client.GetBalance(btcAddress, true).Result.Operations.ToList().ForEach(x =>
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

                            amount += x.Amount;
                        }
                    });

                    if (lstCoin.Count <= 0)
                    {
                        throw new Exception("O endereço não possui entradas confirmadas com valores maior que zero");
                    }

                    Coin[] coin = lstCoin.Select((o, i) => new Coin(o.Outpoint, o.TxOut)).ToArray();

                    /// Destinatários da transação
                    IDestination[] btcAddressOut = new IDestination[0];

                    /// Simulando a transação
                    /// Calculando fee por byte
                    decimal fee = SimulationTransaction(coin, btcAddressOut, Functions.HexStringToBytes(hashBitTrust), pKey.PrivateKey) * Fee;

                    /// Criando o objeto TransactionBuilder
                    TransactionBuilder txBuilder = Network.TestNet.CreateTransactionBuilder();
                    txBuilder.StandardTransactionPolicy.MinRelayTxFee = FeeRate.Zero; // Politica que define a taxa minima de transacao
                    txBuilder.DustPrevention = false; // False para permitir transações com poucos satoshis

                    Transaction tran = txBuilder
                        // Adiciona sempre todas as saídas para normalizar em uma só (Melhor controle)
                        .AddCoins(coin)
                        // Chaves privadas para assinar a transação
                        .AddKeys(pKey.PrivateKey)
                        // Endereços de saida []
                        .Send(btcAddressOut)
                        // OP_RETURN Hash do arquivo
                        .Send(TxNullDataTemplate.Instance.GenerateScriptPubKey(Functions.HexStringToBytes(hashBitTrust)))
                        // Nova entrada com a sobra da transação
                        .SetChange(pKey.PrivateKey.PubKey.Hash.ScriptPubKey) // Protocolo P2PKH
                                                                             // Adicionando as taxas, quando é simulação inclui apenas 1 satoshi
                        .SendFees(Money.Satoshis(fee))
                        // Cria a transação e assina
                        .BuildTransaction(true);

                    //Verificar se a transação possui erros
                    txBuilder.Verify(tran, out TransactionPolicyError[] errors);

                    if (errors.Length > 0)
                    {
                        throw new Exception(errors.ToString());
                    }
                    else
                    {
                        //Se não houver erros envia a transação para a rede
                        var clientBroadcast = new QBitNinjaClient(Network.TestNet);
                        BroadcastResponse broadcastResponse = clientBroadcast.Broadcast(tran).Result;

                        if (!broadcastResponse.Success)
                        {
                            throw new Exception(broadcastResponse.Error.Reason);
                        }
                        else
                        {
                            ConsoleText($"[===== TRANSACTION HASH =====]\n{tran.GetHash().ToString()}");
                            ConsoleText($"[===== Transação efetuada com sucesso! =====]\nhttps://live.blockcypher.com/btc-testnet/tx/{tran.GetHash().ToString()}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleText($"[===== ERROR =====]\n{ex.Message}");
            }
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            ConsoleClear();

            //Criando Client QBitNinjaClient
            var client = new QBitNinjaClient(Network.TestNet);

            //Percorre todas as transações do endereço
            var tran = client.GetTransaction(uint256.Parse(textBox3.Text)).Result;

            ConsoleText($"[===== BlockId =====]\n{(tran.Block != null ? tran.Block.BlockId.ToString() : String.Empty)}");

            ConsoleText($"[===== HashPrevBlock =====]\n{(tran.Block != null ? tran.Block.BlockHeader.HashPrevBlock.ToString() : String.Empty)}");

            ConsoleText($"[===== Height =====]\n{(tran.Block != null ? tran.Block.Height : 0)}");
            ConsoleText($"[===== Nonce =====]\n{(tran.Block != null ? tran.Block.BlockHeader.Nonce : 0)}");
            ConsoleText($"[===== Version =====]\n{(tran.Block != null ? tran.Block.BlockHeader.Version : 0)}");
            ConsoleText($"[===== Difficulty =====]\n{(tran.Block != null ? tran.Block.BlockHeader.Bits.Difficulty : 0)}");
            ConsoleText($"[===== Confirmations =====]\n{(tran.Block != null ? tran.Block.Confirmations : 0)}");
            ConsoleText($"[===== BlockTime =====]\n{(tran.Block != null ? tran.Block.BlockTime : new DateTime(1900, 1, 1))}");
            ConsoleText($"[===== MedianTimePast =====]\n{(tran.Block != null ? tran.Block.MedianTimePast : new DateTime(1900, 1, 1))}");

            ConsoleText($"[===== TransactionId =====]\n{tran.TransactionId.ToString()}");
            ConsoleText($"[===== Fee =====]\n{tran.Fees.ToUnit(MoneyUnit.BTC)}");
            ConsoleText($"[===== FirstSeen =====]\n{tran.FirstSeen}");


            ConsoleText($"[===== Outputs Created =====]");

            var receivedCoins = tran.ReceivedCoins
                    .Select((obj, index) => new ReceivedCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                    .ToList();

            foreach (var item in receivedCoins)
            {
                ConsoleText($"[===== Output =====]\n{receivedCoins.IndexOf(item)}");
                ConsoleText($"[===== Address =====]\n{item.Address}");
                ConsoleText($"[===== ScriptPubKey =====]\n{item.ScriptPubKey}");
                ConsoleText($"[===== Amount =====]\n{item.Amount}");
                ConsoleText($"[===== Message =====]\n{item.Message}");
            }

            ConsoleText($"[===== Input Consumed =====]");

            var spentCoins = tran.SpentCoins
                .Select((obj, index) => new SpentCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                .ToList();

            foreach (var item in spentCoins)
            {
                ConsoleText($"[===== Input =====]\n{spentCoins.IndexOf(item)}");
                ConsoleText($"[===== Address =====]\n{item.Address}");
                ConsoleText($"[===== ScriptPubKey =====]\n{item.ScriptPubKey}");
                ConsoleText($"[===== Amount =====]\n{item.Amount}");
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            ConsoleClear();

            //Criando address BTC
            BitcoinAddress btcAddress = BitcoinAddress.Create(textBox4.Text, Network.TestNet);

            //Criando Client QBitNinjaClient
            var client = new QBitNinjaClient(Network.TestNet);
            List<TransactionsAccountModel> lstTransctionAccount = new List<TransactionsAccountModel>();

            //Percorre todas as transações do endereço
            client.GetBalance(btcAddress).Result.Operations.ToList().ForEach(x =>
            {
                //Detalhes da transação
                var tran = client.GetTransaction(x.TransactionId).Result;

                ConsoleText($"[===== TransactionId =====]\n{tran.TransactionId}");

                ConsoleText($"[===== BlockId =====]\n{(tran.Block != null ? tran.Block.BlockId.ToString() : String.Empty)}");

                ConsoleText($"[===== HashPrevBlock =====]\n{(tran.Block != null ? tran.Block.BlockHeader.HashPrevBlock.ToString() : String.Empty)}");

                ConsoleText($"[===== Height =====]\n{(tran.Block != null ? tran.Block.Height : 0)}");
                ConsoleText($"[===== Nonce =====]\n{(tran.Block != null ? tran.Block.BlockHeader.Nonce : 0)}");
                ConsoleText($"[===== Version =====]\n{(tran.Block != null ? tran.Block.BlockHeader.Version : 0)}");
                ConsoleText($"[===== Difficulty =====]\n{(tran.Block != null ? tran.Block.BlockHeader.Bits.Difficulty : 0)}");
                ConsoleText($"[===== Confirmations =====]\n{(tran.Block != null ? tran.Block.Confirmations : 0)}");
                ConsoleText($"[===== BlockTime =====]\n{(tran.Block != null ? tran.Block.BlockTime : new DateTime(1900, 1, 1))}");
                ConsoleText($"[===== MedianTimePast =====]\n{(tran.Block != null ? tran.Block.MedianTimePast : new DateTime(1900, 1, 1))}");

                ConsoleText($"[===== Fee =====]\n{tran.Fees.ToUnit(MoneyUnit.BTC)}");
                ConsoleText($"[===== FirstSeen =====]\n{tran.FirstSeen}");

                ConsoleText($"[===== Outputs Created =====]");

                var receivedCoins = tran.ReceivedCoins
                        .Select((obj, index) => new ReceivedCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                        .ToList();

                foreach (var item in receivedCoins)
                {
                    ConsoleText($"[===== Output =====]\n{receivedCoins.IndexOf(item)}");
                    ConsoleText($"[===== Address =====]\n{item.Address}");
                    ConsoleText($"[===== ScriptPubKey =====]\n{item.ScriptPubKey}");
                    ConsoleText($"[===== Amount =====]\n{item.Amount}");
                    ConsoleText($"[===== Message =====]\n{item.Message}");
                }

                ConsoleText($"[===== Input Consumed =====]");

                var spentCoins = tran.SpentCoins
                    .Select((obj, index) => new SpentCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                    .ToList();

                foreach (var item in spentCoins)
                {
                    ConsoleText($"[===== Input =====]\n{spentCoins.IndexOf(item)}");
                    ConsoleText($"[===== Address =====]\n{item.Address}");
                    ConsoleText($"[===== ScriptPubKey =====]\n{item.ScriptPubKey}");
                    ConsoleText($"[===== Amount =====]\n{item.Amount}");
                }
            });
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            ConsoleClear();

            //Criando Client QBitNinjaClient
            var client = new QBitNinjaClient(Network.TestNet);

            //Percorre todas as transações do endereço
            var tran = client.GetTransaction(uint256.Parse(textBox5.Text)).Result;

            var receivedCoins = tran.ReceivedCoins
                        .Select((obj, index) => new ReceivedCoinsAccount(obj.TxOut.Value, obj.TxOut.ScriptPubKey))
                        .ToList();

            var hashOpReturn = receivedCoins.Where(r => r.Message != null).FirstOrDefault().Message;
            //foreach (var item in receivedCoins.Where(r => !String.IsNullOrEmpty(r.Message)))
            //{
            //    if(item.Message.Length > 0)
            //    {
            //        hashOpReturn = item.Message;
            //        break;
            //    }
            //}

            ConsoleText($"[===== Message HEX =====]\n{hashOpReturn}");

            //Abrir arquivo e criar hash
            OpenFileDialog dialog = new OpenFileDialog();
            StringBuilder result = new StringBuilder();

            dialog.Filter = "All Files | *.*";
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                //File to bytes
                String path = dialog.FileName;
                byte[] bytesFile = File.ReadAllBytes(path);
                byte[] hash = Functions.GenerateHashFile(bytesFile);

                //Validando HASH                
                var hashOpReturnString = Functions.HexToUTF8(hashOpReturn.ToString());
                ConsoleText($"[===== Message String =====]\n{hashOpReturnString}");

                hashOpReturnString = hashOpReturnString.Substring(10); //Removendo [BITTRUST]
                ConsoleText($"[===== HASH =====]\n{hashOpReturnString}");
                ConsoleText($"[===== HASH FILE =====]\n{Functions.HashToString(hash)}");

                if (Functions.HashToString(hash) == hashOpReturnString)
                {
                    ConsoleText($"[===== AUTHENTIC FILE =====]\n\n");
                    ConsoleText($"[===== BLOCK HEIGHT =====]\n{tran.Block.Height}");
                    ConsoleText($"[===== DATE TIME =====]\n{tran.Block.BlockTime}");
                    ConsoleText($"[===== CONFIRMATIONS =====]\n{tran.Block.Confirmations}");
                }
                else
                {
                    ConsoleText($"[===== FILE DOES NOT CHECK OR WAS CHANGED =====]");
                }
            }            
        }
    }
}
