using System;
using System.Collections.Generic;
using System.Linq;

namespace AIHelper.Manage
{
    class ManageHTML
    {
        internal static string mlInfoStartTag = "mlinfo::";
        internal static string mlInfoEndTag = "::";
        /// <summary>
        /// get text from ыудусеув tag
        /// used info:https://stackoverflow.com/a/10366994
        /// made to get text from meta.ini notes of MO Mod
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        internal static string GetMLInfoTextFromHTML(string htmlString, string tag = "span")
        {
            try
            {
                var html = new HtmlAgilityPack.HtmlDocument();
                html.LoadHtml(htmlString);

                //html.DocumentNode.SelectNodes("//" + tag + "/text()").ToList().ForEach(x => MessageBox.Show(x.InnerHtml));

                var listOfSpanTexts = html.DocumentNode.SelectNodes("//" + tag + "/text()").ToList();
                var mlinfo = new List<string>();
                var loadingmlinfo = false;
                string tx;
                foreach (var text in listOfSpanTexts)
                {
                    if (loadingmlinfo)
                    {
                        if ((tx = text.InnerHtml.TrimEnd()).EndsWith(mlInfoEndTag, StringComparison.InvariantCulture))
                        {
                            mlinfo.Add(tx.TrimStart().Remove(tx.Length - mlInfoEndTag.Length));
                            break;
                        }
                        else
                        {
                            mlinfo.Add(tx.TrimStart());
                        }
                    }
                    else
                    {
                        if ((tx = text.InnerHtml.Trim()).StartsWith(mlInfoStartTag, StringComparison.InvariantCulture))
                        {
                            if (tx != mlInfoStartTag && tx.EndsWith(mlInfoEndTag, StringComparison.InvariantCulture))
                            {
                                mlinfo.Add(tx.Remove(tx.Length - mlInfoEndTag.Length).Remove(0, mlInfoStartTag.Length));
                            }
                            else
                            {
                                loadingmlinfo = true;
                                mlinfo.Add(tx.Replace(mlInfoStartTag, string.Empty));
                            }
                        }
                    }
                }
                return string.Join(Environment.NewLine, mlinfo);
            }
            catch
            {
                return htmlString;
            }
        }
    }
}
