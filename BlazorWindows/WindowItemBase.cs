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

        [Parameter] public int Id { get; set; }

        [Parameter] public bool InitialSelection { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.WindowContainer.AddWindowItem(this);

            if (this.InitialSelection)
            {
                await this.WindowContainer.SetActiveWindow(this);
            }

            await base.OnInitializedAsync();
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {

            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public string TitleCssClass => this.WindowContainer.ActiveWindow == this ? "active" : null;

        public async Task ActivateWindow()
        {
            WindowEventArgs objShown = new WindowEventArgs { DeactivatedItem = this.WindowContainer.ActiveWindow, ActivatedItem = this };
            WindowEventArgs objHidden = new WindowEventArgs { DeactivatedItem = this.WindowContainer.ActiveWindow, ActivatedItem = this };

            await this.WindowContainer.SetActiveWindow(this);
            this.WindowContainer.ShownEvent.Invoke(objShown);
            this.WindowContainer.HiddenEvent(objHidden);
        }

        public void Dispose()
        {
            this.WindowContainer.RemoveWindowItem(this.Id);
        }

    }
}
