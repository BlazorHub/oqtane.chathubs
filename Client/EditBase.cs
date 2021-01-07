using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Oqtane.Shared.Enums;
using Oqtane.Shared.Models;
using Oqtane.Shared;
using BlazorAlerts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.ChatHubs
{
    public class EditBase : ModuleBase, IDisposable
    {

        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public SiteState SiteState { get; set; }
        [Inject] public IChatHubService ChatHubService { get; set; }
        [Inject] public BlazorAlertsService BlazorAlertsService { get; set; }

        protected readonly string FileUploadDropzoneContainerElementId = "EditComponentFileUploadDropzoneContainer";
        protected readonly string FileUploadInputFileElementId = "EditComponentFileUploadInputFileContainer";

        public HashSet<string> SelectionItems { get; set; } = new HashSet<string>();
        public string SelectedItem { get; set; }

        public override SecurityAccessLevel SecurityAccessLevel { get { return SecurityAccessLevel.Anonymous; } }
        public override string Actions { get { return "Add,Edit"; } }

        public int roomId = -1;
        public string title;
        public string content;
        public string imageUrl;
        public string createdby;
        public DateTime createdon;
        public string modifiedby;
        public DateTime modifiedon;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                foreach(var item in Enum.GetNames(typeof(ChatHubRoomType)))
                {
                    this.SelectionItems.Add(item);
                }

                this.SelectedItem = SelectionItems.FirstOrDefault(item => item == ChatHubRoomType.Public.ToString());

                this.ChatHubService.OnUpdateUI += (object sender, EventArgs e) => UpdateUIStateHasChanged();
                await this.InitContextRoomAsync();
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        private async Task InitContextRoomAsync()
        {
            try
            {
                if (PageState.QueryString.ContainsKey("roomid"))
                {
                    this.roomId = Int32.Parse(PageState.QueryString["roomid"]);
                    ChatHubRoom room = await this.ChatHubService.GetChatHubRoomAsync(roomId, ModuleState.ModuleId);
                    if (room != null)
                    {
                        title = room.Title;
                        content = room.Content;
                        imageUrl = room.ImageUrl;
                        createdby = room.CreatedBy;
                        createdon = room.CreatedOn;
                        modifiedby = room.ModifiedBy;
                        modifiedon = room.ModifiedOn;
                    }
                    else
                    {
                        await logger.LogError("Error Loading Room {ChatHubRoomId} {Error}", roomId);
                        ModuleInstance.AddModuleMessage("Error Loading ChatHub", MessageType.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Room", MessageType.Error);
            }
        }

        public async Task SaveRoom()
        {
            try
            {                
                if (roomId == -1)
                {
                    ChatHubRoom room = new ChatHubRoom()
                    {
                        ModuleId = ModuleState.ModuleId,
                        Title = title,
                        Content = content,
                        Type = Enum.GetName(typeof(ChatHubRoomType), ChatHubRoomType.Public),
                        Status = Enum.GetName(typeof(ChatHubRoomStatus), ChatHubRoomStatus.Active),
                        ImageUrl = string.Empty,
                        OneVsOneId = string.Empty,
                        CreatorId = ChatHubService.ConnectedUser.UserId,
                    };

                    room = await this.ChatHubService.AddChatHubRoomAsync(room);
                    await logger.LogInformation("Room Added {ChatHubRoom}", room);
                    NavigationManager.NavigateTo(NavigateUrl());
                }
                else
                {                    
                    ChatHubRoom room = await this.ChatHubService.GetChatHubRoomAsync(roomId, ModuleState.ModuleId);
                    if (room != null)
                    {
                        room.Title = title;
                        room.Content = content;

                        await this.ChatHubService.UpdateChatHubRoomAsync(room);

                        await logger.LogInformation("Room Updated {ChatHubRoom}", room);
                        NavigationManager.NavigateTo(NavigateUrl());
                    }
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Saving Room {ChatHubRoomId} {Error}", roomId, ex.Message);
                ModuleInstance.AddModuleMessage("Error Saving Room", MessageType.Error);
            }
        }

        private void UpdateUIStateHasChanged()
        {
            InvokeAsync(() =>
            {
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            this.ChatHubService.OnUpdateUI -= (object sender, EventArgs e) => UpdateUIStateHasChanged();
        }

    }
}
