using Microsoft.JSInterop;
using Oqtane.Shared.Models;
using System;
using System.Threading.Tasks;

namespace BlazorDraggableList
{
    public class BlazorDraggableListService : IBlazorDraggableListService
    {

        private readonly IJSRuntime JSRuntime;

        public JsRuntimeObjectRef __jsRuntimeObjectRef { get; set; }

        public BlazorDraggableListServiceExtension BlazorDraggableListServiceExtension;

        public DotNetObjectReference<BlazorDraggableListServiceExtension> dotNetObjectReference;

        public Type TItemGenericType { get; set; }

        public BlazorDraggableListService(IJSRuntime jsRuntime)
        {
            this.JSRuntime = jsRuntime;

            this.BlazorDraggableListServiceExtension = new BlazorDraggableListServiceExtension(this);
            this.dotNetObjectReference = DotNetObjectReference.Create(this.BlazorDraggableListServiceExtension);
        }

        public async Task InitEventListeners()
        {
            if (this.__jsRuntimeObjectRef != null)
            {
                await this.JSRuntime.InvokeVoidAsync("__obj.initeventlisteners", __jsRuntimeObjectRef);
            }
        }
    }

    public class BlazorDraggableListServiceExtension
    {

        private BlazorDraggableListService BlazorDraggableListService { get; set; }

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
                BlazorDraggableListEvent eventParameters = new BlazorDraggableListEvent() { DraggableItemOldIndex = oldIndex, DraggableItemNewIndex = newIndex, TItemGenericType = this.BlazorDraggableListService.TItemGenericType };
                OnDropEvent?.Invoke(this, eventParameters);
            }
        }

    }
}
