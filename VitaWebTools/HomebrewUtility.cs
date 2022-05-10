using System.Diagnostics;
using System.IO.Compression;
using VitaWebTools.Entities;

namespace VitaWebTools
{
    public class HomebrewUtility
    {
        private readonly ILogger<HomebrewUtility> _logger;
        private readonly Utilities _utils;

        public HomebrewUtility(ILogger<HomebrewUtility> logger, Utilities utils)
        {
            _logger = logger;
            _utils = utils;
        }

        public async Task<List<AvailableHomebrew>> GetAvailableHomebrewsAsync()
        {
            var hbDir = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Data/Homebrews"));
            var hbList = new List<AvailableHomebrew>();

            foreach (var hb in hbDir.GetDirectories())
            {
                var sfo = SfoNET.Sfo.ReadSfo(
                    await System.IO.File.ReadAllBytesAsync(Path.Combine(hb.FullName, "sce_sys/param.sfo")));
                hbList.Add(new AvailableHomebrew(sfo["TITLE"].ToString()!, sfo["TITLE_ID"].ToString()!));
            }

            return hbList;
        }

        private readonly string[] _homebrewParts = { "savedata", "license", "appmeta", "app" };

        public async Task<MemoryStream> GetZippedHomebrewsAsync(string aid, params string[] homebrews)
        {
            var currentDir = Directory.GetCurrentDirectory();
            var cmaKey = _utils.GenerateCmaKey(aid);

            if (Directory.Exists($"{currentDir}/Data/Temp/{aid}"))
            {
                Directory.Delete($"{currentDir}/Data/Temp/{aid}", true);
            }

            foreach (var homebrew in homebrews)
            {
                foreach (var genPart in _homebrewParts)
                {
                    if (!Directory.Exists($"{currentDir}/Data/Homebrews/{homebrew}/{genPart}"))
                        continue;

                    Directory.CreateDirectory($"{currentDir}/Data/Temp/{aid}/{homebrew.ToUpper()}/{genPart}");
                    using var imgProc = Process.Start(new ProcessStartInfo()
                    {
                        FileName = "psvimg-create",
                        Arguments = $"-m \"{currentDir}/Data/Homebrews/{homebrew.ToUpper()}/{genPart}.psvmd-dec\" -K {cmaKey} \"{currentDir}/Data/Homebrews/{homebrew.ToUpper()}/{genPart}\" \"{currentDir}/Data/Temp/{aid}/{homebrew.ToUpper()}/{genPart}\""
                    });
                    await imgProc!.WaitForExitAsync();
                }

                using var copyProc = Process.Start(new ProcessStartInfo
                {
                    FileName = "cp",
                    Arguments = $"-r \"{currentDir}/Data/Homebrews/{homebrew.ToUpper()}/sce_sys\" \"{currentDir}/Data/Temp/{aid}/{homebrew.ToUpper()}/\""
                });
                await copyProc!.WaitForExitAsync();
            }

            //No using cause File() is unhappy if it gets disposed
            var zipStream = new MemoryStream();
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var homebrew in homebrews)
                {
                    createEntryFromAny(archive, $"{currentDir}/Data/Temp/{aid}/{homebrew.ToUpper()}", aid);
                }
            }

            zipStream.Seek(0, SeekOrigin.Begin);

            try
            {
                Directory.Delete($"{currentDir}/Data/Temp/{aid}", true);
            }
            catch (Exception e)
            {
                _logger.LogError("Unable to delete users leftovers!\nReason: {0}", e);
            }

            return zipStream;
        }


        //https://stackoverflow.com/a/51514527
        private void createEntryFromAny(ZipArchive archive, string sourceName, string entryName = "")
        {
            var fileName = Path.GetFileName(sourceName);
            if (System.IO.File.GetAttributes(sourceName).HasFlag(FileAttributes.Directory))
            {
                createEntryFromDirectory(archive, sourceName, Path.Combine(entryName, fileName));
            }
            else
            {
                archive.CreateEntryFromFile(sourceName, Path.Combine(entryName, fileName), CompressionLevel.Fastest);
            }
        }

        private void createEntryFromDirectory(ZipArchive archive, string sourceDirName, string entryName = "")
        {
            string[] files = Directory.GetFiles(sourceDirName).Concat(Directory.GetDirectories(sourceDirName)).ToArray();
            foreach (var file in files)
            {
                createEntryFromAny(archive, file, entryName);
            }
        }
    }
}
