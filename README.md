![Module](https://raw.githubusercontent.com/boredanyway/oqtane.chathubs/master/Server/wwwroot/Modules/Oqtane.ChatHubs/images/anyways-logo.png "anyways logo")

# Asp .Net Core Blazor SignalR Entity Framework Core Chat Hub Template

The oqtane chathubs module allows developers to code and run realtime chat. Post issues anytime you want it is okay and appreciated i will reply as soon as im ready and fixed it anyhow. 

# Getting Started For Oqtane ChatHub Module Dev

- [x] Get familiar with the [Oqtane Framework](https://github.com/oqtane/oqtane.framework).
- [x] Clone the Oqtane Github Repository in Visual Studio Team Explorer.
- [x] Clone the Oqtane ChatHub Module in VS Team Explorer and build in debug and release mode.
- [ ] get it work somehow good luck anyways

# Edit _Host.cshtml end of head tag
```HTML
<script src="_content/MatBlazor/dist/matBlazor.js"></script>
<link href="_content/MatBlazor/dist/matBlazor.css" rel="stylesheet" />
```

# Edit _Host.cshtml end of body tag
```HTML
<script async defer src="https://buttons.github.io/buttons.js"></script>
<script src="_content/BlazorStrap/blazorStrap.js"></script>
<script src="_content/BlazorTable/BlazorTable.min.js"></script>
<script src="modules/oqtane.chathubs/chat-hub-generated-bootstrap.min.css"></script>
<script src="modules/oqtane.chathubs/chat-hub-js-interop.js"></script>
```

# Edit startup.cs configure services methode
```C#
services.AddScoped<VideoService, VideoService>();
services.AddScoped<BlazorAlertsService, BlazorAlertsService>();
services.AddScoped<BlazorDraggableListService, BlazorDraggableListService>();
services.AddScoped<BlazorFileUploadService, BlazorFileUploadService>();


services.AddMvc()
    .AddNewtonsoftJson(options => 
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

services.AddSignalR()
    .AddHubOptions<ChatHub>(options =>
    {
        options.EnableDetailedErrors = true;
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
        options.MaximumReceiveMessageSize = Int64.MaxValue;
        options.StreamBufferCapacity = Int32.MaxValue;
    })
    .AddMessagePackProtocol()
    .AddNewtonsoftJsonProtocol(options =>
    {
        options.PayloadSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
    });
```

# Edit startup.cs configure runtime pipeline
```C#	
endpoints.MapHub<ChatHub>("/chathub", options =>
{
    options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
    options.ApplicationMaxBufferSize = Int64.MaxValue;
    options.TransportMaxBufferSize = Int64.MaxValue;
    options.WebSockets.CloseTimeout = TimeSpan.FromSeconds(10);
    options.LongPolling.PollTimeout = TimeSpan.FromSeconds(10);
});
```

# Edit TenantResolver.cs
```C#
if (segments.Length > 1 && (segments[1] == "api" || segments[1] == "pages") && segments[0] != "~")
{
	aliasId = int.Parse(segments[0]);
}
else if (segments[0] == "chathub")
{
	aliasId = 1;
}
```

# Module Dependencies so far
```C#
<dependency id="Oqtane.Framework" version="1.0.0" />      
<dependency id="System.Drawing.Common" version="5.0.0" />
<dependency id="MatBlazor" version="2.3.0" />
<dependency id="Microsoft.CSharp" version="4.7.0" />
<dependency id="BlazorStrap" version="1.3.3" />
<dependency id="Microsoft.AspNetCore.SignalR.Client" version="5.0.0" />
<dependency id="Microsoft.AspNetCore.SignalR.Protocols.MessagePack" version="5.0.0" />
<dependency id="Microsoft.AspNetCore.SignalR.Protocols.NewtonsoftJson" version="5.0.0" />
<dependency id="Microsoft.Composition" version="1.0.31" />
```

# Example Screenshots

![Module](https://github.com/boredanyway/oqtane.chathubs/blob/master/screenshot1.png?raw=true "Module")

![Module](https://github.com/boredanyway/oqtane.chathubs/blob/master/screenshot2.png?raw=true "Module")

# Demo Site

Demo Website [Link](https://anyways.tv/).

# Donation Buttom

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=DZVSWXB4L2GWA)
