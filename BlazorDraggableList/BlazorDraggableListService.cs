using Microsoft.JSInterop;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorDraggableList
{
    public class BlazorDraggableListService : IBlazorDraggableListService
    {

        private readonly IJSRuntime JSRuntime;

        private JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        public class JsRuntimeObjectRef
        {
            [JsonPropertyName("__jsObjectRefId")]
            public int JsObjectRefId { get; set; }
        }

        public BlazorDraggableListServiceExtension BlazorDraggableListServiceExtension;

        private DotNetObjectReference<BlazorDraggableListServiceExtension> dotNetObjectReference;

        public BlazorDraggableListService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;
            this.BlazorDraggableListServiceExtension = new BlazorDraggableListServiceExtension(this);
        }

        public async Task InitDraggableJs()
        {
            this.dotNetObjectReference = DotNetObjectReference.Create(this.BlazorDraggableListServiceExtension);
            this.__jsRuntimeObjectRef = await this.JSRuntime.InvokeAsync<JsRuntimeObjectRef>("__initdraggablejs", this.__jsRuntimeObjectRef);
        }

        public async Task InitWindowEventListeners()
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.initevents", __jsRuntimeObjectRef);
            }
        }
    }

    public class BlazorDraggableListServiceExtension
    {

        private BlazorDraggableListService BlazorDraggableListService;

        public event EventHandler<BlazorDraggableListEvent> OnDropEvent;

        public BlazorDraggableListServiceExtension(BlazorDraggableListService blazorDraggableListService)
        {
            this.BlazorDraggableListService = blazorDraggableListService;
        }

        [JSInvokable("OnDrop")]
        public void OnDrop(int oldIndex, int newIndex)
        {
            if (oldIndex >= 0 && newIndex >= 0)
            {
                BlazorDraggableListEvent eventParameters = new BlazorDraggableListEvent() { DraggableItemOldIndex = oldIndex, DraggableItemNewIndex = newIndex };
                OnDropEvent?.Invoke(this, eventParameters);
            }
        }

    }

}
