using Microsoft.JSInterop;
using Oqtane.ChatHubs.Client.Video;
using Oqtane.Modules;
using Oqtane.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs
{
    public class VideoService : ServiceBase, IService, IVideoService
    {

        private readonly HttpClient HttpClient;
        private readonly IJSRuntime JSRuntime;

        public VideoService(HttpClient httpClient, IJSRuntime JSRuntime) : base(httpClient)
        {
            this.HttpClient = httpClient;
            this.JSRuntime = JSRuntime;
        }

        public async Task GetUserMediaPermission(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("video.getUserMediaPermission", roomId);
        }

        public async Task CaptureAudio(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("video.captureAudio", roomId);
        }

        public async Task DrawImage(int roomId)
        {
            await this.JSRuntime.InvokeVoidAsync("video.drawImage", roomId);
        }

        public async Task<byte[]> GetImageAsBase64String(int roomId)
        {
            var imageAsBase64String = await this.JSRuntime.InvokeAsync<string>("video.getImageAsBase64String", roomId);
            byte[] bytes = Convert.FromBase64String(imageAsBase64String);
            return bytes;
        }

        public async Task SetStream(byte[] stream, int roomId)
        {
            var imageAsBase64String = Convert.ToBase64String(stream);
            await this.JSRuntime.InvokeVoidAsync("video.setImage", imageAsBase64String, roomId);
        }

    }
}
