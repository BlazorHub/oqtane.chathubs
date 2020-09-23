using BlazorStrap;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oqtane.ChatHubs
{
    public class SettingsModalBase : ModuleBase
    {

        [Inject]
        public IChatHubService ChatHubService { get; set; }

        public MatDialog SettingsDialog;

        public bool DialogIsOpen { get; set; } = false;

        public void OpenDialog()
        {
            this.DialogIsOpen = true;
            StateHasChanged();
        }

        public void CloseDialog()
        {
            this.DialogIsOpen = false;
            StateHasChanged();
        }

    }
}
