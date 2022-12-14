namespace Loupedeck.Vlc
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

    public partial class Vlc : Plugin
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly String _baseUrl = @"http://127.0.0.1:8080/requests/status.json";
        private static readonly String _playlistUrl = @"http://127.0.0.1:8080/requests/playlist.json";
        private String _redirectUrl;
        private HttpListener _listener;

        public static HttpResponseMessage ResposeMessage { get; private set; }
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

                var randomNumbers = new Random();
                var ports = Enumerable
                    .Repeat(0, 100)
                    .Select(i => randomNumbers.Next(1000, 9000))
                    .ToArray();

                if (!NetworkHelpers.TryGetFreeTcpPort(ports, out var port))
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

            if (!mrl.StartsWith("http") && !mrl.StartsWith("rtsp") && !mrl.StartsWith("ftp"))
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
            if (this.TryGetTrackInfo(out var trackInfo))
            {
                if (0 == InitialPosition || 0 == TrackLength)
                {
                    InitialPosition = trackInfo?.TrackState?.Time ?? 0;
                    TrackLength = trackInfo?.TrackState?.Length ?? 0;
                }
                this.RunTask(task);
            }

        }

        public Information GetTrackInfo()
        {
            var responseDataJo = GetDataFromResponse(ResposeData);
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
            var playlistResposeData = GetResponseString(_playlistUrl).Result;
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

        public void ConnectVlc()
        {
            if (this.TryGetPluginSetting("password", out var value))
            {
                Password = value;
                Authorize();
            }

            ResposeMessage = this.GetResponse(_baseUrl).Result;

            if (null != ResposeMessage)
            {
                ResposeData = GetResponseString(_baseUrl).Result;
                if (ResposeMessage.StatusCode == HttpStatusCode.OK)
                {
                    var data = GetDataFromResponse(ResposeData);
                    InitialVolume = null != data ? data["volume"].ToString().ParseDouble() : 256;
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "Connected", null, null);
                    this._vlcAccount.ReportLogin("", Password, "");
                }
                if (ResposeMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Warning, "Cannot connect to VLC application, please set a password");
                    this._vlcAccount.ReportLogout();
                }
            }
            else
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "VLC media player application is not running");
            }
            InitialPosition = 0;
            TrackLength = 0;
        }

        public static void Authorize()
        {
            var byteArray = Encoding.ASCII.GetBytes($":{Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
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
                HttpResponseMessage response = await client.GetAsync($"{_baseUrl}?command={commandName}");

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
                HttpResponseMessage response = await client.GetAsync($"{url}");
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
                HttpResponseMessage response = await client.GetAsync($"{url}");
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

        public Boolean SystemIsMac() => Environment.OSVersion.Platform == PlatformID.Unix;
    }
}
