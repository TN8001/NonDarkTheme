using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;

namespace NonDarkTheme.Utility
{
    /// <summary>GitHubから新しいバージョンが出ていないか確認する</summary>
    public static class UpdateChecker
    {
        /// <summary>GitHubから新しいバージョンが出ていないか確認する
        /// 新版があれば「2018/04/21 Ver1.2.0 新バージョン通知機能」といった文字列を返す
        /// すでに最新の場合はstring.Emptyを返す</summary>     
        /// <param name="url">GitHubリポジトリのurl</param>
        /// <param name="version">比較するVersion</param>
        /// <returns></returns>
        public static string GetNewVersionString(string url, Version version = null)
        {
            try
            {
                if(version == null)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    version = assembly.GetName().Version;
                }

                // リビジョンを無視
                version = new Version(version.Major, version.Minor, version.Build);

                // どうせリリースミスで何度もappveyorする羽目になるので
                // 満足できる状態になったらGitHub releasesに手動でコメントを入れる
                // コメントを入れるまでは最新版扱いをしない
                var feeds = GetFeeds(url + "/releases.atom")
                    .Where(x => x.Version > version && x.ContentIsExist)
                    .Select(x => x.ToString());

                return string.Join("\n", feeds);
            }
            catch // なにかあっても落とさずにとりあえず動作するように
            {
                return "";
            }
        }

        private static IEnumerable<Feed> GetFeeds(string url)
        {
            using(var xr = XmlReader.Create(url))
            {
                var feed = SyndicationFeed.Load(xr);
                return feed.Items.Select(x => new Feed
                {
                    Version = new Version(Regex.Replace(x?.Title?.Text ?? "0.0.0", "[^0-9.]", "")),
                    Content = (x?.Content as TextSyndicationContent)?.Text,
                    Text = Regex.Replace((x?.Content as TextSyndicationContent)?.Text ?? "", "<[^>]*?>", "").Replace("\n", " "),
                    Link = x?.Links.FirstOrDefault()?.Uri,
                    Updated = x?.LastUpdatedTime,
                    Autor = x?.Authors.FirstOrDefault()?.Name,
                }).ToArray();
            }
        }

        private class Feed
        {
            public Version Version;
            public string Content;
            public string Text;
            public Uri Link;
            public DateTimeOffset? Updated;
            public string Autor;

            public bool ContentIsExist => !string.IsNullOrEmpty(Content) && Content != "No content.";

            public override string ToString() => $"{Updated?.LocalDateTime:d} Ver{Version} {Text}";
        }
    }
}
