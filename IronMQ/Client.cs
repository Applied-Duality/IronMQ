#region Apache 2 License
// Copyright (c) Applied Duality, Inc., All rights reserved.
// See License.txt in the project root for license information.
#endregion

using System.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Threading.Tasks;

// http://pfelix.wordpress.com/2012/01/11/the-new-net-httpclient-class/

namespace System.Reactive.IronMQ
{
    public class Client 
    {
        HttpClient _client;

        public Client(string applicationID, string oauthToken, Cloud cloud = Cloud.AWS) : base()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(string.Format("https://{0}.iron.io/1/projects/{1}/", cloud.toString(), applicationID));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", oauthToken);
        }

        /// <summary>
        /// Get existing queue or create a new queue if it does not exist yet.
        /// If no name is given, a new name is generated.
        /// </summary>
        public async Task<Queue> CreateOrGetQueueAsync(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name)) name = Guid.NewGuid().ToString();
            var content = new JsonContent("{}");
            var response = await _client.PostAsync(string.Format("queues/{0}", name), content);
            return new Queue(this._client, name);
        }

        /// <summary>
        /// Get a list of all queues in a project. 
        /// By default, 30 queues are fetched at a time.
        /// Up to 100 queues may be listed on a single page.
        /// </summary>
        public IObservable<Queue> ListQueues(int startPage = 0, int pageSize = 30)
        {
            return Observable.Create<Queue>(async (observer, cancel) =>
            {
                var page = startPage;
            Next:
                var response = await _client.GetStringAsync(string.Format("queues?page={0}", page++));
                var array = JsonArray.Parse(response) as JsonArray;
                foreach (var info in array) observer.OnNext(new Queue(this._client, new _QueueInfo(info).Name));
                if (array.Count == pageSize && !cancel.IsCancellationRequested) goto Next;
                observer.OnCompleted();
            });
        }

    }

}
