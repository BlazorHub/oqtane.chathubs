using Microsoft.JSInterop;
using Oqtane.ChatHubs.Client.Video;
using Oqtane.Modules;
using Oqtane.Services;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs
{
    public class VideoService : ServiceBase, IService, IVideoService
    {

        private JsRuntimeObjectRef _jsRuntimeObjectRef;
        public class JsRuntimeObjectRef
        {
            [JsonPropertyName("_jsObjectRefId")]
            public int JsObjectRefId { get; set; }
        }

        private readonly HttpClient HttpClient;
        private readonly IJSRuntime JSRuntime;

        public VideoService(HttpClient httpClient, IJSRuntime JSRuntime) : base(httpClient)
        {
            this.HttpClient = httpClient;
            this.JSRuntime = JSRuntime;
        }

        public async Task InitVideoJs()
        {
            this._jsRuntimeObjectRef = await this.JSRuntime.InvokeAsync<JsRuntimeObjectRef>("initvideojs");
        }

        public async Task GetUserMediaPermission(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("videostreams.getUserMediaPermission", roomId, _jsRuntimeObjectRef);
        }

        public async Task CaptureAudio(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("videostreams.captureAudio", roomId);
        }

        public async Task DrawImage(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("videostreams.drawImage", roomId, _jsRuntimeObjectRef);
        }

        public async Task<string> GetImageAsBase64String(int roomId)
        {
            return await this.JSRuntime.InvokeAsync<string>("videostreams.getImageAsBase64String", roomId, _jsRuntimeObjectRef);
        }

        public async Task StopVideo(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("videostreams.stopVideo", roomId, _jsRuntimeObjectRef);
        }

        public async Task SetImage(string image, int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("videostreams.setImage", image, roomId, _jsRuntimeObjectRef);
        }

    }
}
