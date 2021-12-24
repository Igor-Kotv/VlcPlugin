namespace Loupedeck.VlcPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    public partial class VlcPlugin : Plugin
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly String _baseUrl = @"http://127.0.0.1:8080/requests/status.json";
        private static readonly String _playlistUrl = @"http://127.0.0.1:8080/requests/playlist.json";
        private static readonly String _authUrl = $"file:/C:/Users/{Environment.UserName}/AppData/Local/Loupedeck/PluginData/Vlc/AuthorizationPage.html";
        private static readonly Information _trackInfo = new Information();

        public static HttpResponseMessage ResposeMessage { get; private set; }
        public static String ResposeData { get; set; }

        public static String PlaylistResposeData { get; set; }

        public static String Password { get; set; } = "";

        public static Double InitialVolume { get; set; } = 256;
        public static Double InitialPosition { get; set; } = 0;

        public static Double TrackLength { get; set; } = 0;

        public static void Play()
        {
            var task = Action("pl_pause");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
            if (null != GetDataFromResponse(ResposeData))
            {
                InitialVolume = GetDataFromResponse(ResposeData)["volume"].ToString().ParseDouble();
                TrackLength = GetDataFromResponse(ResposeData)["length"].ToString().ParseDouble();
                InitialPosition = GetDataFromResponse(ResposeData)["time"].ToString().ParseDouble();
            }
        }

        public static void PlayTrack(String id)
        {
            var task = Action($"pl_play&id={id}");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void DeleteTrack(String id)
        {
            var task = Action($"pl_delete&id={id}");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void InputPlay(String inputMrl)
        {
            var mrl = Uri.EscapeDataString(inputMrl);

            if (!mrl.StartsWith("http") && !mrl.StartsWith("rtsp") && !mrl.StartsWith("ftp"))
            {
                mrl = $"file:///{mrl}";
            }
            var task = Action($"in_play&input={mrl}");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void Empty()
        {
            var task = Action("pl_empty");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void ToggleRandom()
        {
            var task = Action("pl_random");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void Next()
        {
            var task = Action("pl_next");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void Previous()
        {
            var task = Action("pl_previous");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void Fullscreen()
        {
            var task = Action("fullscreen");
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void ToggleLoop(Boolean loop, Boolean repeat)
        {
            var act = "pl_loop";
            if (loop || repeat)
            {
                act = "pl_repeat";
            }
            var task = Action(act);
            task.ContinueWith(t => t);
            ResposeData = task.Result;
        }

        public static void AdjustVolume(Int32 value)
        {
            var task = Action($"volume&val={value}");
            task.ContinueWith(t => t);
        }

        public static void Seek(Double value)
        {
            var task = Action($"seek&val={value}");
            task.ContinueWith(t => t);
        }

        public static Information GetTrackInfo()
        {            
            var responseDataJo = GetDataFromResponse(ResposeData);
            if (null == responseDataJo)
            {
                return null;
            }
            _trackInfo.TrackState.State = responseDataJo["state"]?.ToString();
            _trackInfo.TrackState.Loop = responseDataJo["loop"].ToString() == "True";
            _trackInfo.TrackState.Repeat = responseDataJo["repeat"].ToString() == "True";
            _trackInfo.TrackState.Random = responseDataJo["random"].ToString() == "True";
            _trackInfo.TrackState.Fullscreen = responseDataJo["fullscreen"].ToString() == "True";
            _trackInfo.TrackState.Time = responseDataJo["time"].ToString().ParseDouble();
            _trackInfo.TrackState.Length = responseDataJo["length"].ToString().ParseDouble();

            var responseJo = responseDataJo["information"]?["category"]?["meta"];

            if (null != responseJo)
            {
                _trackInfo.Category.Meta.Album = responseJo["album"]?.ToString();
                _trackInfo.Category.Meta.Title = responseJo["title"]?.ToString();
                _trackInfo.Category.Meta.TrackNumber = responseJo["track_number"]?.ToString();
                _trackInfo.Category.Meta.ArtworkUrl = responseJo["artwork_url"]?.ToString();
            }

            return _trackInfo;
        }

        public static HashSet<PlaylistItem> GetPlaylistInfo()
        {
            PlaylistResposeData = GetResponseString(_playlistUrl).Result;
            if (null == GetDataFromResponse(PlaylistResposeData))
            {
                return null;
            }
            var responseJa = (JArray)GetDataFromResponse(PlaylistResposeData)?["children"];
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
                    InitialVolume = null != data ? data["volume"].ToString().ToString().ParseDouble() : 256;
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "Connected", null);
                }
                if (ResposeMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Cannot connect to VLC application, please set a password", _authUrl);
                }
            }
            else
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Please start VLC media player application", null);
            }
        }

        public static void Authorize()
        {
            var byteArray = Encoding.ASCII.GetBytes($":{Password}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
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

        public static JObject GetDataFromResponse(String plalistData) => null != plalistData && ResposeMessage.IsSuccessStatusCode ? JObject.Parse(plalistData) : null;

    }
}
