using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using System.Linq;

namespace MagzineStore
{
    public class TokenDetails
    {
        public string success { get; set; }
        public string token { get; set; }
    }

    public class SubscribersDetails
    {
        public List<Subscribers> data { get; set; }
    }

    public class Categories
    {
        public List<string> data { get; set; }
    }
    public class Subscribers
    {
        public Guid id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public List<Int32> magazineIds { get; set; }
    }

    public class MagzineDetails
    {
        public List<Magzines> data { get; set; }
    }
    public class Magzines
    {
        public int id { get; set; }
        public string name { get; set; }
        public string category { get; set; }
    }

    public class ResultSubscribers
    {
        public List<Guid> subscribers { get; set; }
    }

    public class ServiceDetails
    {
       
        private string getTokenUrl = "http://magazinestore.azurewebsites.net/api/token";
        public string tokendata=string.Empty;
        public ServiceDetails()
        {
            RestClient restClient = new RestClient(getTokenUrl);
            RestRequest restRequest = new RestRequest();
            var response = restClient.Get(restRequest);

            TokenDetails tokenDetails = JsonConvert.DeserializeObject<TokenDetails>(response.Content);

            tokendata = tokenDetails.token;
        }

        
        

        //Get Subscribers

        public SubscribersDetails GetSubscribers()
        {
            string token = tokendata;

            string SubscriberRequest = "http://magazinestore.azurewebsites.net/api/subscribers/";

           var getSubscriberUrl= Path.Combine(SubscriberRequest, token);
            RestClient restClient = new RestClient(getSubscriberUrl);
            RestRequest restRequest = new RestRequest();
            var response = restClient.Get(restRequest);

             return JsonConvert.DeserializeObject<SubscribersDetails>(response.Content);


        }

        //Get Caetgories
        public Categories GetCategories()
        {

            string token = tokendata;

            string CategorieRequest = "http://magazinestore.azurewebsites.net/api/categories/";



            var getCategorieURL = Path.Combine(CategorieRequest, token);

            RestClient restClient = new RestClient(getCategorieURL);
            RestRequest restRequest = new RestRequest();
            var response = restClient.Get(restRequest);

           // SubscribersDetails data = JsonConvert.DeserializeObject<SubscribersDetails>(response.Content);


             return JsonConvert.DeserializeObject<Categories>(response.Content);
        }

        // Get Magzines Id by Category Name
        public MagzineDetails GetMagzinesByCategory(string Category)
        
      {
            string token = tokendata;

            string MagzinesRequest = "http://magazinestore.azurewebsites.net/api/magazines/";




        var getMagzinesURL = Path.Combine(MagzinesRequest,token,Category);

        RestClient restClient = new RestClient(getMagzinesURL);
        RestRequest restRequest = new RestRequest();
        var response = restClient.Get(restRequest);

        // SubscribersDetails data = JsonConvert.DeserializeObject<SubscribersDetails>(response.Content);


       return JsonConvert.DeserializeObject<MagzineDetails>(response.Content);
    
        
      }

        // Returns subscriber id's of subscribed atleast one magzine in  each category 
        public List<Guid> FinalResult()
        {
            SubscribersDetails sb = GetSubscribers();
            Categories cb = GetCategories();
            MagzineDetails mb = null;

            int totalSubscribers = sb.data.Count;
            int toalCategories = cb.data.Count;

            int resultCount = 0;
            List<Guid> subscriberList = new List<Guid>();

            for (int i = 0; i < totalSubscribers; i++)
            {
                resultCount = 0;

                for (int j = 0; j < toalCategories; j++)
                {
                    //sb.data[i].magazineIds;

                    mb = GetMagzinesByCategory(cb.data[j]);
                    if (mb.data != null)
                    {
                        List<int> termsList = new List<int>();
                        for (int k = 0; k < mb.data.Count; k++)
                        {
                            termsList.Add(mb.data[k].id);
                        }
                        
                        List<int> id = sb.data[i].magazineIds;

                        if (sb.data[i].magazineIds.Where(x => termsList.Contains(x)).Count() > 0)
                        {
                            resultCount = resultCount + 1;

                        }
                    }

                    if (resultCount == toalCategories)
                    {
                        subscriberList.Add(GetSubscribers().data[i].id);
                        resultCount = 0;
                    }
                }

                

            }

            return subscriberList;
        }

        //Submit Answer
        public void SubmitAnswer()
        {
                ResultSubscribers resultSubscribers = new ResultSubscribers();
            
                resultSubscribers.subscribers = FinalResult();

            string token = tokendata;
            string SubscriberRequest = "http://magazinestore.azurewebsites.net/api/answer/";
            var getSubscriberUrl = Path.Combine(SubscriberRequest, token);
           
            var client = new RestClient(getSubscriberUrl);
            var request = new RestRequest(Method.POST);
            string jsonRequest = JsonConvert.SerializeObject(resultSubscribers);
            request.AddParameter(
                              "application/json; charset=utf-8",
                              jsonRequest,
                              ParameterType.RequestBody
                              );
            request.AddJsonBody(jsonRequest);
            var response = client.Execute(request);
            response.Content.ToString();
            Console.WriteLine(response.Content.ToString());

            


        }


    }
}
