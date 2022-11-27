using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AIHelper.Manage.Rules.ModList
{
    class RuleFromMeta : ModListRulesBase
    {
        public RuleFromMeta(ModListRulesData modlistData) : base(modlistData)
        {
        }

        internal override bool IsHardRule { get => false; }

        internal override bool Condition()
        {
            var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name);

            var metaPath = Path.Combine(modPath, "meta.ini");
            if (!File.Exists(metaPath))
                return false;

            var ini = ManageIni.GetINIFile(metaPath);

            var metaNotes = ini.GetKey("General", "notes");

            return metaNotes != null && metaNotes.Contains("mlinfo::");
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
            var modPath = Path.Combine(ManageSettings.CurrentGameModsDirPath, ModlistData.Mod.Name);

            var metaPath = Path.Combine(modPath, "meta.ini");
            if (!File.Exists(metaPath)) return false;

            var ini = ManageIni.GetINIFile(metaPath);

            var metaNotes = ini.GetKey("General", "notes");
            //var metaComments = ManageINI.GetINIValueIfExist(metaPath, "comments", "General");

            var mlinfo = GetTagInfoTextFromHtml(metaNotes, "mlinfo::").Replace("\\\\", "\\");

            if (!string.IsNullOrWhiteSpace(mlinfo))
            {
                return ParseRules(mlinfo.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries));
            }

            //regex to capture all between ::mlinfo:: and ::
            //::mlinfo::(?:(?!::).)+::

            //read info from standalone key
            //need to think about section and key names
            if (!ini.SectionExistsAndNotEmpty(ManageSettings.AiMetaIniSectionName)) return false;

            if (ini.KeyExists(ManageSettings.AiMetaIniKeyModlistRulesInfoName, ManageSettings.AiMetaIniSectionName))
            {
                mlinfo = ini.GetKey(ManageSettings.AiMetaIniSectionName, ManageSettings.AiMetaIniKeyModlistRulesInfoName);
            }
            if (!string.IsNullOrWhiteSpace(mlinfo))
            {
                return ParseRules(mlinfo.Split(new[] { @"\r\n" }, StringSplitOptions.RemoveEmptyEntries));
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
        internal static string GetTagInfoTextFromHtml(string htmlString, string infoStartTag, string infoEndTag = "::", string splitter = ",", string htmLtag = "span")
        {
            try
            {
                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(htmlString);

                //html.DocumentNode.SelectNodes("//" + tag + "/text()").ToList().ForEach(x => MessageBox.Show(x.InnerHtml));
                var nodes = html.DocumentNode.SelectNodes("//" + htmLtag + "/text()");
                if (nodes == null) return string.Empty;

                var listOfTexts = nodes.ToList();

                var info = new List<string>();
                var loadingmlinfo = false;
                string tx;
                foreach (var text in listOfTexts)
                {
                    if (loadingmlinfo)
                    {
                        if ((tx = text.InnerHtml.TrimEnd()).EndsWith(infoEndTag, StringComparison.InvariantCulture))
                        {
                            info.Add(tx.TrimStart().Remove(tx.Length - infoEndTag.Length));
                            break;
                        }
                        else
                        {
                            info.Add(tx.TrimStart());
                        }
                    }
                    else
                    {
                        if ((tx = text.InnerHtml.Trim()).StartsWith(infoStartTag, StringComparison.InvariantCulture))
                        {
                            if (tx != infoStartTag && tx.EndsWith(infoEndTag, StringComparison.InvariantCulture))
                            {
                                info.Add(tx.Remove(tx.Length - infoEndTag.Length).Remove(0, infoStartTag.Length));
                                return string.Join(Environment.NewLine, info);
                            }
                            else
                            {
                                loadingmlinfo = true;
                                info.Add(tx.Replace(infoStartTag, string.Empty));
                            }
                        }
                    }
                }
                return string.Join(Environment.NewLine, info) + (htmLtag == "span" ? GetTagInfoTextFromHtml(htmlString, infoStartTag, infoEndTag = "::", splitter = ",", htmLtag = "p") : string.Empty);
            }
            catch (Exception ex)
            {
                _log.Error("GetTagInfoTextFromHTML error:" + Environment.NewLine + ex);
                return string.Empty;
            }
        }
    }
}
