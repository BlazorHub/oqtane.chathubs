using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using Oqtane.ChatHubs.Services;
using Oqtane.Modules;
using Oqtane.Services;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Oqtane.Shared.Models;
using BlazorAlerts;
using BlazorWindows;
using System.Net;

namespace Oqtane.ChatHubs
{
    public class IndexBase : ModuleBase, IDisposable
    {
        
        [Inject] protected IJSRuntime JSRuntime { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }
        [Inject] protected HttpClient HttpClient { get; set; }
        [Inject] protected SiteState SiteState { get; set; }
        [Inject] protected ISettingService SettingService { get; set; }
        [Inject] protected BlazorAlertsService BlazorAlertsService { get; set; }
        [Inject] protected IChatHubService ChatHubService { get; set; }
        [Inject] protected VideoService VideoService { get; set; }
        [Inject] protected BrowserResizeService BrowserResizeService { get; set; }
        [Inject] protected ScrollService ScrollService { get; set; }
        [Inject] protected CookieService CookieService { get; set; }

        public int MessageWindowHeight { get; set; }
        public int UserlistWindowHeight { get; set; }

        public string GuestUsername { get; set; } = string.Empty;
        public ChatHubRoom contextRoom { get; set; }

        public List<ChatHubRoom> rooms;
        public int maxUserNameCharacters;

        public int InnerHeight = 0;
        public int InnerWidth = 0;

        public static string ChatWindowDatePattern = @"HH:mm:ss";

        public Dictionary<string, string> settings { get; set; }

        protected ImageModal ImageModalRef;
        protected SettingsModal SettingsModalRef;

        public IndexBase()
        {
            
        }

