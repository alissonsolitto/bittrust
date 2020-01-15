using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinCore.Parameters
{
    public class TransactionSingleParam
    {
        /// <summary>
        /// Caso seja simulação não faz o broadcast da transação e 
        /// retorna apenas o tamanho da transação em Bytes
        /// </summary>
        public bool Simulation { get; set; }
        /// <summary>
        /// Endereço de origem
        /// </summary>
        public string AddressOrigin { get; set; }        
        /// <summary>
        /// Chave secreta do bitcoin para assinar a transação
        /// </summary>
        public string BitcoinSecret { get; set; }
        /// <summary>
        /// Senha para descriptografar a chave secreta
        /// </summary>
        public string PasswordSecret { get; set; }
        /// <summary>
        /// Endereço de destino da transação
        /// </summary>
        public List<string> AddressOut { get; set; }
        /// <summary>
        /// Hash do arquivo
        /// </summary>
        public string Hashfile { get; set; }
        /// <summary>
        /// Fee da transação em satoshis
        /// </summary>
        public decimal TxFee { get; set; }
    }

    public class RegisterFileParam
    {
        /// <summary>
        /// Endereço de origem
        /// </summary>
        public string AddressOrigin { get; set; }
        /// <summary>
        /// Chave secreta do bitcoin para assinar a transação
        /// </summary>
        public List<string> AddressOut { get; set; }
        // <summary>
        /// Arquivo para registros
        /// </summary>
        public string FileB64 { get; set; }
    }

    public class BroadcastParam
    {
        /// <summary>
        /// Transação assinada
        /// </summary>
        public string Transaction { get; set; }        
    }


    //public class TransactionParam
    //{
    //    /// <summary>
    //    /// Chaves públicas do endereço de entrada
    //    /// </summary>
    //    public List<string> PubKey { get; set; }
    //    /// <summary>
    //    /// Quantidade de chaves que precisam assinar a transação para gastar os fundos, PubKey > 1
    //    /// </summary>
    //    public int SigCount { get; set; }
    //    /// <summary>
    //    /// Chaves privadas para assinar o endereço de entrada
    //    /// </summary>
    //    public List<string> SignKey { get; set; }
    //    /// <summary>
    //    /// Chaves privadas para assinar o endereço de entrada
    //    /// </summary>
    //    public List<string> PasswordSignKey { get; set; }
    //    /// <summary>
    //    /// Endereços para saída
    //    /// </summary>
    //    public List<string> Out { get; set; }
    //    /// <summary>
    //    /// Byte do arquivo para gerar o hash na Blockchain
    //    /// </summary>
    //    public byte[] File { get; set; }
    //    /// <summary>
    //    /// 0 - hourFee
    //    /// 1 - halfHourFee
    //    /// 2 - fastestFee
    //    /// </summary>
    //    public int TxFee { get; set; }
    //}
}
