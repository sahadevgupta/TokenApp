using Android.Content;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace TokenApp.Platforms.Android.Handlers
{
    public class PlainEntryHandler : EntryHandler
    {
        protected override MauiAppCompatEditText CreatePlatformView()
        {
            var editText = new MauiAppCompatEditText(Context)
            {
                Background = null
            };

            editText.SetBackgroundColor(Colors.Transparent.ToPlatform());
            editText.SetPadding(0, 0, 0, 0);
            editText.SetSingleLine(true);

            return editText;
        }
    }
}
