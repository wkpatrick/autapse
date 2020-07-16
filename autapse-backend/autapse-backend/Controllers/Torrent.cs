using System.Collections.Generic;
using System.IO;
using System.Net;
using autapse_backend.Models;
using BencodeNET.Parsing;
using LazyCache;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace autapse_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors]
    public class Torrent : Controller
    {
        private readonly IAppCache cache;
        
        // GET
        public Torrent(IAppCache cache)
        {
            this.cache = cache;
        }

        public string Index()
        {
            return "Yeet";
        }

        /**
        [HttpGet("saved")]
        public List<ParsedTorrent> GetCachedTorrent([FromBody] List<string> hashes)
        {
            var retList = new List<ParsedTorrent>();
            foreach (var hash in hashes)
            {
                retList.Add(cache.Get<ParsedTorrent>(hash));
            }
            return retList;
        } **/
        
        [HttpPost("saved")]
        public ParsedTorrent GetCachedTorrent([FromBody] string hash)
        {
            return cache.Get<ParsedTorrent>(hash);
        }

        //Takes in a list of urls, sends back the list as an an array of uuids, torrent info
        [HttpPost("url")]
        public List<ParsedTorrent> GetTorrentsFromURLs([FromBody] List<string> urls)
        {
            var myWebClient = new WebClient();
            var parser = new BencodeParser();
            
            var retList = new List<ParsedTorrent>();
            foreach (var url in urls)
            {
                var torrent = new ParsedTorrent(myWebClient.DownloadData(url));
                torrent.ParsedData = parser.Parse<BencodeNET.Torrents.Torrent>(torrent.Data);
                torrent.ExtractData();
                retList.Add(torrent);
                cache.Add(torrent.InfoHash, torrent);
            }
            return retList;
        }

        [HttpPost("file")]
        public List<ParsedTorrent> GetTorrentsFromFiles([FromForm] List<IFormFile> files)
        {
            var retList = new List<ParsedTorrent>();
            var parser = new BencodeParser();

            foreach (var file in files)
            {
                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var torrent = new ParsedTorrent(ms.ToArray());
                    torrent.ParsedData = parser.Parse<BencodeNET.Torrents.Torrent>(torrent.Data);
                    torrent.ExtractData();
                    cache.Add(torrent.InfoHash, torrent);
                    retList.Add(torrent);
                }
            }
            return retList;
        }
    }
}