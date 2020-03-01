using IVySoft.VPlatform.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IVySoft.VDS.Client
{
    internal class VdsApiClient : IDisposable
    {
        private readonly WebSocketPipeline ws_ = new WebSocketPipeline();
        private int message_id_;
        private readonly Dictionary<int, Action<int, JToken>> subscriptions_ = new Dictionary<int, Action<int, JToken>>();
        private readonly Dictionary<int, TaskCompletionSource<JToken>> action_handlers_ = new Dictionary<int, TaskCompletionSource<JToken>>();


        public Action<Exception> ErrorHandler { get; set; }

        public async Task Connect(VdsApiClientOptions options)
        {
            using (var cts = new CancellationTokenSource(options.ConnectionTimeout))
            {
                var ws = new ClientWebSocket();
                await ws.ConnectAsync(options.ServiceUri, cts.Token);
                this.ws_.start(
                    ws,
                    options.SendTimeout,
                    x => this.input_handler(x),
                    y => this.on_error(y));
            }
        }

        private async Task input_handler(string body)
        {
            var result = JsonConvert.DeserializeObject<WebsocketResponse>(body);

            TaskCompletionSource<JToken> source = null;
            Action<int, JToken> subscription = null;
            lock (this)
            {
                if (this.action_handlers_.TryGetValue(result.id, out source))
                {
                    this.action_handlers_.Remove(result.id);
                }
                else if (!this.subscriptions_.TryGetValue(result.id, out subscription))
                {
                    throw new ArgumentException();
                }
            }

            if (null != source)
            {
                ThreadPool.QueueUserWorkItem((x) =>
                {
                    if (string.IsNullOrWhiteSpace(result.error))
                    {
                        source.SetResult(result.result);
                    }
                    else
                    {
                        source.SetException(new Exception(result.error));
                    }
                });
            }
            else
            {
                ThreadPool.QueueUserWorkItem((x) => subscription(result.id, result.result));
            }
        }

        private void on_error(Exception ex)
        {
            lock (this)
            {
                foreach (var handler in this.action_handlers_)
                {
                    handler.Value.SetException(ex);
                }

                this.action_handlers_.Clear();
            }

            if (null != this.ErrorHandler)
            {
                this.ErrorHandler(ex);
            }
        }

        public async Task<T> call<T>(
            string methodToCall,
            params object[] callParameters)
        {
            var source = new TaskCompletionSource<JToken>();

            int id;
            lock (this)
            {
                id = ++this.message_id_;
                this.action_handlers_.Add(id, source);
            }

            this.ws_.enqueue(Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(
                    new WebsocketRequest
                    {
                        id = id,
                        invoke = methodToCall,
                        @params = callParameters
                    }, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })));

            var result = await source.Task;
            return result.ToObject<T>();
        }

        public void Dispose()
        {
            if (null != this.ws_)
            {
                this.ws_.Dispose();
            }
        }
    }
}
