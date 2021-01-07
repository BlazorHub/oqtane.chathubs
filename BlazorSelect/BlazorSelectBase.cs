using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorSelect
{
    public class BlazorSelectBase : ComponentBase
    {

        [Parameter] public HashSet<string> SelectionItems { get; set; }

        [Parameter] public string SelectedItem { get; set; }

    }
}
