using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AdvancedCompany.Terminal
{
    public class Utils
    {
        public static string WrapText(string text, int lineLength, string padLeft = "", string padRight = "", bool padLeftFirst = true)
        {
            var first = true;
            var trueLineLength = 0;
            var newText = "";

            var currentLine = "";
            var currentLinePos = 0;
            var lastPossibleWrap = -1;
            var inTag = false;
            for (var i = 0; i < text.Length; i++)
            {
                trueLineLength = lineLength - padRight.Length - padLeft.Length;
                var c = text[i];
                if (c == ' ')
                    lastPossibleWrap = currentLine.Length;
                if (c == '<')
                    inTag = true;
                if (c != '\n')
                    currentLine += c;
                if (!inTag && c != '\n')
                    currentLinePos++;
                if (c == '>' && inTag)
                    inTag = false;
                if (currentLinePos > trueLineLength || c == '\n')
                {
                    var pLeft = (padLeftFirst || !first ? padLeft : "");
                    if (c != ' ' && c != '\n')
                    {
                        if (lastPossibleWrap == -1)
                        {
                            var l = pLeft + currentLine + padRight;
                            newText += l + "\n";
                            currentLine = "";
                            currentLinePos = 0;
                        }
                        else
                        {
                            var l = pLeft + currentLine.Substring(0, lastPossibleWrap) + new string(' ', (int)System.Math.Max(0, trueLineLength - lastPossibleWrap)) + padRight;
                            newText += l + "\n";
                            currentLine = currentLine.Substring(lastPossibleWrap + 1);
                            currentLinePos = currentLine.Length;
                        }
                        lastPossibleWrap = -1;
                        first = false;
                    }
                    else
                    {
                        if (currentLine != "")
                        {
                            newText += pLeft + currentLine + new string(' ', (int)System.Math.Max(0, trueLineLength - currentLinePos)) + padRight + "\n";
                        }
                        currentLine = "";
                        currentLinePos = 0;
                        lastPossibleWrap = -1;
                        first = false;
                    }
                }
            }
            if (currentLine != "")
            {
                newText += (padLeftFirst || !first ? padLeft : "") + currentLine + new string(' ', (int)System.Math.Max(0, trueLineLength - currentLinePos)) + padRight + "\n";
            }
            return newText;
            /*
            var first = true;
            var trueLineLength = lineLength - padLeft.Length - padRight.Length;
            var newText = "";

            var originalText = text;
            int pos = 0;
            while (pos < text.Length)
            {
                var end = (pos + trueLineLength >= text.Length) ? text.Length - 1 : pos + trueLineLength;

                var ind = text.LastIndexOf(' ', end);
                var ind2 = text.IndexOf('\n', pos);
                if (ind2 != -1 && ind2 <= end)
                    ind = ind2;
                if (end == text.Length - 1)
                    ind = text.Length;
                if (ind < pos) ind = -1;
                var len = ind == -1 ? (text.Length - pos < trueLineLength ? text.Length - pos : trueLineLength) : ind - pos;
                if (len > trueLineLength) len = trueLineLength;
                var line = text.Substring(pos, len);
                var result = $"{(!padLeftFirst && first ? "" : padLeft)}{line}{(line.Length < trueLineLength ? new String(' ', trueLineLength - line.Length) : "")}{padRight}\n";
                newText += result;
                first = false;
                pos = pos + len + (ind == -1 ? 0 : 1);
            }

            return newText;*/
        }
    }
}
