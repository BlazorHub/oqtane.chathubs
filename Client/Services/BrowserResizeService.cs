using Microsoft.JSInterop;
using Oqtane.Modules;
using Oqtane.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Oqtane.ChatHubs.Services
{
    public class BrowserResizeService : ServiceBase, IService
    {
        private readonly IJSRuntime JSRuntime;

        public static event Func<Task> OnResize;

        public BrowserResizeService(HttpClient http, IJSRuntime JSRuntime) : base(http)
        {
            this.JSRuntime = JSRuntime;
        }

        [JSInvokable("OnBrowserResize")]
        public static async Task OnBrowserResize()
        {
            await OnResize?.Invoke();
        }

        public async Task<int> GetInnerHeight()
        {
            return await this.JSRuntime.InvokeAsync<int>("browserResize.getInnerHeight");
        }

        public async Task<int> GetInnerWidth()
        {
            return await this.JSRuntime.InvokeAsync<int>("browserResize.getInnerWidth");
        }
    }
}