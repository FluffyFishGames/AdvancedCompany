using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
/*
namespace AdvancedCompany.Terminal
{
    public class Help
    {
        public static void Show()
        {
            List<string> commandLines = new List<string>();
            foreach (var node in Patches.Terminal.Instance.terminalNodes.allKeywords)
            {
                if (node.word == "help")
                    commandLines.AddRange(node.specialKeywordResult.displayText.Split('\n'));
                if (node.word == "other")
                    commandLines.AddRange(node.specialKeywordResult.displayText.Split('\n'));
            }
            string currentCommand = "";
            string currentCommandDescription = "";
            List<(string, string)> commands = new List<(string, string)>();
            for (var i = 0; i < commandLines.Count; i++)
            {
                var line = commandLines[i];
                if (line.StartsWith(">"))
                {
                    if (currentCommand != "")
                    {
                        if (!currentCommand.Equals("other", StringComparison.OrdinalIgnoreCase))
                        {
                            commands.Add((currentCommand, currentCommandDescription));
                            currentCommandDescription = "";
                        }
                        else
                            currentCommandDescription = "";
                    }
                    currentCommand = line.Substring(1);
                }
                else if (line.Trim() != "") currentCommandDescription += line + "\n";
            }
            if (currentCommand != "")
                commands.Add((currentCommand, currentCommandDescription));

            var text = "";
            foreach (var command in commands)
                text += "> " + command.Item1 + "\n" + Utils.WrapText(command.Item2, 51, "  ");

            foreach (var command in Command.Commands)
                if (command.IsActive && !command.IsCheat && (!command.ServerOnly || NetworkManager.Singleton.IsServer))
                    text += "> " + command.CommandText.ToUpperInvariant() + "\n" + Utils.WrapText(command.Description, 51, "  ") + "\n";
            
            Patches.Terminal.SetText(text);
        }
    }
}
*/