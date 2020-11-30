using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorDraggableList
{
    public class BlazorDraggableListEvent
    {

        public int DraggableItemNewIndex { get; set; }

        public int DraggableItemOldIndex { get; set; }

        public BlazorDraggableListEvent() { }

    }
}
