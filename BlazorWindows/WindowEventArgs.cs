using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWindows
{
    public class WindowEventArgs
    {

        public IWindowItem PreviousItem { get; set; }

        public IWindowItem NextItem { get; set; }

    }
}
