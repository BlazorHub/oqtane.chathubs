using Microsoft.AspNetCore.Components;

namespace BlazorWindows
{
    public interface IWindowItem
    {
        WindowContainer WindowContainer { get; set; }
        RenderFragment WindowTitle { get; }
        RenderFragment WindowContent { get; }
        int Id { get; set; }
    }
}
