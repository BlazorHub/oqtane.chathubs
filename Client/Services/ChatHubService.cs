using Microsoft.AspNetCore.Components;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections;
using Oqtane.Services;
using System.Linq;
using System.Timers;
using Oqtane.Shared.Models;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Modules;
using System.Threading;
using System.Data;

namespace Oqtane.ChatHubs.Services
{

    public class ChatHubService : ServiceBase, IChatHubService, IService
    {

        public HttpClient HttpClient { get; set; }
        public NavigationManager NavigationManager { get; set; }
        public SiteState SiteState { get; set; }
        public IJSRuntime JSRuntime { get; set; }
        public VideoService VideoService { get; set; }

        public HubConnection Connection { get; set; }
        public ChatHubUser ConnectedUser { get; set; }
        public string ContextRoomId { get; set; }
        public int ModuleId { get; set; }

        public List<ChatHubRoom> Lobbies { get; set; } = new List<ChatHubRoom>();
        public List<ChatHubRoom> Rooms { get; set; } = new List<ChatHubRoom>();

        public List<ChatHubInvitation> Invitations { get; set; } = new List<ChatHubInvitation>();

        public List<ChatHubUser> IgnoredUsers { get; set; } = new List<ChatHubUser>();
        public List<ChatHubUser> IgnoredByUsers { get; set; } = new List<ChatHubUser>();

        public Dictionary<int, dynamic> StreamTasks { get; set; } = new Dictionary<int, dynamic>();

        public System.Timers.Timer GetLobbyRoomsTimer { get; set; } = new System.Timers.Timer();
        public System.Timers.Timer VideoStreamTimer { get; set; } = new System.Timers.Timer();

        public event Action UpdateUI;
        public event EventHandler<ChatHubUser> OnConnectedEvent;
        public event EventHandler<ChatHubRoom> OnAddChatHubRoomEvent;
        public event EventHandler<ChatHubRoom> OnRemoveChatHubRoomEvent;
        public event EventHandler<dynamic> OnAddChatHubUserEvent;
        public event EventHandler<dynamic> OnRemoveChatHubUserEvent;
        public event EventHandler<ChatHubMessage> OnAddChatHubMessageEvent;
        public event EventHandler<ChatHubInvitation> OnAddChatHubInvitationEvent;
        public event EventHandler<ChatHubInvitation> OnRemoveChatHubInvitationEvent;
        public event EventHandler<ChatHubUser> OnAddIgnoredUserEvent;
        public event EventHandler<ChatHubUser> OnRemoveIgnoredUserEvent;
        public event EventHandler<ChatHubUser> OnAddIgnoredByUserEvent;
        public event EventHandler<ChatHubUser> OnRemoveIgnoredByUserEvent;
        public event EventHandler<dynamic> OnDownloadBytes;
        public event EventHandler<int> OnClearHistoryEvent;
        public event EventHandler<ChatHubUser> OnDisconnectEvent;
        public event EventHandler<dynamic> OnExceptionEvent;

        public ChatHubService(HttpClient httpClient, SiteState siteState, NavigationManager navigationManager, IJSRuntime JSRuntime, VideoService videoService = null) : base (httpClient)
        {
            this.HttpClient = httpClient;
            this.SiteState = siteState;
            this.NavigationManager = navigationManager;
            this.JSRuntime = JSRuntime;
            this.VideoService = videoService;

            this.VideoService.OnDataAvailableEventHandler += async (object sender, dynamic e) => await OnDataAvailableEventHandlerExecute(e.dataURI, e.roomId, e.dataType);

            this.OnConnectedEvent += OnConnectedExecute;
            this.OnAddChatHubRoomEvent += OnAddChatHubRoomExecute;
            this.OnRemoveChatHubRoomEvent += OnRemoveChatHubRoomExecute;
            this.OnAddChatHubUserEvent += OnAddChatHubUserExecute;
            this.OnRemoveChatHubUserEvent += OnRemoveChatHubUserExecute;
            this.OnAddChatHubMessageEvent += OnAddChatHubMessageExecute;
            this.OnAddChatHubInvitationEvent += OnAddChatHubInvitationExecute;
            this.OnRemoveChatHubInvitationEvent += OnRemoveChatHubInvitationExecute;
            this.OnAddIgnoredUserEvent += OnAddIngoredUserExexute;
            this.OnRemoveIgnoredUserEvent += OnRemoveIgnoredUserExecute;
            this.OnAddIgnoredByUserEvent += OnAddIgnoredByUserExecute;
            this.OnDownloadBytes += OnDownloadBytesExecuteAsync;
            this.OnRemoveIgnoredByUserEvent += OnRemoveIgnoredByUserExecute;
            this.OnClearHistoryEvent += OnClearHistoryExecute;
            this.OnDisconnectEvent += OnDisconnectExecute;
        }

