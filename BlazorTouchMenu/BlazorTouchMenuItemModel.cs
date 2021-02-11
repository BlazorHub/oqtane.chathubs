using Microsoft.AspNetCore.Components.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorTouchMenu
{
    public class BlazorTouchMenuItemModel
    {

        public string Name { get; set; }

        public string Href { get; set; }

        public string Icon { get; set; }

        public NavLinkMatch NavLinkMatch { get; set; }

    }
}
