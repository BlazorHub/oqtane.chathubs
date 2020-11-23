using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorWindows
{
    public partial class WindowContainerBase : ComponentBase, IDisposable
    {
        [Parameter] public Action<WindowEventArgs> ShowEvent { get; set; }
        [Parameter] public Action<WindowEventArgs> HideEvent { get; set; }
        [Parameter] public Action<WindowEventArgs> ShownEvent { get; set; }
        [Parameter] public Action<WindowEventArgs> HiddenEvent { get; set; }
        [Parameter] public RenderFragment ChildContent { get; set; }

        public IWindowItem ActiveWindow { get; private set; }
        public List<IWindowItem> WindowItems { get; set; } = new List<IWindowItem>();

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if(this.ActiveWindow == null)
                {
                    await this.SetActiveWindow(this.WindowItems.FirstOrDefault());
                }
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public void AddWindowItem(IWindowItem windowItem)
        {
            if(!WindowItems.Any(item => item.Id == windowItem.Id))
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

        public async Task SetActiveWindow(IWindowItem windowItem)
        {
            if (this.ActiveWindow != windowItem)
            {
                await InvokeAsync(async () =>
                {
                    await Task.Run(() =>
                    {
                        this.ActiveWindow = windowItem;
                    });

                    StateHasChanged();
                });
            }
        }

        public void Dispose()
        {
            
        }

    }
}
