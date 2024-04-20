
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.SDK3.Components;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Data;
using TMPro;


namespace nekomimiStudio.ytScraper
{
    public class YoutubeScraper : UdonSharpBehaviour
    {
        // [SerializeField] VRCUrl search;
        [SerializeField] VRCUrlInputField urlField;
        [SerializeField] VRCUrlInputField urlField2;
        // [SerializeField] string aaaaa;
        // [SerializeField] Yamadev.YamaStream.VideoInfoDownloader videoInfoDownloader;
        void Start()
        {
            transform.GetChild(0).gameObject.SetActive(false);
            urlField.SetUrl(VRCUrl.Empty);
        }

        public void reload()
        {
            VRCStringDownloader.LoadUrl(urlField.GetUrl(), (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);
            urlField.SetUrl(VRCUrl.Empty);

            var r = this.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(r.sizeDelta.x, 0);

            for (var i = 1; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        public void reload2()
        {
            VRCStringDownloader.LoadUrl(urlField2.GetUrl(), (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this);

            var r = this.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(r.sizeDelta.x, 0);

            for (var i = 1; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        private void yatteiki(string title, string videoUrl, string description, string channelName, string channelUrl)
        {
            yaaa++;
            var baseContainer = transform.GetChild(0);
            var container = Instantiate(baseContainer.gameObject, transform).transform;

            var rect = container.GetComponent<RectTransform>();
            var pos = rect.anchoredPosition;
            pos.x = -5;
            pos.y = -rect.sizeDelta.y * (yaaa - 1);
            rect.anchoredPosition = pos;

            var r = this.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(r.sizeDelta.x, r.sizeDelta.y + rect.sizeDelta.y);

            container.GetChild(0).GetComponent<TextMeshProUGUI>().text = title;
            container.GetChild(1).GetComponent<InputField>().text = videoUrl;
            container.GetChild(2).GetComponent<TextMeshProUGUI>().text = description;
            container.GetChild(3).GetComponent<TextMeshProUGUI>().text = channelName;
            container.GetChild(4).GetComponent<InputField>().text = channelUrl;

            Debug.Log("yatteiki: " + title);
            Debug.Log(rect.anchoredPosition);

            container.gameObject.SetActive(true);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.Log(result.Error);
        }
        int yaaa = 0;
        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            yaaa = 0;
            // if (videoInfoDownloader) videoInfoDownloader.OnStringLoadSuccess(result);
            for (var i = 1; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            var r = this.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(r.sizeDelta.x, 0);

            this.transform.parent.parent.GetComponent<ScrollRect>().verticalNormalizedPosition = 1.0f;

            Debug.Log("success");
            string json = "";
            var str = result.Result.Split('>');
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i].StartsWith("var ytInitialData = "))
                {
                    json = str[i].Substring("var ytInitialData = ".Length);
                    for (var j = i; j < str.Length; j++) json += ">" + str[j];
                    break;
                }
            }
            str = json.Split(';');
            json = str[0];
            for (var i = 1; i < str.Length; i++)
            {
                if (str[i].StartsWith("</script>"))
                    break;
                else
                    json += ";" + str[i];
            }

            DataToken token;
            if (!VRCJson.TryDeserializeFromJson(json, out token)) return;
            var ytInitialData = (DataDictionary)token;

            var root = GetChildByPath(ytInitialData, "contents");
            DataToken c;

            if ((c = GetChildByPath(root, "twoColumnSearchResultsRenderer.primaryContents.sectionListRenderer.contents[0].itemSectionRenderer.contents")).TokenType != TokenType.Error)
            {
                Debug.Log("Search");
                var itemSectionRenderer = (DataList)c;

                for (var i = 0; i < itemSectionRenderer.Count; i++)
                {
                    itemSectionRenderer.TryGetValue(i, TokenType.DataDictionary, out token);
                    var elem = (DataDictionary)token;

                    if (elem.ContainsKey("videoRenderer"))
                    {
                        Debug.Log("https://youtu.be/" + GetChildByPath(elem, "videoRenderer.videoId"));
                        Debug.Log(GetChildByPath(elem, "videoRenderer.title.runs[0].text"));
                        var description = "";
                        // VRCJson.TrySerializeToJson(elem, JsonExportType.Beautify, out token);
                        // aaaaa = (string)token;

                        DataList snippetText;

                        if ((token = GetChildByPath(elem, "videoRenderer.detailedMetadataSnippets[0].snippetText.runs")).TokenType != TokenType.Error)
                        {
                            snippetText = (DataList)token;
                        }
                        else
                        {
                            snippetText = new DataList();
                        }

                        for (var itr = 0; itr < snippetText.Count; itr++)
                        {
                            description += (string)GetChildByPath(snippetText[itr], "text");
                        }

                        yatteiki(
                          (string)GetChildByPath(elem, "videoRenderer.title.runs[0].text"),
                          "https://www.youtube.com/watch?v=" + (string)GetChildByPath(elem, "videoRenderer.videoId"),
                          description,
                          (string)GetChildByPath(elem, "videoRenderer.longBylineText.runs[0].text"),
                          "https://www.youtube.com" + (string)GetChildByPath(elem, "videoRenderer.longBylineText.runs[0].navigationEndpoint.browseEndpoint.canonicalBaseUrl")
                        );
                    }
                    else if (elem.ContainsKey("reelShelfRenderer"))
                    {
                        Debug.Log("Shorts");
                        var shorts = (DataList)GetChildByPath(elem, "reelShelfRenderer.items");
                        for (var j = 0; j < shorts.Count; j++)
                        {
                            var e = GetChildByPath(shorts, "[" + j + "].reelItemRenderer");
                            Debug.Log("https://youtu.be/" + GetChildByPath(e, "videoId"));
                            Debug.Log(GetChildByPath(e, "headline.simpleText"));

                            yatteiki(
                              (string)GetChildByPath(e, "headline.simpleText"),
                              "https://www.youtube.com/watch?v=" + (string)GetChildByPath(e, "videoId"),
                              "",
                              "(Youtube Short)",
                              ""
                            );
                        }
                    }
                }
            }
            else if ((c = GetChildByPath(root, "twoColumnBrowseResultsRenderer.tabs[0].tabRenderer.content.sectionListRenderer.contents")).TokenType != TokenType.Error)
            {
                Debug.Log("Playlist or @user");
                var sectionListRenderer = (DataList)c;

                for (var i = 0; i < sectionListRenderer.Count; i++)
                {
                    var itemSectionRenderer = (DataList)GetChildByPath(sectionListRenderer[i], "itemSectionRenderer.contents");
                    for (var j = 0; j < sectionListRenderer.Count; j++)
                    {
                        if ((c = GetChildByPath(itemSectionRenderer[j], "playlistVideoListRenderer.contents")).TokenType != TokenType.Error)
                        {
                            Debug.Log("Playlist");
                            var playlistVideoListRenderer = (DataList)c;

                            for (var k = 0; k < playlistVideoListRenderer.Count; k++)
                            {
                                Debug.Log("https://youtu.be/" + GetChildByPath(playlistVideoListRenderer[k], "playlistVideoRenderer.videoId"));
                                Debug.Log(GetChildByPath(playlistVideoListRenderer[k], "playlistVideoRenderer.title.runs[0].text"));
                                yatteiki(
                                    (string)GetChildByPath(playlistVideoListRenderer[k], "playlistVideoRenderer.title.runs[0].text"),
                                    "https://www.youtube.com/watch?v=" + (string)GetChildByPath(playlistVideoListRenderer[k], "playlistVideoRenderer.videoId"),
                                    "",
                                    (string)GetChildByPath(playlistVideoListRenderer[k], "playlistVideoRenderer.shortBylineText.runs[0].text"),
                                    ""
                                );
                            }
                        }
                        else if ((c = GetChildByPath(itemSectionRenderer[j], "shelfRenderer.content.horizontalListRenderer.items")).TokenType != TokenType.Error)
                        {
                            Debug.Log("@user");
                            Debug.Log(GetChildByPath(itemSectionRenderer[j], "shelfRenderer.title.runs[0].text"));
                            var horizontalListRenderer = (DataList)c;

                            yatteiki(
                                (string)GetChildByPath(itemSectionRenderer[j], "shelfRenderer.title.runs[0].text"),
                                "https://www.youtube.com" + (string)GetChildByPath(itemSectionRenderer[j], "shelfRenderer.endpoint.commandMetadata.webCommandMetadata.url"),
                                "", "", "");

                            for (var k = 0; k < horizontalListRenderer.Count; k++)
                            {
                                Debug.Log("https://youtu.be/" + GetChildByPath(horizontalListRenderer[k], "gridVideoRenderer.videoId"));
                                Debug.Log(GetChildByPath(horizontalListRenderer[k], "gridVideoRenderer.title.simpleText"));
                                yatteiki(
                                    (string)GetChildByPath(horizontalListRenderer[k], "gridVideoRenderer.title.simpleText"),
                                    "https://www.youtube.com/watch?v=" + (string)GetChildByPath(horizontalListRenderer[k], "gridVideoRenderer.videoId"),
                                    "",
                                    (string)GetChildByPath(horizontalListRenderer[k], "gridVideoRenderer.shortBylineText.runs[0].text"),
                                    ""
                                );
                            }
                        }
                    }
                }
            }
            else if ((c = GetChildByPath(root, "twoColumnBrowseResultsRenderer.tabs[1].tabRenderer.content.richGridRenderer.contents")).TokenType != TokenType.Error)
            {
                Debug.Log("@user/video");
                var richGridRenderer = (DataList)c;

                for (var i = 0; i < richGridRenderer.Count; i++)
                {
                    richGridRenderer.TryGetValue(i, TokenType.DataDictionary, out token);
                    var elem = (DataDictionary)token;

                    if (elem.ContainsKey("richItemRenderer"))
                    {
                        Debug.Log("https://youtu.be/" + GetChildByPath(elem, "richItemRenderer.content.videoRenderer.videoId"));
                        Debug.Log(GetChildByPath(elem, "richItemRenderer.content.videoRenderer.title.runs[0].text"));
                        yatteiki(
                            (string)GetChildByPath(elem, "richItemRenderer.content.videoRenderer.title.runs[0].text"),
                           "https://www.youtube.com/watch?v=" + (string)GetChildByPath(elem, "richItemRenderer.content.videoRenderer.videoId"),
                            "", "", ""
                        );
                    }
                }
            }
            else if ((c = GetChildByPath(root, "twoColumnWatchNextResults.secondaryResults.secondaryResults.results")).TokenType != TokenType.Error)
            {
                // watch
                var secondaryResults = (DataList)c;
                for (var i = 0; i < secondaryResults.Count; i++)
                {
                    var elem = (DataDictionary)secondaryResults[i];
                    if (elem.ContainsKey("compactVideoRenderer"))
                    {
                        Debug.Log("https://youtu.be/" + GetChildByPath(elem, "compactVideoRenderer.videoId"));
                        Debug.Log(GetChildByPath(elem, "compactVideoRenderer.title.simpleText"));

                        yatteiki(
                            (string)GetChildByPath(elem, "compactVideoRenderer.title.simpleText"),
                            "https://www.youtube.com/watch?v=" + (string)GetChildByPath(elem, "compactVideoRenderer.videoId"),
                            "",
                            (string)GetChildByPath(elem, "compactVideoRenderer.longBylineText.runs[0].text"),
                            "https://www.youtube.com" + (string)GetChildByPath(elem, "compactVideoRenderer.longBylineText.runs[0].navigationEndpoint.browseEndpoint.canonicalBaseUrl")
                        );
                    }
                }
            }
            else
            {
                Debug.Log("IDK");
                // aaaaa = json;
            }
        }

        public static DataToken GetChildByPath(DataToken root, string path)
        {
            DataList query = new DataList();

            foreach (var q in path.Split('.'))
            {
                var nodeName = q;

                // if (nodeName.Contains("\\"))
                // {
                //     int esc = nodeName.IndexOf('\\');
                //     while (esc != -1 && nodeName.Length - 1 != esc)
                //     {
                //         char c = '\0';
                //         switch (nodeName[esc + 1])
                //         {
                //             case '"': c = '"'; break;
                //             case '\\': c = '\\'; break;
                //             case '/': c = '/'; break;
                //             case 'b': c = '\b'; break;
                //             case 'f': c = '\f'; break;
                //             case 'n': c = '\n'; break;
                //             case 'r': c = '\r'; break;
                //         }
                //         if (c != '\0')
                //         {
                //             nodeName = nodeName.Remove(esc, 2).Insert(esc, c.ToString());
                //         }
                //         esc = nodeName.IndexOf('\\', esc + 1);
                //     }
                // }

                string lastQ = query[query.Count - 1].String;

                if (query.Count > 1 && lastQ.EndsWith("\\"))
                    query.SetValue(query.Count - 1, lastQ.Substring(0, lastQ.Length - 1) + "." + nodeName);
                else
                    query.Add(nodeName);

                var start = nodeName.IndexOf('[');
                var end = nodeName.IndexOf(']');
                while (start != -1)
                {
                    if (start == 0 || nodeName[start - 1] != '\\')
                    {
                        if (end == -1)
                            return new DataToken(DataError.UnableToParse);

                        if (query[query.Count - 1].String.Length > start)
                            query.SetValue(query.Count - 1, query[query.Count - 1].String.Substring(0, start));
                        query.Add(nodeName.Substring(start, end - start + 1));
                    }
                    start = nodeName.IndexOf('[', start + 1);
                    end = nodeName.IndexOf(']', end + 1);
                }
            }
            DataToken elem = root;

            for (int i = 0; i < query.Count; i++)
            {
                string tag = query[i].String;
                int idx = -1;

                if (tag.StartsWith("[") && tag.EndsWith("]"))
                {
                    if (!int.TryParse(tag.Substring(1, tag.Length - 2), out idx))
                        return new DataToken(DataError.UnableToParse);
                    tag = "";
                }

                if (tag != "")
                    if (elem.TokenType == TokenType.DataDictionary)
                    {
                        elem.DataDictionary.TryGetValue(tag, out DataToken token);
                        if (token.TokenType == TokenType.Error)
                            return token;
                        elem = token;
                    }
                    else return new DataToken(DataError.TypeMismatch);


                if (idx != -1)
                    if (elem.TokenType == TokenType.DataList)
                    {
                        elem.DataList.TryGetValue(idx, out DataToken token);
                        if (token.TokenType == TokenType.Error)
                            return token;
                        elem = token;
                    }
                    else return new DataToken(DataError.TypeMismatch);
            }
            return elem;
        }
    }
}