using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWindows
{
    public class WindowEventArgs
    {

        public IWindowItem ActivatedItem { get; set; }

        public IWindowItem DeactivatedItem { get; set; }

    }
}
