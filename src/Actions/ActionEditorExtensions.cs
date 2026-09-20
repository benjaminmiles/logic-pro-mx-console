namespace Loupedeck.LogicProPlugin
{
    using System;

    public static class ActionEditorExtensions
    {
        // Keeps a listbox on the value the user saved, falling back to a default only when the
        // control has never been set.
        //
        // The event's own SelectedItemName arrives empty even for a saved action — the stored value
        // lives in the editor state — so selecting a default from SelectedItemName alone overwrites
        // the user's choice every time the editor reopens.
        public static void SelectSavedOrDefault(this ActionEditorListboxItemsRequestedEventArgs e, String defaultItemName)
        {
            var saved = e.ActionEditorState?.GetControlValue(e.ControlName);
            var selection = !String.IsNullOrEmpty(saved) ? saved
                : !String.IsNullOrEmpty(e.SelectedItemName) ? e.SelectedItemName
                : defaultItemName;

            PluginLog.Verbose($"Editor '{e.ControlName}': saved '{saved}', selecting '{selection}'");
            e.SetSelectedItemName(selection);
        }
    }
}
