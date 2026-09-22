namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    // A key that flips Logic's Division between two values you choose - say 1/16 for editing and 1/4
    // for arranging - and shows which one is set. Each press sends the matching Set Division Value
    // command, so it needs the key command file merged like the division buttons do.
    public class LogicDivisionToggleCommand : ActionEditorCommand
    {
        private const String FirstControl = "First";
        private const String SecondControl = "Second";

        private static readonly String[] DivisionIds = { "Division4", "Division16", "Division48", "Division192" };

        // Which of the two is currently set, per configured key. Remembered in plugin settings so
        // the key face is right after a restart.
        private readonly ConcurrentDictionary<UInt64, Boolean> _secondIsSet = new();

        public LogicDivisionToggleCommand()
        {
            this.Name = "LogicDivisionToggle";
            this.DisplayName = "Division Toggle *";
            this.GroupName = "Transport";
            this.Description = "Switch Logic's Division between two values with one key";

            this.ActionEditor.AddControlEx(new ActionEditorListbox(FirstControl, "First division:"));
            this.ActionEditor.AddControlEx(new ActionEditorListbox(SecondControl, "Second division:"));
            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;
        }

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            var isFirst = e.ControlName.EqualsNoCase(FirstControl);
            if (!isFirst && !e.ControlName.EqualsNoCase(SecondControl))
            {
                return;
            }

            foreach (var id in DivisionIds)
            {
                e.AddItem(id, DivisionName(id), null);
            }

            e.SelectSavedOrDefault(isFirst ? "Division4" : "Division16");
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            var key = actionParameters.GetHashCode64();
            var second = !this.Current(key, actionParameters);
            var id = actionParameters.GetString(second ? SecondControl : FirstControl, second ? "Division16" : "Division4");
            var command = LogicKeyCommands.Find(id);
            if (command == null)
            {
                return false;
            }

            LogicKeySender.Send(this.Plugin, command.Key);
            this._secondIsSet[key] = second;
            this.Plugin.SetPluginSetting(Setting(key), second ? "second" : "first");
            this.ActionImageChanged();
            return true;
        }

        protected override String GetCommandDisplayName(ActionEditorActionParameters actionParameters)
        {
            var key = actionParameters.GetHashCode64();
            var second = this.Current(key, actionParameters);
            return DivisionName(actionParameters.GetString(second ? SecondControl : FirstControl, second ? "Division16" : "Division4"));
        }

        private Boolean Current(UInt64 key, ActionEditorActionParameters actionParameters)
        {
            if (!this._secondIsSet.TryGetValue(key, out var second))
            {
                second = this.Plugin.TryGetPluginSetting(Setting(key), out var saved) && saved == "second";
                this._secondIsSet[key] = second;
            }

            return second;
        }

        private static String Setting(UInt64 key) => $"DivisionToggle:{key:X}";

        // "Division 1/16 *" -> "1/16", for the key face and the dropdowns.
        private static String DivisionName(String id) =>
            LogicKeyCommands.KeyFaceName(LogicKeyCommands.Find(id)?.DisplayName ?? id).Replace("Division ", "");
    }
}
