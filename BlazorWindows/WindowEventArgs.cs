using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWindows
{
    public class WindowEventArgs
    {

        public IWindowItem HiddenItem { get; set; }

        public IWindowItem ShownItem { get; set; }

    }
}
