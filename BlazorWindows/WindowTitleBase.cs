using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace BlazorWindows
{
    public partial class WindowTitleBase : ComponentBase, IDisposable
    {

        [CascadingParameter]
        public IWindowItem WindowItem { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        public void Dispose()
        {

        }

    }
}