        protected override void OnInitialized()
        {
            BrowserResizeService.OnResize += BrowserHasResized;
            this.ChatHubService.OnUpdateUI += (object sender, EventArgs e) => UpdateUIStateHasChanged();

            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {

            if (firstRender)
            {
                string hostname = new Uri(NavigationManager.BaseUri).Host;
                this.ChatHubService.IdentityCookie = new Cookie(".AspNetCore.Identity.Application", await this.CookieService.GetCookieAsync(".AspNetCore.Identity.Application"), "/", hostname);

                await JSRuntime.InvokeAsync<object>("browserResize.registerResizeCallback");
                await BrowserHasResized();

                await this.JSRuntime.InvokeVoidAsync("showchathubscontainer");
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                this.ChatHubService.ModuleId = ModuleState.ModuleId;

                this.settings = await this.SettingService.GetModuleSettingsAsync(ModuleState.ModuleId);
                maxUserNameCharacters = int.Parse(this.SettingService.GetSetting(settings, "MaxUserNameCharacters", "500"));
                
                if (PageState.QueryString.ContainsKey("moduleid") && PageState.QueryString.ContainsKey("roomid") && int.Parse(PageState.QueryString["moduleid"]) == ModuleState.ModuleId)
                {
                    this.contextRoom = await this.ChatHubService.GetChatHubRoomAsync(int.Parse(PageState.QueryString["roomid"]), ModuleState.ModuleId);
                }
                else
                {
                    await this.ChatHubService.GetLobbyRooms(ModuleState.ModuleId);
                }
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Loading Rooms {Error}", ex.Message);
                ModuleInstance.AddModuleMessage("Error Loading Rooms", MessageType.Error);
            }

            await base.OnParametersSetAsync();
        }

        public async Task DeleteRoom(int id)
        {
            try
            {
                await ChatHubService.DeleteChatHubRoomAsync(id, ModuleState.ModuleId);
                await logger.LogInformation("Room Deleted {ChatHubRoomId}", id);
                NavigationManager.NavigateTo(NavigateUrl());
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Deleting Room {ChatHubRoomId} {Error}", id, ex.Message);
                ModuleInstance.AddModuleMessage("Error Deleting Room", MessageType.Error);
            }
        }

        public async Task ConnectAsGuest()
        {
            try
            {
                if (this.ChatHubService.Connection?.State == HubConnectionState.Connected
                 || this.ChatHubService.Connection?.State == HubConnectionState.Connecting
                 || this.ChatHubService.Connection?.State == HubConnectionState.Reconnecting)
                {
                    this.BlazorAlertsService.NewBlazorAlert("The client is already connected.");
                }

                this.ChatHubService.BuildGuestConnection(GuestUsername, ModuleState.ModuleId);
                this.ChatHubService.RegisterHubConnectionHandlers();
                await this.ChatHubService.ConnectAsync();
            }
            catch (Exception ex)
            {
                await logger.LogError(ex, "Error Connecting To ChatHub: {Error}", ex.Message);
                ModuleInstance.AddModuleMessage("Error Connecting To ChatHub", MessageType.Error);
            }
        }

        public async Task EnterRoom_Clicked(int roomId, int moduleid)
        {
            if(!this.ChatHubService.Rooms.Any(item => item.Id == roomId) && ChatHubService.Connection?.State == HubConnectionState.Connected)
            {
                await this.ChatHubService.EnterChatRoom(roomId);
            }
        }

        public async Task LeaveRoom_Clicked(int roomId, int moduleId)
        {
            await this.ChatHubService.LeaveChatRoom(roomId);
        }

        public async Task FollowInvitation_Clicked(Guid invitationGuid, int roomId)
        {
            if (ChatHubService.Connection?.State == HubConnectionState.Connected)
            {
                await this.ChatHubService.EnterChatRoom(roomId);
                this.ChatHubService.RemoveInvitation(invitationGuid);
            }
        }

        public void RemoveInvitation_Clicked(Guid guid)
        {
            this.ChatHubService.RemoveInvitation(guid);
        }

        public async Task KeyDown(KeyboardEventArgs e, ChatHubRoom room)
        {
            if (!e.ShiftKey && e.Key == "Enter")
            {
                await this.SendMessage_Clicked(room.MessageInput, room);
            }
        }

        public async Task SendMessage_Clicked(string messageInput, ChatHubRoom room)
        {
            await this.ChatHubService.SendMessage(messageInput, room.Id, ModuleState.ModuleId);
            room.MessageInput = string.Empty;
            this.StateHasChanged();
        }
        
        private async Task BrowserHasResized()
        {
            try
            {
                await InvokeAsync(async () =>
                {
                    InnerHeight = await this.BrowserResizeService.GetInnerHeight();
                    InnerWidth = await this.BrowserResizeService.GetInnerWidth();

                    SetChatTabElementsHeight();

                    StateHasChanged();
                });
            }
            catch(Exception ex)
            {
                await logger.LogError(ex, "Error On Browser Resize {Error}", ex.Message);
                ModuleInstance.AddModuleMessage("Error On Browser Resize", MessageType.Error);
            }
        }

        private void SetChatTabElementsHeight()
        {
            MessageWindowHeight = 520;
            UserlistWindowHeight = 570;
        }

        public void UserlistItem_Clicked(MouseEventArgs e, ChatHubRoom room, ChatHubUser user)
        {
            InvokeAsync(() =>
            {
                if (user.UserlistItemCollapsed)
                {
                    user.UserlistItemCollapsed = false;
                }
                else
                {
                    foreach (var chatUser in room.Users.Where(x => x.UserlistItemCollapsed == true))
                    {
                        chatUser.UserlistItemCollapsed = false;
                    }
                    user.UserlistItemCollapsed = true;
                }

                StateHasChanged();
            });
        }

        public void OpenProfile_Clicked(int userId, int roomId)
        {
            this.SettingsModalRef.OpenDialog();
        }

        public async Task FixCorruptConnections_ClickedAsync()
        {
            try
            {
                await this.ChatHubService.FixCorruptConnections(ModuleState.ModuleId);
            }
            catch
            {
                throw;
            }
        }

        public string ReplaceYoutubeLinksAsync(string message)
        {
            try
            {
                //var youtubeRegex = @"(?:http?s?:\/\/)?(?:www.)?(?:m.)?(?:music.)?youtu(?:\.?be)(?:\.com)?(?:(?:\w*.?:\/\/)?\w*.?\w*-?.?\w*\/(?:embed|e|v|watch|.*\/)?\??(?:feature=\w*\.?\w*)?&?(?:v=)?\/?)([\w\d_-]{11})(?:\S+)?";
                List<string> regularExpressions = this.SettingService.GetSetting(this.settings, "RegularExpression", "").Split(";delimiter;", StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var regularExpression in regularExpressions)
                {
                    string pattern = regularExpression;
                    string replacement = string.Format("<a href=\"{0}\" target=\"_blank\" title=\"{0}\">{0}</a>", "$0");
                    message = Regex.Replace(message, pattern, replacement);
                }
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }

            return message;
        }

        public string HighlightOwnUsername(string message, string username)
        {
            try
            {
                string pattern = username;
                string replacement = string.Format("<strong>{0}</strong>", "$0");
                message = Regex.Replace(message, pattern, replacement);
            }
            catch (Exception ex)
            {
                ModuleInstance.AddModuleMessage(ex.Message, MessageType.Error);
            }

            return message;
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
            BrowserResizeService.OnResize -= BrowserHasResized;
            this.ChatHubService.OnUpdateUI -= (object sender, EventArgs e) => UpdateUIStateHasChanged();

            //this.ChatHubService.DisposeStreamTasks();
        }

        public void ShowWindow(WindowEvent e)
        {
            //this.BlazorAlertsService.NewBlazorAlert("on show");

            this.ChatHubService.ContextRoomId = e.ActivatedItem.Id.ToString();
            var room = this.ChatHubService.Rooms.FirstOrDefault(item => item.Id.ToString() == this.ChatHubService.ContextRoomId);
            if (room != null)
            {
                room.UnreadMessages = 0;
            }
        }
        public void HideWindow(WindowEvent e)
        {
            //this.BlazorAlertsService.NewBlazorAlert("on hide");

            var room = this.ChatHubService.Rooms.FirstOrDefault(item => item.Id == e.DeactivatedItem.Id);
            if (room != null)
            {
                //this.ChatHubService.StopVideoChat(e.DeactivatedItem.Id);
            }
        }
        public void ShownWindow(WindowEvent e)
        {
            //this.BlazorAlertsService.NewBlazorAlert("on shown");
        }
        public void HiddenWindow(WindowEvent e)
        {
            //this.BlazorAlertsService.NewBlazorAlert("on hidden");
        }

    }
}