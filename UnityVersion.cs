using System.Collections.Generic;
using _UnityVersion = AssetRipper.Primitives.UnityVersion;

namespace UDGB
{
    internal class UnityVersion
    {
        internal static List<UnityVersion> VersionTbl = new List<UnityVersion>();
        internal static string UnityURL = "https://unity3d.com/get-unity/download/archive";
        internal _UnityVersion Version = _UnityVersion.MinVersion;
        internal string DownloadURL = null;
        internal string HashStr = null;
        internal bool UsePayloadExtraction = false;

        internal UnityVersion(string fullversion, string downloadurl)
        {
            Version = _UnityVersion.Parse(fullversion);

            string[] downloadurl_splices = downloadurl.Split('/');
            if (Version < _UnityVersion.Parse("5.3.99") || downloadurl_splices[4].EndsWith(".exe"))
            {
                Logger.DebugMsg($"{Version.ToStringWithoutType()} - {DownloadURL}");
                return;
            }

            UsePayloadExtraction = true;
            HashStr = downloadurl_splices[4];
            DownloadURL = $"https://download.unity3d.com/download_unity/{HashStr}/MacEditorTargetInstaller/UnitySetup-Windows-";
            if (Version >= _UnityVersion.Parse("2018.0.0"))
                DownloadURL += "Mono-";
            DownloadURL += $"Support-for-Editor-{Version}.pkg";

            Logger.DebugMsg($"{Version.ToStringWithoutType()} - {HashStr} - {DownloadURL}");
        }

        internal static void Refresh()
        {
            if (VersionTbl.Count > 0)
                VersionTbl.Clear();

            string pageSource = Program.webClient.DownloadString(UnityURL);
            if (string.IsNullOrEmpty(pageSource))
                return;

            string target = "unityHubDeepLink\\\":\\\"unityhub://";

            int next;
            while ((next = pageSource.IndexOf(target)) != -1)
            {
                pageSource = pageSource.Substring(next + target.Length);
                int end = pageSource.IndexOf("\\\"");

                if (end == -1)
                    continue;

                string url = pageSource.Substring(0, end);

                string[] parts = url.Split('/');
                string fullVersion = parts[0];
                string hash = parts[1];

                string foundUrl = $"https://download.unity3d.com/download_unity/{hash}/Windows64EditorInstaller/UnitySetup64-{fullVersion}.exe";

                VersionTbl.Add(new UnityVersion(fullVersion, foundUrl));
            }

            VersionTbl.Reverse();
        }
    }
}
