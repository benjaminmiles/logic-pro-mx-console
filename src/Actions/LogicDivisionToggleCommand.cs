namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    // A key that flips Logic's Division between two values - say 1/16 for editing and 1/4 for
    // arranging - and shows which one is set. One action per pair, so the key face can carry the
    // current division: Options+ overrides the label of editor-configured actions with the name the
    // user typed, which is why this is a plain command rather than an editor one.
    //
    // Each press sends the matching Set Division Value command, so it needs the key command file
    // merged like the division buttons do.
    public class LogicDivisionToggleCommand : PluginDynamicCommand
    {
        private static readonly String[] Divisions = { "Division4", "Division16", "Division48", "Division192" };

        // Which side of each pair is set, per key. Remembered in plugin settings so the key face is
        // right after a restart.
        private readonly ConcurrentDictionary<String, Boolean> _secondIsSet = new();

        public LogicDivisionToggleCommand()
        {
            this.DisplayName = "Division Toggle";
            this.GroupName = "Not used";

            foreach (var (first, second) in Pairs())
            {
                this.AddParameter($"{first}|{second}", $"Toggle {DivisionName(first)} / {DivisionName(second)} *", "Transport");
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            var (first, second) = Split(actionParameter);
            var toSecond = !this.Current(actionParameter);
            var command = LogicKeyCommands.Find(toSecond ? second : first);
            if (command == null)
            {
                return;
            }

            LogicKeySender.Send(this.Plugin, command.Key);
            this._secondIsSet[actionParameter] = toSecond;
            this.Plugin.SetPluginSetting(Setting(actionParameter), toSecond ? "second" : "first");
            this.ActionImageChanged(actionParameter);
        }

        // The key face names the division that is set now.
        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            var (first, second) = Split(actionParameter);
            return DivisionName(this.Current(actionParameter) ? second : first);
        }

        private Boolean Current(String actionParameter)
        {
            if (!this._secondIsSet.TryGetValue(actionParameter, out var second))
            {
                second = this.Plugin.TryGetPluginSetting(Setting(actionParameter), out var saved) && saved == "second";
                this._secondIsSet[actionParameter] = second;
            }

            return second;
        }

        private static IEnumerable<(String, String)> Pairs()
        {
            for (var i = 0; i < Divisions.Length; i++)
            {
                for (var j = i + 1; j < Divisions.Length; j++)
                {
                    yield return (Divisions[i], Divisions[j]);
                }
            }
        }

        private static (String First, String Second) Split(String actionParameter)
        {
            var parts = actionParameter.Split('|');
            return (parts[0], parts.Length > 1 ? parts[1] : parts[0]);
        }

        private static String Setting(String actionParameter) => $"DivisionToggle:{actionParameter}";

        // "Division 1/16 *" -> "1/16", for the key face and the action list.
        private static String DivisionName(String id) =>
            LogicKeyCommands.KeyFaceName(LogicKeyCommands.Find(id)?.DisplayName ?? id).Replace("Division ", "");
    }
}
