namespace Loupedeck.VlcPlugin
{
    using System;
    using System.Collections.Generic;

    public class Information
    {
        public Int32 Chapter { get; set; }
        public List<Object> Chapters { get; set; }
        public String Title { get; set; }
        public Category Category { get; set; } = new Category();
        public TrackState TrackState { get; set; } = new TrackState();
        public List<Object> Titles { get; set; }
    }

    public class Category
    {
        public Meta Meta { get; set; } = new Meta();
    }

    public class Meta
    {
        public String TrackTotal { get; set; }
        public String Date { get; set; }
        public String ArtworkUrl { get; set; }
        public String Artist { get; set; }
        public String Album { get; set; }
        public String TrackNumber { get; set; }
        public String Title { get; set; }
        public String Filename { get; set; }
        public String Copyright { get; set; }
    }

    public class PlaylistItem
    {
        public String Name { get; set; }
        public String Id { get; set; }
        public String Current { get; set; }
    }

    public class TrackState
    {
        public String State { get; set; }
        public Boolean Loop { get; set; }
        public Boolean Repeat { get; set; }
        public Boolean Random { get; set; }
        public Double Time { get; set; }
        public Double Length { get; set; }
    }
}
