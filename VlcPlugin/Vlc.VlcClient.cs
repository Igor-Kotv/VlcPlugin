﻿namespace Loupedeck.Vlc
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;

    using Newtonsoft.Json.Linq;

    public partial class Vlc : Plugin
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly String _baseUrl = @"http://127.0.0.1:8080/requests/status.json";
        private static readonly String _playlistUrl = @"http://127.0.0.1:8080/requests/playlist.json";
        private static readonly String _authPagePath = @"Loupedeck/PluginData/Vlc/AuthorizationPage.html";

        public static HttpResponseMessage ResposeMessage { get; private set; }
        public static String ResposeData { get; set; }

        public static String PlaylistResposeData { get; set; }

        public static String Password { get; set; } = "";

        public static Double InitialVolume { get; set; } = 256;
        public static Double InitialPosition { get; set; } = 0;

        public static Double TrackLength { get; set; } = 0;

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
            if (TryGetTrackInfo(out var trackInfo))
            {
                if (0 == InitialPosition || 0 == TrackLength)
                {
                    InitialPosition = trackInfo?.TrackState?.Time ?? 0;
                    TrackLength = trackInfo?.TrackState?.Length ?? 0;
                }
                this.RunTask(task);
            }

        }

        public static Information GetTrackInfo()
        {
            var trackInfo = new Information();
            var responseDataJo = GetDataFromResponse(ResposeData);
            if (null == responseDataJo)
            {
                return null;
            }
            trackInfo.TrackState.State = responseDataJo["state"]?.ToString();
            trackInfo.TrackState.Loop = responseDataJo["loop"].ToString() == "True";
            trackInfo.TrackState.Repeat = responseDataJo["repeat"].ToString() == "True";
            trackInfo.TrackState.Random = responseDataJo["random"].ToString() == "True";
            trackInfo.TrackState.Fullscreen = responseDataJo["fullscreen"].ToString() == "True";
            trackInfo.TrackState.Time = responseDataJo["time"].ToString().ParseDouble();
            trackInfo.TrackState.Length = responseDataJo["length"].ToString().ParseDouble();

            var responseJo = responseDataJo["information"]?["category"]?["meta"];

            if (null != responseJo)
            {
                trackInfo.Category.Meta.Album = responseJo["album"]?.ToString();
                trackInfo.Category.Meta.Title = responseJo["title"]?.ToString();
                trackInfo.Category.Meta.TrackNumber = responseJo["track_number"]?.ToString();
                trackInfo.Category.Meta.ArtworkUrl = HttpUtility.UrlDecode(responseJo["artwork_url"]?.ToString().Replace(@"file:///", ""));
            }

            return trackInfo;
        }

        public static Boolean TryGetTrackInfo(out Information trackInfo)
        {
            trackInfo = GetTrackInfo();
            return null != trackInfo;
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

            if (this.TryGetReponseMessage(out var responseMessage))
            {
                ResposeMessage = responseMessage;
                ResposeData = GetResponseString(_baseUrl).Result;
                if (ResposeMessage.StatusCode == HttpStatusCode.OK)
                {
                    var data = GetDataFromResponse(ResposeData);
                    InitialVolume = null != data ? data["volume"].ToString().ParseDouble() : 256;
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Normal, "Connected", null, null);
                }
                if (ResposeMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    this.OnPluginStatusChanged(Loupedeck.PluginStatus.Warning, "Cannot connect to VLC application, please set a password", this.GetAuthUrl(), "Password form");
                }
            }
            else
            {
                this.OnPluginStatusChanged(Loupedeck.PluginStatus.Error, "Please start VLC media player application", null, null);
            }
            InitialPosition = 0;
            TrackLength = 0;
        }

        private Boolean TryGetReponseMessage(out HttpResponseMessage responseMessage)
        {
            var response = this.GetResponse(_baseUrl);
            if (null != response.Result)
            {
                responseMessage = response.Result;
                return true;
            }

            responseMessage = null;
            return false;
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

        public static JObject GetDataFromResponse(String playlistData) => null != playlistData && ResposeMessage.IsSuccessStatusCode ? JObject.Parse(playlistData) : null;

        private String GetAuthUrl()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix
                ? $"file:///Users/{Environment.UserName}/.local/share/{_authPagePath}"
                : $"file:/C:/Users/{Environment.UserName}/AppData/Local/{_authPagePath}";
        }
    }
}