using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinCore
{
    public class StaticConfig
    {
        /// <summary>
        /// Tipos de OpCode que estão presentes na assinatura SegWit
        /// </summary>
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
    }
}
