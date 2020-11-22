using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;

namespace BlazorWindows
{
    public partial class WindowItemBase : ComponentBase, IDisposable, IWindowItem
    {

        [CascadingParameter] public WindowContainer WindowContainer { get; set; }

        [Parameter] public RenderFragment WindowTitle { get; set; }

        [Parameter] public RenderFragment WindowContent { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await this.WindowContainer.AddWindow(this);
        }

        public string TitleCssClass => this.WindowContainer.ActiveWindow == this ? "active" : null;

        public async Task ActivateWindow()
        {
            WindowEventArgs objShown = new WindowEventArgs { PreviousItem = this.WindowContainer.ActiveWindow, NextItem = this };
            WindowEventArgs objHidden = new WindowEventArgs { PreviousItem = this.WindowContainer.ActiveWindow, NextItem = this };

            await this.WindowContainer.SetActiveWindow(this);
            this.WindowContainer.ShownEvent.Invoke(objShown);
            this.WindowContainer.HiddenEvent(objHidden);
        }

        public void Dispose()
        {

        }
    }
}
