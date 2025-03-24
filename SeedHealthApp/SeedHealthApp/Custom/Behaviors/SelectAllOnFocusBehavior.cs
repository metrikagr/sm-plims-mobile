using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SeedHealthApp.Custom.Behaviors
{
    public class SelectAllOnFocusBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry entry)
        {
            entry.Focused += OnEntryFocused;
            base.OnAttachedTo(entry);
        }

        protected override void OnDetachingFrom(Entry entry)
        {
            entry.Focused -= OnEntryFocused;
            base.OnDetachingFrom(entry);
        }

        private void OnEntryFocused(object sender, FocusEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Entry entry = (Entry)sender;
                entry.CursorPosition = 0;
                entry.SelectionLength = entry.Text == null ? 0 : entry.Text.Length;
            });
        }
    }
}
