#if WINDOWS
using System;
using Microsoft.Maui.Controls;
using WinRT.Interop;

namespace Xenia_360____Canary_.Helpers;

public static class WindowHelper
{
    public static IntPtr GetWindowHandle(Microsoft.Maui.Controls.Window mauiWindow)
    {
        // Here explicitly reference native WinUI window
        var nativeWindow = mauiWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        if (nativeWindow == null)
            throw new InvalidOperationException("Unable to get native window handle.");

        return WindowNative.GetWindowHandle(nativeWindow);
    }
}
#endif
