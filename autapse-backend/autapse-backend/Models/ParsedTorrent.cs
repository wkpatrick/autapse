using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BencodeNET.Torrents;

namespace autapse_backend.Models
{
    public class ParsedTorrent
    {
        [JsonIgnore]
        public BencodeNET.Torrents.Torrent ParsedData { get; set; }

        public byte[] Data { get; set; }
        public string InfoHash { get; set; }
        public string DisplayName { get; set; }
        public List<String> Trackers { get; set; } = new List<string>();
        public List<String> Files { get; set; } = new List<string>();
        public string Path { get; set; } = ""; //The download path
        public bool Start { get; set; } = false; //Whether to start the torrent


        public ParsedTorrent(byte[] rawData)
        {
            Data = rawData;
        }

        public void ExtractData()
        {
            InfoHash = ParsedData.OriginalInfoHash;
            DisplayName = ParsedData.DisplayName;

            if (ParsedData.FileMode == TorrentFileMode.Multi)
            {
                var directory = ParsedData.Files.DirectoryName;
                foreach (var file in ParsedData.Files)
                {
                    Files.Add(directory + "/" + file.FileName);
                }
            }
            else
            {
                Files.Add(ParsedData.File.FileName);
            }

            foreach (var TrackerList in ParsedData.Trackers)
            {
                foreach (var tracker in TrackerList)
                {
                    Trackers.Add(tracker);
                }
            }
        }
    }
}