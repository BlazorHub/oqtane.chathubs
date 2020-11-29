using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorPager
{
    public partial class BlazorPagerBase<TItemGeneric> : ComponentBase
    {

        [Parameter] public List<TItemGeneric> Items { get; set; } = new List<TItemGeneric>();

        [Parameter] public RenderFragment<TItemGeneric> BlazorPagerItem { get; set; }
        
        [Parameter] public int ItemsPerPage { get; set; }

        [Parameter] public string Class { get; set; }

        public List<TItemGeneric> ContextPageItems { get; set; }

        public int ContextPage { get; set; } = 1;

        public int PagesTotal
        {
            get => Convert.ToInt32(Math.Ceiling(this.Items.Count / Convert.ToDouble(ItemsPerPage)));
        }

        public void UpdateContext()
        {
            this.ContextPageItems = this.Items.Skip((ContextPage - 1) * this.ItemsPerPage).Take(this.ItemsPerPage).ToList();
            StateHasChanged();
        }

        public void SetContextPage(int index)
        {
            this.ContextPage = index;
            this.UpdateContext();
        }

        public void Next()
        {
            this.ContextPage++;
            this.UpdateContext();
        }

        public void Previous()
        {
            this.ContextPage--;
            this.UpdateContext();
        }

        public void First()
        {
            this.ContextPage = 1;
            this.UpdateContext();
        }

        public void Last()
        {
            this.ContextPage = this.PagesTotal;
            this.UpdateContext();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override void OnParametersSet()
        {
            this.UpdateContext();
            base.OnParametersSet();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
        }

    }
}