        public void OnConnectedExecute(object sender, ChatHubUser user)
        {
            this.ConnectedUser = user;
            this.UpdateUI();
        }

        public void BuildGuestConnection(string username, int moduleId)
        {
            StringBuilder urlBuilder = new StringBuilder();
            var chatHubConnection = this.NavigationManager.BaseUri + "chathub";

            urlBuilder.Append(chatHubConnection);
            urlBuilder.Append("?guestname=" + username);

            var url = urlBuilder.ToString();
            Connection = new HubConnectionBuilder().WithUrl(url, options =>
            {
                options.Headers["moduleid"] = moduleId.ToString();
                options.Headers["platform"] = "Oqtane";
                options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
            })
            .AddNewtonsoftJsonProtocol(options => {
                options.PayloadSerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            })
            .Build();
        }

        public void RegisterHubConnectionHandlers()
        {
            Connection.Reconnecting += (ex) =>
            {
                if (ex != null)
                {
                    this.HandleException(new Exception(ex.Message, ex));
                }

                return Task.CompletedTask;
            };
            Connection.Reconnected += (msg) =>
            {
                if (msg != null)
                {
                    this.HandleException(new Exception(msg));
                }

                return Task.CompletedTask;
            };
            Connection.Closed += (ex) =>
            {
                if (ex != null)
                {
                    this.HandleException(new Exception(ex.Message, ex));
                }

                this.Rooms.Clear();
                this.UpdateUI();
                return Task.CompletedTask;
            };

            this.Connection.On("OnConnected", (ChatHubUser user) => OnConnectedEvent(this, user));
            this.Connection.On("AddRoom", (ChatHubRoom room) => OnAddChatHubRoomEvent(this, room));
            this.Connection.On("RemoveRoom", (ChatHubRoom room) => OnRemoveChatHubRoomEvent(this, room));
            this.Connection.On("AddUser", (ChatHubUser user, string roomId) => OnAddChatHubUserEvent(this, new { userModel = user, roomId = roomId }));
            this.Connection.On("RemoveUser", (ChatHubUser user, string roomId) => OnRemoveChatHubUserEvent(this, new { userModel = user, roomId = roomId }));
            this.Connection.On("AddMessage", (ChatHubMessage message) => OnAddChatHubMessageEvent(this, message));
            this.Connection.On("AddInvitation", (ChatHubInvitation invitation) => OnAddChatHubInvitationEvent(this, invitation));
            this.Connection.On("RemoveInvitation", (ChatHubInvitation invitation) => OnRemoveChatHubInvitationEvent(this, invitation));
            this.Connection.On("AddIgnoredUser", (ChatHubUser ignoredUser) => OnAddIgnoredUserEvent(this, ignoredUser));
            this.Connection.On("RemoveIgnoredUser", (ChatHubUser ignoredUser) => OnRemoveIgnoredUserEvent(this, ignoredUser));
            this.Connection.On("AddIgnoredByUser", (ChatHubUser ignoredUser) => OnAddIgnoredByUserExecute(this, ignoredUser));
            this.Connection.On("DownloadBytes", (IAsyncEnumerable<string> dataURI, int roomId, string dataType) => OnDownloadBytesExecuteAsync(this, new { dataURI = dataURI, roomId = roomId, dataType = dataType }));
            this.Connection.On("RemoveIgnoredByUser", (ChatHubUser ignoredUser) => OnRemoveIgnoredByUserExecute(this, ignoredUser));
            this.Connection.On("ClearHistory", (int roomId) => OnClearHistoryEvent(this, roomId));
            this.Connection.On("Disconnect", (ChatHubUser user) => OnDisconnectEvent(this, user));
        }

