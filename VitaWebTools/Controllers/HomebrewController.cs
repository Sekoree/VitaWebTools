using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using VitaWebTools.Entities;

namespace VitaWebTools.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomebrewController : Controller
    {
        private readonly ILogger<HomebrewController> _logger;
        private readonly HomebrewUtility _hbUtil;

        private readonly string[] homebrewParts = new[] { "savedata", "license", "appmeta", "app" };

        public HomebrewController(ILogger<HomebrewController> logger, HomebrewUtility hbUtil)
        {
            _logger = logger;
            _hbUtil = hbUtil;
        }

        [HttpGet("ping")]
        public string Ping()
        {
            return "pong";
        }

        [HttpGet("getArchive")]
        public async Task<IActionResult> GetHomebrewsArchiveAsync(string aid, [FromQuery]string[] homebrews)
        {
            var zipStream = await _hbUtil.GetZippedHomebrewsAsync(aid, homebrews);
            return File(zipStream, "application/zip", $"{aid}.zip");
        }

        [HttpGet("getHomebrews")]
        public async Task<List<AvailableHomebrew>> GetHombrewListAsync()
        {
            return await _hbUtil.GetAvailableHomebrewsAsync();
        }

    }
}
