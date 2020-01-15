using NBitcoin;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBitcoinCore.Model;

namespace NBitcoinCore.Service
{
    public class BroadcastService
    {
        public bool BroadcastTransaction(Transaction tran)
        {
            try
            {
                var lstUrlBroadcast = new BroadcastModel().lstUrlBroadcast;
                bool broadcastNetwork = false;

                foreach (var item in lstUrlBroadcast)
                {
                    var client = new RestClient(item.URL);
                    var request = new RestRequest(Method.POST);

                    var param = item.RequestBody.Replace("#TransactionRaw", tran.ToHex());

                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", param, ParameterType.RequestBody);

                    var response = client.Execute(request);

                    if (response.IsSuccessful)
                    {
                        broadcastNetwork = true;
                        break;
                    }
                }

                return broadcastNetwork;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
