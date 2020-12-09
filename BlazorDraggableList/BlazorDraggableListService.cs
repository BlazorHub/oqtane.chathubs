using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace BlazorDraggableList
{
    public class BlazorDraggableListService : IBlazorDraggableListService
    {

        private readonly IJSRuntime JSRuntime;

        public static event EventHandler<BlazorDraggableListEvent> OnDropEvent;

        public BlazorDraggableListService(IJSRuntime JSRuntime)
        {
            this.JSRuntime = JSRuntime;
        }

        [JSInvokable("OnDrop")]
        public static void OnDrop(int oldIndex, int newIndex)
        {
            if(oldIndex >= 0 && newIndex >= 0) {
                BlazorDraggableListEvent eventParameters = new BlazorDraggableListEvent() { DraggableItemOldIndex = oldIndex, DraggableItemNewIndex = newIndex };
                OnDropEvent?.Invoke(typeof(BlazorDraggableListService), eventParameters);
            }
        }

    }
}
