using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBitcoinCore.Model
{
    public class BroadcastParamModel
    {
        public string URL { get; set; }
        public string RequestBody { get; set; }

        public BroadcastParamModel(string URL, string RequestBody)
        {
            this.URL = URL;
            this.RequestBody = RequestBody;
        }
    }

    public class BroadcastModel
    {
        public List<BroadcastParamModel> lstUrlBroadcast;

        public BroadcastModel()
        {
            lstUrlBroadcast = new List<BroadcastParamModel>();
            ImportListAPI();
        }

        ///// <summary>
        ///// Lista de URLS para fazer Broadcast
        ///// https://bitcointalk.org/index.php?topic=2172803.0        
        ///// </summary>
        public void ImportListAPI()
        {
            lstUrlBroadcast.Add(new BroadcastParamModel(
                "https://api.smartbit.com.au/v1/blockchain/pushtx",
                "{\"hex\": \"#TransactionRaw\"}"
                ));

            lstUrlBroadcast.Add(new BroadcastParamModel(
                "https://api.blockcypher.com/v1/bcy/test/txs/push",
                "{\"tx\": \"#TransactionRaw\"}"
                ));

            lstUrlBroadcast.Add(new BroadcastParamModel(
                "http://webbtc.com/relay_tx.json",
                "{\"wait\":10,\"tx\":\"#TransactionRaw\"}"
                ));
        }
    }
}
