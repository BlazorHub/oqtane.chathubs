using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
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
