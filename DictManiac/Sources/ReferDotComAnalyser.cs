using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace DictManiac.Sources
{
    class DictHtmlAnalyseResult
    {
        public string definition;
    }

    class HtmlNodeLocal
    {
        // the whole file
        public string content;
        // name of the tag (e.g. tag name of <title ....></title> is 'title')
        public string tagName;
        // including this char
        public int startIndex;
        // excluding this char
        public int endIndex;

    }

    static class ReferDotComAnalyser
    {
        public static char[] tagSectionEnd = new char[3] { ' ', '>', '/'};
        // tagStartIndex: index of '<tagName'
        public static int FindEnd(string htmlContent, int tagStartIndex, int rightLimit)
        {
            int tagNameEnd = htmlContent.IndexOfAny(tagSectionEnd, tagStartIndex, rightLimit - tagStartIndex);
            string tagName = htmlContent.Substring(tagStartIndex + 1, tagNameEnd - (tagStartIndex + 1));
            string endStr = "</" + tagName + '>';
            string sameTagStr = "<" + tagName;

            int searchStartIndex = tagStartIndex + 1;
            int endIndex = -1;
            int currentWeight = 1; // counter of the times that tags with same name appear
            int lastSameTagIndex = searchStartIndex;
            for (; ; )
            {
                int likelyEndIndex = htmlContent.IndexOf(endStr, searchStartIndex, rightLimit - searchStartIndex);
                if (likelyEndIndex < 0)
                {
                    endIndex = htmlContent.Length;
                    break;
                }

                //int sameTagSearchStart = lastSameTagIndex;

                for (; lastSameTagIndex < likelyEndIndex ; )
                {
                    int sameTagIndex = htmlContent.IndexOf(sameTagStr, lastSameTagIndex + 1, (likelyEndIndex - lastSameTagIndex));
                    if (sameTagIndex < 0)
                    {
                        lastSameTagIndex = likelyEndIndex;
                        break;
                    }

                    lastSameTagIndex = sameTagIndex;
                    int indexOfTagNameEnd = htmlContent.IndexOf('>', sameTagIndex, rightLimit - sameTagIndex);
                    // if '/>'
                    if (!(indexOfTagNameEnd > 0 && htmlContent[indexOfTagNameEnd-1] == '/'))
                        currentWeight++;
                }

                if (currentWeight == 1)
                {
                    endIndex = likelyEndIndex;
                    break;
                }
                else
                {
                    searchStartIndex = likelyEndIndex + 1;
                    currentWeight--;
                }
            }

            return endIndex;
        }

        public static string Refine(string str)
        {
            return str.Trim(' ', '"');
        }

        public static string RemoveBracketsAndStripQuotes(string str, int startIndex, int endIndex)
        {
            int nextToBeInserted = startIndex;
            int state = 0; // 0: finding left bracket, 1: left found, finding right
            string result = "";
            MyLogger.Attach("---------------------");
            for (; ; )
            {
                if (state == 0)
                {
                    int leftBIndex = str.IndexOf('<', nextToBeInserted, endIndex - nextToBeInserted);
                    if (leftBIndex < 0)
                    {
                        result += Refine(str.Substring(nextToBeInserted, endIndex - nextToBeInserted));
                        break;
                    }
                    var thisSec = Refine(str.Substring(nextToBeInserted, leftBIndex - nextToBeInserted));
                    MyLogger.Attach(thisSec);
                    result += thisSec;
                    nextToBeInserted = leftBIndex;
                    state = 1;
                }
                else if (state == 1)
                {
                    int rightBIndex = str.IndexOf('>', nextToBeInserted, endIndex - nextToBeInserted);
                    if (rightBIndex < 0)
                    {
                        break;
                    }
                    nextToBeInserted = rightBIndex + 1;
                    state = 0;
                    if (nextToBeInserted >= endIndex)
                        break;
                }
            }

            return result;
        }

        public static string DIV_IDENTIFIER = "<div class=\"luna-Ent\">";

        public static int DigIn(string htmlContent, int startIndex, int endIndex, StringBuilder sb)
        {
            int defLineIndex = htmlContent.IndexOf(DIV_IDENTIFIER, startIndex, endIndex - startIndex);
            if (defLineIndex < 0)
            {
                return -1;
            }

            int defLineEndIndex = FindEnd(htmlContent, defLineIndex, htmlContent.Length);

            int recursiveStart = defLineIndex + DIV_IDENTIFIER.Length;
            bool noChild = true;

            while (recursiveStart < endIndex)
            {
                int childIndex = DigIn(htmlContent, recursiveStart, defLineEndIndex, sb);
                if (childIndex < 0)
                    break;
                recursiveStart = childIndex;
                noChild = false;
            }

            // leaf node
            if (noChild)
            {
                string defLine = RemoveBracketsAndStripQuotes(htmlContent, defLineIndex, defLineEndIndex);
                sb.Append(defLine);
                sb.Append("\r\n");
            }

            return defLineEndIndex;
        }

        public static DictHtmlAnalyseResult AnalyseOld(string htmlContent)
        {
            DictHtmlAnalyseResult result = new DictHtmlAnalyseResult();

            //HtmlNode root = new HtmlNode { content = htmlContent, startIndex = 0, endIndex = htmlContent.Length };
            StringBuilder sb = new StringBuilder();

            DigIn(htmlContent, 0, htmlContent.Length, sb);

            result.definition = sb.ToString();

            return result;
        }

        public static DictHtmlAnalyseResult Analyse2(string htmlContent)
        {
            DictHtmlAnalyseResult result = new DictHtmlAnalyseResult();

            HtmlDocument doc = new HtmlDocument();
            StringBuilder sb = new StringBuilder();
            doc.LoadHtml(htmlContent);
            var root = doc.DocumentNode;
            HtmlNode node;
            node = root.SelectSingleNode("//*[@id=\"query_h1\"]");
            sb.Append(node.InnerText).Append("\r\n");
            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[1]/h2");
            sb.Append(node.InnerText).Append("\r\n");
            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[1]/span/span[3]/span[2]");
            sb.Append(node.InnerText).Append("\r\n");
            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/span[1]");
            sb.Append(node.InnerText).Append("\r\n");
            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/div[1]");
            sb.Append(node.InnerText).Append("\r\n");
            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/div[5]");
            sb.Append(node.InnerText).Append("\r\n");
            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[2]/div[1]");
            sb.Append(node.InnerText).Append("\r\n");

            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/div[6]");
            //sb.Append(node.InnerText).Append("\r\n");
            
            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/div[6]/div[1]/span");
            sb.Append(node.InnerText).Append("\r\n");
            node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/div[6]/div[1]/div");
            sb.Append(node.InnerText).Append("\r\n");

            result.definition = sb.ToString();
            return result;
        }

        public static DictHtmlAnalyseResult Analyse3(string htmlContent)
        {
            DictHtmlAnalyseResult result = new DictHtmlAnalyseResult();

            HtmlDocument doc = new HtmlDocument();
            StringBuilder sb = new StringBuilder();
            doc.LoadHtml(htmlContent);
            var root = doc.DocumentNode;
            HtmlNode node;
            //node = root.SelectSingleNode("//*[@id=\"query_h1\"]");
            //sb.Append(node.InnerText).Append("\r\n");
            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]/div[1]/div[1]/div[1]/div[1]/h2"); // take
            //sb.Append(node.InnerText).Append("\r\n");
            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]/div[1]/div[1]/div[1]/div[1]/span[1]/span[3]/span[2]"); // teyk
            //sb.Append(node.InnerText).Append("\r\n");
            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/span[1]");    // verb
            //sb.Append(node.InnerText).Append("\r\n");
            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[1]");
            //sb.Append(node.InnerText).Append("\r\n");
            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/div[5]");
            //sb.Append(node.InnerText).Append("\r\n");
            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]/div[1]/div/div[1]/div[2]/div[2]/div[1]");
            //sb.Append(node.InnerText).Append("\r\n");
            ////node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]/div[1]/div/div/div[2]/div[1]/div[5]");
            ////node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[4]/div[1]/div[2]/div/div/div[2]/div[1]/div[1]");

            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/div[6]");
            ////sb.Append(node.InnerText).Append("\r\n");
            
            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/div[6]/div[1]/span");
            //sb.Append(node.InnerText).Append("\r\n");
            //node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div/div/div/div[1]/div[2]/div[1]/div[6]/div[1]/div");
            //sb.Append(node.InnerText).Append("\r\n");

            sb.Clear();
            sb.Append(root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]").InnerText);
            result.definition = InsertLineBeforeNumber(sb.ToString());
            return result;
        }

        public static DictHtmlAnalyseResult Analyse(string htmlContent)
        {
            DictHtmlAnalyseResult result = new DictHtmlAnalyseResult();

            HtmlDocument doc = new HtmlDocument();
            StringBuilder sb = new StringBuilder();
            doc.LoadHtml(htmlContent);
            var root = doc.DocumentNode;
            var node = root.SelectSingleNode("//*[@id=\"rpane\"]/div[3]/div[1]"); 
            
            sb.Append(node.InnerText);

            result.definition = InsertLineBeforeNumber(sb.ToString());
            return result;
        }

        private static string InsertLineBeforeNumber(string str)
        {
            //string pattern = @"(\p{Sc}\s?)?(\d+\.?((?<=\.)\d+)?)(?(1)|\s?\p{Sc})?";
            //string input = "$17.43  €2 16.33  £0.98  0.43   £43   12€  17";
            string pattern = @"(((\d{1,3})(,\d{3})*)|(\d+))(.\d+)?\.";
            string replacement = "\r\n$1.";
            Regex rgx = new Regex(pattern);
            string result = rgx.Replace(str, replacement);
            rgx = new Regex(@"<!\-\-.*\-\->");
            result = rgx.Replace(result, "");
            return result;
        }
    }
}
