using INIFileMan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Rules.ModList
{
    class RuleFromMeta : ModListRulesBase
    {
        public RuleFromMeta(ModListData modlistData) : base(modlistData)
        {
        }

        internal override bool IsHardRule { get => false; }

        internal override bool Condition()
        {
            var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName);

            var metaPath = Path.Combine(ModPath, "meta.ini");
            if (!File.Exists(metaPath))
                return false;

            var INI = new INIFile(metaPath);

            var metaNotes = INI.ReadINI("General", "notes");

            return metaNotes!=null && metaNotes.Contains("mlinfo::");
        }

        internal override string Description()
        {
            return "Rules from meta.ini of the mod";
        }

        internal override bool Fix()
        {
            return ParseRulesFromMeta();
        }

        private bool ParseRulesFromMeta()
        {
            var ModPath = Path.Combine(ManageSettings.GetCurrentGameModsPath(), modlistData.ModName);

            var metaPath = Path.Combine(ModPath, "meta.ini");
            if (File.Exists(metaPath))
            {
                var INI = new INIFile(metaPath);

                var metaNotes = INI.ReadINI("General", "notes");
                //var metaComments = ManageINI.GetINIValueIfExist(metaPath, "comments", "General");

                var mlinfo = GetTagInfoTextFromHTML(metaNotes, "mlinfo::").Replace("\\\\", "\\");

                if (!string.IsNullOrWhiteSpace(mlinfo))
                {
                    return ParseRules(mlinfo.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries));
                }

                //regex to capture all between ::mlinfo:: and ::
                //::mlinfo::(?:(?!::).)+::

                //read info from standalone key
                //need to think about section and key names
                if (INI.SectionExistsAndNotEmpty(ManageSettings.AIMetaINISectionName()))
                {
                    if(INI.KeyExists(ManageSettings.AIMetaINIKeyModlistRulesInfoName(), ManageSettings.AIMetaINISectionName()))
                    {
                        mlinfo = INI.ReadINI(ManageSettings.AIMetaINISectionName(), ManageSettings.AIMetaINIKeyModlistRulesInfoName());
                    }
                    if (!string.IsNullOrWhiteSpace(mlinfo))
                    {
                        return ParseRules(mlinfo.Split(new[] { @"\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                    }
                }
            }

            return false;
        }

        //internal static string mlInfoStartTag = "mlinfo::";
        //internal static string mlInfoEndTag = "::";
        /// <summary>
        /// get text from selected tag
        /// used info:https://stackoverflow.com/a/10366994
        /// made to get text from meta.ini notes of MO Mod
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        internal static string GetTagInfoTextFromHTML(string htmlString, string InfoStartTag, string InfoEndTag = "::", string Splitter = ",", string HTMLtag = "span")
        {
            try
            {
                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(htmlString);

                //html.DocumentNode.SelectNodes("//" + tag + "/text()").ToList().ForEach(x => MessageBox.Show(x.InnerHtml));
                var nodes = html.DocumentNode.SelectNodes("//" + HTMLtag + "/text()");
                if (nodes == null)
                {
                    return string.Empty;
                }
                var listOfTexts = nodes.ToList();

                var info = new List<string>();
                var loadingmlinfo = false;
                string tx;
                foreach (var text in listOfTexts)
                {
                    if (loadingmlinfo)
                    {
                        if ((tx = text.InnerHtml.TrimEnd()).EndsWith(InfoEndTag, StringComparison.InvariantCulture))
                        {
                            info.Add(tx.TrimStart().Remove(tx.Length - InfoEndTag.Length));
                            break;
                        }
                        else
                        {
                            info.Add(tx.TrimStart());
                        }
                    }
                    else
                    {
                        if ((tx = text.InnerHtml.Trim()).StartsWith(InfoStartTag, StringComparison.InvariantCulture))
                        {
                            if (tx != InfoStartTag && tx.EndsWith(InfoEndTag, StringComparison.InvariantCulture))
                            {
                                info.Add(tx.Remove(tx.Length - InfoEndTag.Length).Remove(0, InfoStartTag.Length));
                                return string.Join(Environment.NewLine, info);
                            }
                            else
                            {
                                loadingmlinfo = true;
                                info.Add(tx.Replace(InfoStartTag, string.Empty));
                            }
                        }
                    }
                }
                return string.Join(Environment.NewLine, info) + (HTMLtag == "span" ? GetTagInfoTextFromHTML(htmlString, InfoStartTag, InfoEndTag = "::", Splitter = ",", HTMLtag = "p") : string.Empty);
            }
            catch (Exception ex)
            {
                ManageLogs.Log("GetTagInfoTextFromHTML error:" + Environment.NewLine + ex);
                return string.Empty;
            }
        }
    }
}
