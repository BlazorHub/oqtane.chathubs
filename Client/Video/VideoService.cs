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

        private JsRuntimeObjectRef _jsRuntimeObjectRef { get; set; }

        public class JsRuntimeObjectRef
        {
            [JsonPropertyName("__jsObjectRefId")]
            public int JsObjectRefId { get; set; }
        }

        private readonly HttpClient HttpClient;
        private readonly IJSRuntime JSRuntime;

        public static event EventHandler<dynamic> OnDataAvailableEventHandler;

        public VideoService(HttpClient httpClient, IJSRuntime JSRuntime) : base(httpClient)
        {
            this.HttpClient = httpClient;
            this.JSRuntime = JSRuntime;
        }

        public async Task InitVideoJs()
        {
            this._jsRuntimeObjectRef = await this.JSRuntime.InvokeAsync<JsRuntimeObjectRef>("__initjsstreams");
        }

        [JSInvokable("OnDataAvailable")]
        public static object OnDataAvailable(string dataURI, int roomId, string dataType)
        {
            OnDataAvailableEventHandler?.Invoke(typeof(VideoService), new { dataURI = dataURI, roomId = roomId, dataType = dataType });
            return new { msg = "room id: " + roomId + "; dataType: " + dataType + "; data: " + dataURI };
        }

        public async Task StartBroadcasting(int roomId)
        {
            if (this._jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.startbroadcasting", roomId, _jsRuntimeObjectRef);
            }
        }

        public async Task StartStreaming(int roomId)
        {
            if (this._jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.startstreaming", roomId, _jsRuntimeObjectRef);
            }
        }

        public async Task CloseLivestream(int roomId)
        {
            if (this._jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.closelivestream", roomId, _jsRuntimeObjectRef);
            }
        }

        public async Task DrawImage(int roomId)
        {
            if (this._jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.drawimage", roomId, _jsRuntimeObjectRef);
            }
        }

        public async Task AppendBuffer(string dataURI, int roomId, string dataType)
        {
            if (this._jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.appendbuffer", dataURI, roomId, dataType, _jsRuntimeObjectRef);
            }
        }

    }
}
