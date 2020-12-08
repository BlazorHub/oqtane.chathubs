using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorDraggableList;

namespace BlazorWindows
{
    public partial class WindowContainerBase : ComponentBase, IDisposable
    {
        [Parameter] public RenderFragment ChildContent { get; set; }

        [Parameter] public EventCallback<WindowEvent> ShowEvent { get; set; }
        [Parameter] public EventCallback<WindowEvent> ShownEvent { get; set; }
        [Parameter] public EventCallback<WindowEvent> HideEvent { get; set; }
        [Parameter] public EventCallback<WindowEvent> HiddenEvent { get; set; }

        internal bool Disposing { get; set; } = false;
        internal bool HasRendered { get; set; } = false;
        internal List<EventCallback<WindowEvent>> WindowEvents { get; set; } = new List<EventCallback<WindowEvent>>();
        internal WindowEvent WindowEvent { get; set; }

        public bool InitialSelection { get; set; }

        private IWindowItem _activeWindow;
        public IWindowItem ActiveWindow
        {
            get { 
                return _activeWindow;
            }
            set
            {
                if (this.ActiveWindow == value) return;
                if (this.Disposing) return;
                this.WindowEvent = new WindowEvent() { ActivatedItem = value, DeactivatedItem = _activeWindow };

                if (this.HasRendered)
                {
                    InvokeAsync(() => ShowEvent.InvokeAsync(this.WindowEvent));
                    InvokeAsync(() => HideEvent.InvokeAsync(this.WindowEvent));
                    WindowEvents.Add(this.ShownEvent);
                    WindowEvents.Add(this.HiddenEvent);
                }
                this._activeWindow = value;
                InvokeAsync(StateHasChanged);
            }
        }

        public List<IWindowItem> WindowItems { get; set; } = new List<IWindowItem>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                this.HasRendered = true;
            }

            for (var i = 0; i < this.WindowEvents.Count; i++)
            {
                this.WindowEvents[i].InvokeAsync(WindowEvent);
                this.WindowEvents.RemoveAt(i);
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void SelectWindowById(int id)
        {
            this.ActiveWindow = this.WindowItems.Find(item => item.Id == id);
        }        

        public void AddWindowItem(IWindowItem windowItem)
        {
            if (!WindowItems.Any(item => item.Id == windowItem.Id))
            {
                this.WindowItems.Add(windowItem);
            }
        }

        public void RemoveWindowItem(int id)
        {
            var windowItem = this.WindowItems.Where(item => item.Id == id).FirstOrDefault();
            if (windowItem != null)
            {
                this.WindowItems.Remove(windowItem);
            }
        }

        public void Dispose()
        {
            
        }

    }
}
