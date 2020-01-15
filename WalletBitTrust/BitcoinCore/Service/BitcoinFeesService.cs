using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NBitcoinCore.Model;

namespace NBitcoinCore.Service
{
    public class BitcoinFeesService
    {
        public const string URL = "https://bitcoinfees.earn.com/api/v1/fees/recommended";

        public FeesRecommendedModel GetFeeRecommended()
        {
            try
            {
                var client = new RestClient(URL);
                var request = new RestRequest("fees/recommended", Method.GET);

                var response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<FeesRecommendedModel>(response.Content);
                }
                else
                {
                    return new FeesRecommendedModel();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public FeesListModel GetFeeList()
        {
            try
            {
                var client = new RestClient(URL);
                var request = new RestRequest("fees/list", Method.GET);

                var response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<FeesListModel>(response.Content);
                }
                else
                {
                    return new FeesListModel();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
