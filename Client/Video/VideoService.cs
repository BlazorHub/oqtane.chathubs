using Microsoft.JSInterop;
using Oqtane.ChatHubs.Client.Video;
using Oqtane.Modules;
using Oqtane.Services;
using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs
{
    public class VideoService : ServiceBase, IService, IVideoService
    {

        private JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        public class JsRuntimeObjectRef
        {
            [JsonPropertyName("__jsObjectRefId")]
            public int JsObjectRefId { get; set; }
        }

        private readonly HttpClient HttpClient;
        private readonly IJSRuntime JSRuntime;

        public event EventHandler<dynamic> OnDataAvailableEventHandler;

        public VideoService(HttpClient httpClient, IJSRuntime JSRuntime) : base(httpClient)
        {
            this.HttpClient = httpClient;
            this.JSRuntime = JSRuntime;
        }

        public async Task InitVideoJs()
        {
            this.__jsRuntimeObjectRef = await this.JSRuntime.InvokeAsync<JsRuntimeObjectRef>("__initjsstreams", DotNetObjectReference.Create(this));
        }

        [JSInvokable("OnDataAvailable")]
        public object OnDataAvailable(string dataURI, int roomId, string dataType)
        {
            OnDataAvailableEventHandler?.Invoke(typeof(VideoService), new { dataURI = dataURI, roomId = roomId, dataType = dataType });
            return new { msg = string.Format("room id: {1}; dataType: {2}; dataUri: {0}", dataURI, roomId, dataType) };
        }

        public async Task StartBroadcasting(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.startbroadcasting", roomId, __jsRuntimeObjectRef);
            }
        }

        public async Task StartStreaming(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.startstreaming", roomId, __jsRuntimeObjectRef);
            }
        }

        public async Task CloseLivestream(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.closelivestream", roomId, __jsRuntimeObjectRef);
            }
        }

        public async Task StartSequence(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.startsequence", roomId, __jsRuntimeObjectRef);
            }
        }

        public async Task StopSequence(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.stopsequence", roomId, __jsRuntimeObjectRef);
            }
        }

        public async Task DrawImage(int roomId)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.drawimage", roomId, __jsRuntimeObjectRef);
            }
        }

        public async Task AppendBuffer(string dataURI, int roomId, string dataType)
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.appendbuffer", dataURI, roomId, dataType, __jsRuntimeObjectRef);
            }
        }

    }
}
