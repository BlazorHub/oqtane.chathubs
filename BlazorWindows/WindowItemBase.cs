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

        [Parameter] public RenderFragment WindowLivestream { get; set; }

        [Parameter] public int Id { get; set; }

        [Parameter] public bool InitialSelection { get; set; }

        protected override async Task OnInitializedAsync()
        {
            this.WindowContainer.AddWindowItem(this);

            if (!this.WindowContainer.InitialSelection)
            {
                this.WindowContainer.ActiveWindow = this;
                this.WindowContainer.InitialSelection = true;
            }
            if (this.InitialSelection)
            {
                this.WindowContainer.ActiveWindow = this;
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

        public async void ActivateWindow()
        {
            this.WindowContainer.ActiveWindow = this;
        }

        public void Dispose()
        {
            this.WindowContainer.RemoveWindowItem(this.Id);
        }

    }
}
