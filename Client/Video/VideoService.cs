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
            [JsonPropertyName("__jsObjectRefId")]
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
            this._jsRuntimeObjectRef = await this.JSRuntime.InvokeAsync<JsRuntimeObjectRef>("__initjsstreams");
        }

        public async Task StartVideo(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("___obj.startvideo", roomId, _jsRuntimeObjectRef);
        }

        public async Task<string> GetVideo(int roomId)
        {
            return await this.JSRuntime.InvokeAsync<string>("___obj.getvideo", roomId, _jsRuntimeObjectRef);
        }

        public async Task SetVideo(string base64, int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("___obj.setvideo", base64, roomId, _jsRuntimeObjectRef);
        }

        public async Task DrawImage(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("___obj.drawimage", roomId, _jsRuntimeObjectRef);
        }

        public async Task<string> GetImageAsBase64String(int roomId)
        {
            return await this.JSRuntime.InvokeAsync<string>("___obj.getimageasbase64string", roomId, _jsRuntimeObjectRef);
        }

        public async Task StopVideo(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("___obj.stopvideo", roomId, _jsRuntimeObjectRef);
        }

        public async Task SetImage(string image, int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("___obj.setimage", image, roomId, _jsRuntimeObjectRef);
        }

    }
}
