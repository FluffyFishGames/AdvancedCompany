using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal
{
    public class Command
    {
        public string CommandText;
        public string Description;
        public bool IsActive = true;
        public bool IsCheat = false;
        public bool ServerOnly = false;
        public Action<string[]> Action;
        internal static List<Command> Commands = new();
        
        public Command()
        {
            Commands.Add(this);
        }

        public static Command FindCommand(string commandText, bool isServer)
        {
            for (var i = 0; i < Commands.Count; i++)
            {
                if (Commands[i].IsActive && Commands[i].CommandText.Equals(commandText, StringComparison.OrdinalIgnoreCase) && (!Commands[i].ServerOnly || isServer))
                    return Commands[i];
            }
            return null;
        }

        public void Execute(string[] @params)
        {
            Action(@params);
        }
    }
}
