using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorDraggableList
{
    public partial class BlazorDraggableListBase<TItemGeneric> : ComponentBase
    {

        [Parameter] public List<TItemGeneric> Items { get; set; }

        [Parameter] public string Class { get; set; }

        [Parameter] public RenderFragment<TItemGeneric> DraggableItem { get; set; }

    }
}