        public async Task ConnectAsync()
        {
            await this.Connection.StartAsync().ContinueWith(task =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }

        public async Task StartVideoChat(ChatHubRoom room)
        {
            try
            {
                if (room.CreatorId == this.ConnectedUser.UserId)
                {
                    await this.VideoService.InitVideoJs();
                    await this.VideoService.StartBroadcasting(room.Id);

                    CancellationTokenSource tokenSource = new CancellationTokenSource();
                    CancellationToken token = tokenSource.Token;
                    Task task = new Task(async () => await this.StreamTaskImplementation(room.Id, token), token);
                    this.AddStreamTask(room.Id, task, tokenSource);
                    task.Start();
                }
                else
                {
                    await this.VideoService.InitVideoJs();
                    await this.VideoService.StartStreaming(room.Id);
                }
            }
            catch (Exception ex)
            {
                this.HandleException(ex);
            }
        }

        public void AddStreamTask(int roomId, Task task, CancellationTokenSource tokenSource)
        {
            this.RemoveStreamTask(roomId);
            this.StreamTasks.Add(roomId, new { task = task, tokenSource = tokenSource });
        }

        public async Task StreamTaskImplementation(int roomId, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await this.VideoService.RecordSequence(roomId);
                    await Task.Delay(120);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void DisposeStreamTasks()
        {
            foreach(var task in StreamTasks)
            {
                this.StopVideoChat(task.Key);
            }
        }

        public async void StopVideoChat(int roomId)
        {
            this.RemoveStreamTask(roomId);
            await this.VideoService.CloseLivestream(roomId);            
        }

        public void RemoveStreamTask(int roomId)
        {
            List<KeyValuePair<int, dynamic>> list = this.StreamTasks.Where(item => item.Key == roomId).ToList();
            if (list.Any())
            {
                KeyValuePair<int, dynamic> keyValuePair = list.FirstOrDefault();
                keyValuePair.Value.tokenSource.Cancel();
                keyValuePair.Value.task.Dispose();
                this.StreamTasks.Remove(keyValuePair.Key);
            }
        }

        public async Task OnDataAvailableEventHandlerExecute(string dataURI, int roomId, string dataType)
        {
            try
            {
                if(this.Connection?.State == HubConnectionState.Connected)
                {
                    int maxLength = 60;
                    async IAsyncEnumerable<string> broadcastData()
                    {
                        for (var i = 0; i < dataURI.Length; i += maxLength)
                        {
                            yield return dataURI.Substring(i, Math.Min(maxLength, dataURI.Length - i));
                        }
                    }

                    await this.Connection.SendAsync("UploadBytes", broadcastData(), roomId, dataType).ContinueWith((task) =>
                    {
                        if (task.IsCompleted)
                        {
                            this.HandleException(task);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async void OnDownloadBytesExecuteAsync(object sender, dynamic e)
        {
            IAsyncEnumerable<string> dataURI = e.dataURI;
            int roomId = e.roomId;
            string dataType = e.dataType;

            string dataURIresult = string.Empty;
            IAsyncEnumerator<string> enumerators = dataURI.GetAsyncEnumerator();

            while (await enumerators.MoveNextAsync())
            {
                dataURIresult += enumerators.Current;
            }

            try
            {
                await this.VideoService.AppendBuffer(dataURIresult, roomId, dataType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task EnterChatRoom(int roomId)
        {
            await this.Connection.InvokeAsync("EnterChatRoom", roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }

        public async Task LeaveChatRoom(int roomId)
        {
            await this.Connection.InvokeAsync("LeaveChatRoom", roomId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }

        public async Task GetLobbyRooms(int moduleId)
        {
            try
            {
                this.Lobbies = await this.GetChatHubRoomsByModuleIdAsync(moduleId);
                this.SortLobbyRooms();
                this.UpdateUI();
            }
            catch (Exception ex)
            {
                // !!!Important | This Try Catch Block Is Necessary
                this.HandleException(ex);
            }
        }

        public async Task GetIgnoredUsers()
        {
            await this.Connection.InvokeAsync<List<ChatHubUser>>("GetIgnoredUsers").ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);

                    var ignoredUsers = task.Result;
                    if (ignoredUsers != null)
                    {
                        foreach (var user in ignoredUsers)
                        {
                            this.AddIgnoredUser(user);
                        }
                    }
                }
            });
        }

        public async Task GetIgnoredByUsers()
        {
            await this.Connection.InvokeAsync<List<ChatHubUser>>("GetIgnoredByUsers").ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);

                    var ignoredByUsers = task.Result;
                    if (ignoredByUsers != null)
                    {
                        foreach (var user in ignoredByUsers)
                        {
                            this.AddIgnoredByUser(user);
                        }
                    }
                }
            });
        }

        public void SortLobbyRooms()
        {
            if (this.Lobbies != null && this.Lobbies.Any())
            {
                this.Lobbies = this.Lobbies.OrderByDescending(item => item.Users?.Count()).ThenByDescending(item => item.CreatedOn).Take(100).ToList();
            }
        }

        public void SendMessage(string content, int roomId, int moduleId)
        {
            this.Connection.InvokeAsync("SendMessage", content, roomId, moduleId).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {
                    this.HandleException(task);
                }
            });
        }

        public void IgnoreUser_Clicked(int userId, int roomId, string username)
        {
            this.Connection.InvokeAsync("IgnoreUser", username).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {

                }
            });
        }

        public void UnignoreUser_Clicked(string username)
        {
            this.Connection.InvokeAsync("UnignoreUser", username).ContinueWith((task) =>
            {
                if (task.IsCompleted)
                {

                }
            });
        }

        public void ClearHistory(int roomId)
        {
            var room = this.Rooms.FirstOrDefault(x => x.Id == roomId);
            room.Messages.Clear();
            this.UpdateUI();
        }

        public void ToggleUserlist(ChatHubRoom room)
        {
            room.ShowUserlist = !room.ShowUserlist;
        }

        public async Task DisconnectAsync()
        {
            if (Connection.State != HubConnectionState.Disconnected)
            {
                await Connection.StopAsync();
            }
        }

        private void OnAddChatHubRoomExecute(object sender, ChatHubRoom room)
        {
            this.AddRoom(room);
            this.UpdateUI();
        }
        private void OnRemoveChatHubRoomExecute(object sender, ChatHubRoom room)
        {
            this.RemoveRoom(room);
            this.UpdateUI();
        }
        private void OnAddChatHubUserExecute(object sender, dynamic obj)
        {
            this.AddUser(obj.userModel, obj.roomId);
            this.UpdateUI();
        }
        private void OnRemoveChatHubUserExecute(object sender, dynamic obj)
        {
            this.RemoveUser(obj.userModel, obj.roomId);
            this.UpdateUI();
        }

        public async void OnAddChatHubMessageExecute(object sender, ChatHubMessage message)
        {
            ChatHubRoom room = this.Rooms.FirstOrDefault(item => item.Id == message.ChatHubRoomId);

            this.AddMessage(message, room);
            this.UpdateUI();            
        }

        private void OnAddChatHubInvitationExecute(object sender, ChatHubInvitation item)
        {
            this.AddInvitation(item);
        }
        private void OnRemoveChatHubInvitationExecute(object sender, ChatHubInvitation item)
        {
            this.RemoveInvitation(item.Guid);
        }

        private void OnAddIngoredUserExexute(object sender, ChatHubUser user)
        {
            this.AddIgnoredUser(user);
            this.UpdateUI();
        }
        private void OnRemoveIgnoredUserExecute(object sender, ChatHubUser user)
        {
            this.RemoveIgnoredUser(user);
            this.UpdateUI();
        }
        private void OnAddIgnoredByUserExecute(object sender, ChatHubUser user)
        {
            this.AddIgnoredByUser(user);
            this.UpdateUI();
        }
        private void OnRemoveIgnoredByUserExecute(object sender, ChatHubUser user)
        {
            this.RemoveIgnoredByUser(user);
            this.UpdateUI();
        }
        private void OnClearHistoryExecute(object sender, int roomId)
        {
            this.ClearHistory(roomId);
        }
        private async void OnDisconnectExecute(object sender, ChatHubUser user)
        {
            await this.DisconnectAsync();
        }

        public void AddRoom(ChatHubRoom room)
        {
            if (!this.Rooms.Any(x => x.Id == room.Id))
            {
                this.Rooms.Add(room);
            }
        }
        public void RemoveRoom(ChatHubRoom room)
        {
            var chatRoom = this.Rooms.First(x => x.Id == room.Id);
            if (chatRoom != null)
            {
                this.Rooms.Remove(chatRoom);
            }
        }
        public void AddUser(ChatHubUser user, string roomId)
        {
            var room = this.Rooms.FirstOrDefault(x => x.Id.ToString() == roomId);
            if (room != null && !room.Users.Any(x => x.UserId == user.UserId))
            {
                room.Users.Add(user);
            }
        }
        public void RemoveUser(ChatHubUser user, string roomId)
        {
            var room = this.Rooms.FirstOrDefault(x => x.Id.ToString() == roomId);
            if (room != null)
            {
                var userItem = room.Users.FirstOrDefault(x => x.UserId == user.UserId);
                if (userItem != null)
                {
                    room.Users.Remove(userItem);
                }
            }
        }
        public void AddMessage(ChatHubMessage message, ChatHubRoom room)
        {
            if (!room.Messages.Any(x => x.Id == message.Id))
            {
                room.Messages.Add(message);
            }
        }
        public void AddInvitation(ChatHubInvitation invitation)
        {
            if (!this.Invitations.Any(x => x.Guid == invitation.Guid))
            {
                this.Invitations.Add(invitation);
            }
        }
        public void RemoveInvitation(Guid guid)
        {
            var item = this.Invitations.First(x => x.Guid == guid);
            if (item != null)
            {
                this.Invitations.Remove(item);
            }
        }
        public void AddIgnoredUser(ChatHubUser user)
        {
            if (!this.IgnoredUsers.Any(x => x.UserId == user.UserId))
            {
                this.IgnoredUsers.Add(user);
            }
        }
        public void RemoveIgnoredUser(ChatHubUser user)
        {
            var item = this.IgnoredUsers.FirstOrDefault(x => x.UserId == user.UserId);
            if (item != null)
            {
                this.IgnoredUsers.Remove(item);
            }
        }
        public void AddIgnoredByUser(ChatHubUser user)
        {
            if (!this.IgnoredByUsers.Any(x => x.UserId == user.UserId))
            {
                this.IgnoredByUsers.Add(user);
            }
        }
        public void RemoveIgnoredByUser(ChatHubUser user)
        {
            var item = this.IgnoredByUsers.FirstOrDefault(x => x.UserId == user.UserId);
            if (item != null)
            {
                this.IgnoredByUsers.Remove(item);
            }
        }

        private async void OnGetLobbyRoomsTimerElapsed(object source, ElapsedEventArgs e)
        {
            await this.GetLobbyRooms(this.ModuleId);
        }

        public string apiurl
        {
            get { return CreateApiUrl(SiteState.Alias, "ChatHub"); }
        }

        public async Task<List<ChatHubRoom>> GetChatHubRoomsByModuleIdAsync(int ModuleId)
        {
            return await HttpClient.GetJsonAsync<List<ChatHubRoom>>(apiurl + "/getchathubroomsbymoduleid?moduleid=" + ModuleId + "&entityid=" + ModuleId);
        }
        public async Task<ChatHubRoom> GetChatHubRoomAsync(int ChatHubRoomId, int ModuleId)
        {
            return await HttpClient.GetJsonAsync<ChatHubRoom>(apiurl + "/getchathubroom/" + ChatHubRoomId + "?moduleid=" + ModuleId + "&entityid=" + ModuleId);
        }
        public async Task<ChatHubRoom> AddChatHubRoomAsync(ChatHubRoom ChatHubRoom)
        {
            return await HttpClient.PostJsonAsync<ChatHubRoom>(apiurl + "/addchathubroom" + "?entityid=" + ChatHubRoom.ModuleId, ChatHubRoom);
        }
        public async Task UpdateChatHubRoomAsync(ChatHubRoom ChatHubRoom)
        {
            await HttpClient.PutJsonAsync(apiurl + "/updatechathubroom/" + ChatHubRoom.Id + "?entityid=" + ChatHubRoom.ModuleId, ChatHubRoom);
        }
        public async Task DeleteChatHubRoomAsync(int ChatHubRoomId, int ModuleId)
        {
            await HttpClient.DeleteAsync(apiurl + "/deletechathubroom/" + ChatHubRoomId + "?moduleid=" + ModuleId + "&entityid=" + ModuleId);
        }

        public async Task DeleteRoomImageAsync(int ChatHubRoomId, int ModuleId)
        {
            await HttpClient.DeleteAsync(apiurl + "/deleteroomimage/" + ChatHubRoomId + "?moduleid=" + ModuleId + "&entityid=" + ModuleId);
        }

        private void HandleException(Task task)
        {
            if (task.Exception != null)
            {
                this.HandleException(task.Exception);
            }
        }
        private void HandleException(Exception exception)
        {
            this.OnExceptionEvent.Invoke(this, new { Exception = exception, ConnectedUser = this.ConnectedUser });
        }

        public async Task FixCorruptConnections(int ModuleId)
        {
            await HttpClient.DeleteAsync(apiurl + "/fixcorruptconnections" + "?moduleid=" + ModuleId + "&entityid=" + ModuleId);
        }

    }
}
