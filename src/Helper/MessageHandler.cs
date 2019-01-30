using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using KevinHelper;

namespace KMezzenger.Helper
{
    static class MessageHandler
    {
        static AhoCorasick trieFSM;
        static Dictionary<string, string> dicEmoticons;
        static string baseUrlEmotion;

        static MessageHandler()
        {
            // load emoticons
            dicEmoticons = new Dictionary<string, string>();
            using (StreamReader sr = new StreamReader(Path.Combine(HttpRuntime.AppDomainAppPath, "Content/emoticon_list.txt")))
            {
                while (!sr.EndOfStream)
                {
                    string[] emo = sr.ReadLine().Split('\t');
                    for (int i = 2; i < emo.Length; ++i)
                    {
                        if (!string.IsNullOrEmpty(emo[i]))
                            dicEmoticons[emo[i].ToLower()] = emo[1];
                    }
                }
            }

            // build trie
            trieFSM = new AhoCorasick(true, dicEmoticons.Keys.ToArray());
            baseUrlEmotion = Path.Combine(HttpRuntime.AppDomainAppVirtualPath, "Content/Emoticons/");
        }
        internal static string replace_with_emo(string message)
        {
            List<MatchResult> results = trieFSM.Search(message);
            //results = trieFSM.FilterMatch(results);
            results.Sort();

            StringBuilder process_mess = new StringBuilder();
            int position = 0;
            foreach (MatchResult m in results)
            {
                int len_before = m.position - position;
                if (len_before < 0)
                    continue;
                else if (len_before > 0)
                {
                    string before = message.Substring(position, len_before);
                    // encode html, then append
                    process_mess.Append(escape_html(before));
                }
                // replace emo code by <img> tag
                process_mess.AppendFormat("<img src='{0}{1}'/>", baseUrlEmotion, dicEmoticons[m.keyword]);
                position = m.position + m.keyword.Length;
            }

            if (position < message.Length)
                process_mess.Append(message.Substring(position, message.Length - position));

            return process_mess.ToString();
        }

        static string escape_html(string message)
        {
            return message.Replace("&", "&amp;")
                        .Replace("<", "&lt;")
                        .Replace(">", "&gt;")
                ;
        }
    }
}