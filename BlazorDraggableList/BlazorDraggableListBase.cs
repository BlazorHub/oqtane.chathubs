using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace BlazorDraggableList
{
    public partial class BlazorDraggableListBase<TItemGeneric> : ComponentBase, IDisposable
    {

        [Inject] BlazorDraggableListService BlazorDraggableListService { get; set; }

        [Parameter] public IList<TItemGeneric> Items { get; set; }

        [Parameter] public string Class { get; set; }

        [Parameter] public RenderFragment<TItemGeneric> BlazorDraggableListItem { get; set; }

        protected override Task OnInitializedAsync()
        {
            this.BlazorDraggableListService.TItemGenericType = typeof(TItemGeneric);
            this.BlazorDraggableListService.BlazorDraggableListServiceExtension.OnDropEvent += OnDropEventExecute;
            return base.OnInitializedAsync();
        }

        private void OnDropEventExecute(object sender, BlazorDraggableListEvent e)
        {
            this.Items = this.Items.Swap(e.DraggableItemOldIndex, e.DraggableItemNewIndex);
            StateHasChanged();
        }

        public void Dispose()
        {
            BlazorDraggableListService.BlazorDraggableListServiceExtension.OnDropEvent -= OnDropEventExecute;
        }
    }

    public static class BlazorDraggableListExtension
    {
        public static IList<TItemGeneric> Swap<TItemGeneric>(this IList<TItemGeneric> list, int x, int y)
        {
            TItemGeneric temp = list[x];
            list[x] = list[y];
            list[y] = temp;
            return list;
        }
    }
}
