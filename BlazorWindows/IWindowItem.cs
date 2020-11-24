﻿using Microsoft.AspNetCore.Components;

namespace BlazorWindows
{
    public interface IWindowItem
    {
        WindowContainer WindowContainer { get; set; }
        RenderFragment WindowTitle { get; set; }
        RenderFragment WindowContent { get; set; }
        int Id { get; set; }
    }
}
