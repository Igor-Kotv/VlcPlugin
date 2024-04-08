namespace Loupedeck.VlcPlugin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    public partial class VlcPlugin : Plugin
    {
        private String _redirectUrl;
        private HttpListener _listener;

        private static HttpClient Client { get; set; } = new HttpClient();
        public static HttpResponseMessage ResposeMessage { get; private set; }

        private static String BaseUrl { get; set; } = "";
        private static String PlaylistUrl { get; set; } = "";

        public static String ResposeData { get; set; }

        public static String Password { get; set; } = "";

        public static Double InitialVolume { get; set; } = 256;
        public static Double InitialPosition { get; set; } = 0;

        public static Double TrackLength { get; set; } = 0;

        public void StartServer()
        {
            try
            {
                if (this._listener != null)
                {
                    return;
                }

                if (!this.TryGetTcpPort(out var port))
                {
                    return;
                }

                this._redirectUrl = $"http://localhost:{port}/";

                this._listener = new HttpListener();
                this._listener.Prefixes.Add(this._redirectUrl);
                this._listener.Start();
                this._listener.BeginGetContext(this.GetContextCallback, null);
            }
            catch (Exception e)
            {
                Tracer.Error($"Server Start error: {e.Message}", e);
            }
        }

        public Boolean TryGetTcpPort(out Int32 portNumber)
        {
            portNumber = 0;

            var randomNumbers = new Random();
            var ports = Enumerable
                .Repeat(0, 100)
                .Select(i => randomNumbers.Next(8000, 9100))
                .ToArray();

            if (NetworkHelpers.TryGetFreeTcpPort(ports, out var port))
            {
                portNumber = port;
            }
            return 0 != port;
        }

        public String GetSettingsPath()
        {
            var currentUserFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            return this.SystemIsMac() ? $"{currentUserFolder}/Library/Preferences/org.videolan.vlc/vlcrc" : $@"{currentUserFolder}\AppData\Roaming\vlc\vlcrc";
        }

        private Boolean TryFindSetting(String[] lines, String key, out String setting)
        {
            setting = lines.FirstOrDefault((line) =>
            {
                var keyPart = line.Split("=")[0];
                return keyPart.EqualsNoCase(key) || $"#{keyPart}".EqualsNoCase(key);
            });

            return !setting.IsNullOrEmpty();
        }

        public Boolean TryGetSettingValue(String key, out String value)
        {
            value = null;
            try
            {
                var filePath = this.GetSettingsPath();

                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath);

                    if (this.TryFindSetting(lines, key, out var line))
                    {
                        value = line.Split("=")?[1];
                    }
                }
                else
                {
                    Tracer.Trace("The file does not exist.");
                }
            }
            catch (Exception ex)
            {
                Tracer.Trace("An error occurred: " + ex.Message);
            }
            return null != value;
        }

        public void SetSettingValue(String key, String value)
        {
            try
            {
                var filePath = this.GetSettingsPath();

                if (File.Exists(filePath))
                {
                    var lines = File.ReadAllLines(filePath);

                    if (this.TryFindSetting(lines, key, out var line))
                    {
                        var i = Array.IndexOf(lines, line);
                        var keyPart = line.Split("=")?[0];
                        lines[i] = $"{(keyPart.StartsWithNoCase("#") ? keyPart.Substring(1) : keyPart)}={value}";
                        File.WriteAllLines(filePath, lines);
                    }
                }
                else
                {
                    Tracer.Trace("The file does not exist.");
                }
            }
            catch (Exception ex)
            {
                Tracer.Trace("An error occurred: " + ex.Message);
            }
        }

        public void SetPassword() => this.SetSettingValue("#http-password", "vlc-plugin-password");

        public void SetInterfaceSetting()
        {
            if(this.TryGetSettingValue("extraintf", out var value))
            {
                if (value != "http")
                {
                    this.SetSettingValue("extraintf", "http");
                    return;
                }
            }
            this.SetSettingValue("#extraintf", "http");
        }

        public void SetPort()
        {
            var ports = new [] { 8080, 9090 };

            if (NetworkHelpers.TryGetFreeTcpPort(ports, out var port))
            {
                this.SetSettingValue("#http-port", port.ToString());
            }
        }

        public void GetContextCallback(IAsyncResult res)
        {
            var pluginDataDirectory = this.GetPluginDataDirectory();
            var filePath = Path.Combine(pluginDataDirectory, "AuthorizationPage.html");
            var webPage = File.ReadAllText(filePath);
            var buffer = Encoding.UTF8.GetBytes(webPage);

            this._listener.BeginGetContext(this.GetContextCallback, null);
            var context = this._listener.EndGetContext(res);

            var response = context.Response;
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            response.StatusCode = 200;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        public void OpenAuthenticationUrl()
        {
            if (!this._listener.IsListening)
            {
                Tracer.Error("Server: Listener has not been started yet.");
                return;
            }

            try
            {
                if (Helpers.IsWindows())
                {
                    System.Diagnostics.Process.Start(this._redirectUrl);
                }
                else
                {
                    System.Diagnostics.Process.Start("open", this._redirectUrl);
                }
            }
            catch (Exception e)
            {
                Tracer.Error($"OpenAuthenticationUrl error opening browser: {e.Message}", e);
            }
        }

        public void Play()
        {
            var task = Action("pl_pause");
            this.RunTask(task);
            this.SetInitialTrackValues();
        }

        public void PlayTrack(String id)
        {
            var task = Action($"pl_play&id={id}");
            this.RunTask(task);
            this.SetInitialTrackValues();
        }

        public void DeleteTrack(String id)
        {
            var task = Action($"pl_delete&id={id}");
            this.RunTask(task);
            this.SetInitialTrackValues();
        }

        public void InputPlay(String inputMrl)
        {
            var mrl = Uri.EscapeDataString(inputMrl);

            if (!mrl.StartsWithNoCase("http") && !mrl.StartsWithNoCase("rtsp") && !mrl.StartsWithNoCase("ftp"))
            {
                mrl = $"file:///{mrl}";
            }
            var task = Action($"in_play&input={mrl}");
            this.RunTask(task);
            this.SetInitialTrackValues();
        }

        public void Empty()
        {
            var task = Action("pl_empty");
            this.RunTask(task);
        }

        public void ToggleRandom()
        {
            var task = Action("pl_random");
            this.RunTask(task);
        }

        public void Next()
        {
            var task = Action("pl_next");
            this.RunTask(task);
            this.SetInitialTrackValues();
        }

        public void Previous()
        {
            var task = Action("pl_previous");
            this.RunTask(task);
            this.SetInitialTrackValues();
        }

        public void Fullscreen()
        {
            var task = Action("fullscreen");
            this.RunTask(task);
        }

        public void ToggleLoop(Boolean loop, Boolean repeat)
        {
            var act = "pl_loop";
            if (loop || repeat)
            {
                act = "pl_repeat";
            }
            var task = Action(act);
            this.RunTask(task);
        }

        public void AdjustVolume(Int32 value)
        {
            var task = Action($"volume&val={value}");
            this.RunTask(task);
        }

        public void Seek(Double value)
        {
            var task = Action($"seek&val={value}");
            this.RunTask(task);
        }

        public Information GetTrackInfo()
        {
            var responseDataJo = GetDataFromResponse(GetResponseString(BaseUrl).Result);
            if (null == responseDataJo)
            {
                return null;
            }
            var trackInfo = new Information();
            trackInfo.TrackState.State = responseDataJo["state"]?.ToString();
            trackInfo.TrackState.Loop = responseDataJo["loop"].ToString().EqualsNoCase("true");
            trackInfo.TrackState.Repeat = responseDataJo["repeat"].ToString().EqualsNoCase("true");
            trackInfo.TrackState.Random = responseDataJo["random"].ToString().EqualsNoCase("true");
            trackInfo.TrackState.Fullscreen = responseDataJo["fullscreen"].ToString().EqualsNoCase("true");
            trackInfo.TrackState.Time = responseDataJo["time"].ToString().ParseDouble();
            trackInfo.TrackState.Length = responseDataJo["length"].ToString().ParseDouble();

            var responseJo = responseDataJo["information"]?["category"]?["meta"];

            if (null != responseJo)
            {
                trackInfo.Category.Meta.Album = responseJo["album"]?.ToString();
                trackInfo.Category.Meta.Title = responseJo["title"]?.ToString();
                trackInfo.Category.Meta.TrackNumber = responseJo["track_number"]?.ToString();
                var artWorkString = responseJo["artwork_url"]?.ToString();
                if (!artWorkString.IsNullOrEmpty())
                {
                    artWorkString = this.SystemIsMac() ? artWorkString.Replace(@"file:///", "/") : artWorkString.Replace(@"file:///", "");
                    trackInfo.Category.Meta.ArtworkUrl = Uri.UnescapeDataString(artWorkString);
                }
            }

            return trackInfo;
        }

        public Boolean TryGetTrackInfo(out Information trackInfo)
        {
            trackInfo = this.GetTrackInfo();
            return null != trackInfo;
        }

        public HashSet<PlaylistItem> GetPlaylistInfo()
        {
            var playlistResposeData = GetResponseString(PlaylistUrl).Result;
            if (null == GetDataFromResponse(playlistResposeData))
            {
                return null;
            }
            var responseJa = (JArray)GetDataFromResponse(playlistResposeData)?["children"];
            var playlistJo = (JArray)responseJa?[0]?["children"];

            if (null == playlistJo)
            {
                return null;
            }

            var playlist = new HashSet<PlaylistItem>();

            foreach (var item in playlistJo)
            {
                var playlistItem = new PlaylistItem();
                playlistItem.Id = item["id"].ToString();
                playlistItem.Name = item["name"].ToString();
                playlistItem.Current = item["current"]?.ToString();
                playlist.Add(playlistItem);
            }

            return playlist;
        }

        private Boolean TryGetPort(out Int32? port)
        {
            port = null;

            if (!this.ClientApplication.IsRunning())
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, $"VLC media player application is not running");
                return false;
            }

            if (this.TryGetSettingValue("#http-port", out var portString) && Int32.TryParse(portString, out var portNumber))
            {
                port = portNumber;
            }

            return null != port;
        }

        public void ConnectVlc()
        {
            if (this.TryGetPort(out var port))
            {
                BaseUrl = $"http://127.0.0.1:{port}/requests/status.json";
                PlaylistUrl = $"http://127.0.0.1:{port}/requests/playlist.json";
            }

            if (BaseUrl.IsNullOrEmpty() && PlaylistUrl.IsNullOrEmpty())
            {
                return;
            }

            if (this.TryGetPluginSetting("password", out var value))
            {
                Password = value;
            }

            this.Authorize();

            ResposeMessage = this.GetResponse(BaseUrl)?.Result;

            if (null != ResposeMessage)
            {
                var loginRequired = this.LoginRequired();
                ResposeData = GetResponseString(BaseUrl).Result;
                if (ResposeMessage.StatusCode == HttpStatusCode.OK)
                {
                    var data = GetDataFromResponse(ResposeData);
                    InitialVolume = null != data ? data["volume"].ToString().ParseDouble() : 256;
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "Connected", null, null);
                    if (loginRequired)
                    { this._vlcAccount.ReportLogin("", Password, ""); }
                }
                if (ResposeMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Warning, $"Cannot connect to VLC application, please set a password");
                    if (loginRequired)
                    { this._vlcAccount.ReportLogout(); }
                }
            }

            InitialPosition = 0;
            TrackLength = 0;
        }

        public Boolean TryGetPasswordFromVlcSettings(out String password)
        {
            password = null;

            if (this.TryGetSettingValue("http-password", out var value))
            {
                password = value;
            }

            return null != password;
        }

        public Boolean LoginRequired() => !this.TryGetPasswordFromVlcSettings(out var _);

        public void Authorize()
        {
            var byteArray = this.TryGetPasswordFromVlcSettings(out var password)
                ? Encoding.ASCII.GetBytes($":{password}")
                : Encoding.ASCII.GetBytes($":{Password}");
            if (null != byteArray)
            {
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        private void RunTask(Task<String> task)
        {
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public Boolean TryGetCoverArt(PluginImageSize imageSize, out BitmapImage coverArt)
        {
            coverArt = null;

            if (this.TryGetTrackInfo(out var trackInfo))
            {
                var filePath = trackInfo.Category.Meta.ArtworkUrl;
                if (!filePath.IsNullOrEmpty())
                {
                    coverArt = BitmapImage.FromFile(trackInfo.Category.Meta.ArtworkUrl);
                    coverArt.Resize(imageSize);
                }
            }
            return coverArt != null;
        }

        private static async Task<String> Action(String commandName)
        {

            try
            {
                HttpResponseMessage response = await Client.GetAsync($"{BaseUrl}?command={commandName}");

                var responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                return responseBody;
            }
            catch (HttpRequestException e)
            {
                Tracer.Trace("\nException Caught!");
                Tracer.Trace(e.Message);
                return null;
            }
        }

        private async Task<HttpResponseMessage> GetResponse(String url)
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync($"{url}");
                return response;
            }
            catch (HttpRequestException e)
            {
                Tracer.Trace("\nException Caught!");
                Tracer.Trace(e.Message);
                return null;
            }

        }

        private static async Task<String> GetResponseString(String url)
        {
            try
            {
                HttpResponseMessage response = await Client.GetAsync($"{url}");
                var responseString = await response.Content.ReadAsStringAsync();

                return responseString;
            }
            catch (HttpRequestException e)
            {
                Tracer.Trace("\nException Caught!");
                Tracer.Trace(e.Message);
                return null;
            }

        }

        private void SetInitialTrackValues()
        {
            var response = GetDataFromResponse(ResposeData);
            if (null != response)
            {
                InitialVolume = response["volume"].ToString().ParseDouble();
                TrackLength = response["length"].ToString().ParseDouble();
                InitialPosition = response["time"].ToString().ParseDouble();
            }
        }

        public static JObject GetDataFromResponse(String playlistData) =>
           null != ResposeMessage &&
           null != playlistData &&
           ResposeMessage.IsSuccessStatusCode ?
           JObject.Parse(playlistData) :
           null;

        public void SetupVlc()
        {
            if (this.LoginRequired())
            {
                this.SetPassword();
            }

            this.SetInterfaceSetting();
        }

        public Boolean SystemIsMac() => Environment.OSVersion.Platform == PlatformID.Unix;
    }
}
