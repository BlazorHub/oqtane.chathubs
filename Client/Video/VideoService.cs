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
        public static object OnDataAvailable(string item, int roomId)
        {
            OnDataAvailableEventHandler?.Invoke(typeof(VideoService), new { item = item, roomId = roomId });
            return new { msg = "room id: " + roomId + "; data: " + item };
        }

        public async Task StartVideo(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("__obj.startvideo", roomId, _jsRuntimeObjectRef);
        }

        public async Task CloseLivestream(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("__obj.closelivestream", roomId, _jsRuntimeObjectRef);
        }

        public async Task AppendBuffer(string item, int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("__obj.appendbuffer", item, roomId, _jsRuntimeObjectRef);
        }

    }
}
