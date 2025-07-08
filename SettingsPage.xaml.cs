using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace Xenia_360____Canary_;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        if (Preferences.ContainsKey("XeniaCanaryPath"))
            XeniaPathEntry.Text = Preferences.Get("XeniaCanaryPath", "");
    }

    private async void SaveSettings_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("XeniaCanaryPath", XeniaPathEntry.Text?.Trim());
        await DisplayAlert("Saved", "Settings saved successfully.", "OK");
        await Navigation.PopAsync();
    }
}
