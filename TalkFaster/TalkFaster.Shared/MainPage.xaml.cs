using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TalkFaster
{
    public sealed partial class MainPage : Page
    {
        DisplayRequest m_displayRequest = new DisplayRequest();
        IPropertySet m_settings = ApplicationData.Current.LocalSettings.Values;

        public MainPage()
        {
            this.InitializeComponent();
            m_displayRequest.RequestActive();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Restore playback rate
            if (m_settings.Keys.Contains("PlaybackRateIndex"))
            {
                PlaybackRate.SelectedIndex = (int)m_settings["PlaybackRateIndex"];
            }
            else
            {
                PlaybackRate.SelectedIndex = 1;
            }
            SetPlaybackRate();

            // Restore file being played
            if (m_settings.Keys.Contains("FileToken"))
            {
                var token = (string)m_settings["FileToken"];
                try
                {
                    var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
                    await OpenFileAsync(file);
                }
                catch (Exception)
                {
                }
            }
            else
            {
                Buttons.Visibility = Visibility.Visible;
            }
        }

#if WINDOWS_PHONE_APP
        private void Open_Click(object sender, RoutedEventArgs e)
#else
        private async void Open_Click(object sender, RoutedEventArgs e)
#endif
        {
            // Select a file
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".wmv");
            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;

#if WINDOWS_PHONE_APP
            picker.PickSingleFileAndContinue();
#else
            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                await OpenFileAsync(file);
            }
#endif
        }

        public async Task OpenFileAsync(StorageFile file)
        {
            try
            {
                var stream = await file.OpenAsync(FileAccessMode.Read);

                Video.AutoPlay = true;
                Video.IsFullWindow = true;
                SetPlaybackRate();
                Video.Visibility = Visibility.Visible;

                Video.SetSource(stream, file.ContentType);

                StorageApplicationPermissions.FutureAccessList.Clear();
                m_settings["FileToken"] = StorageApplicationPermissions.FutureAccessList.Add(file);
            }
            catch (Exception)
            {
            }
        }

        private void SetPlaybackRate()
        {
            int index = PlaybackRate.SelectedIndex;
            switch (index)
            {
                case 0:
                    Video.PlaybackRate = 1;
                    Video.DefaultPlaybackRate = 1;
                    break;
                case 1:
                    Video.PlaybackRate = 1.4;
                    Video.DefaultPlaybackRate = 1.4;
                    break;
                case 2:
                    Video.PlaybackRate = 2;
                    Video.DefaultPlaybackRate = 2;
                    break;
            }
            m_settings["PlaybackRateIndex"] = index;
        }

        private void Video_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Toggle button visibility
            Video.IsFullWindow = !Video.IsFullWindow;
            Buttons.Visibility = Video.IsFullWindow ? Visibility.Collapsed : Visibility.Visible;
        }

        private void PlaybackRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPlaybackRate();
        }
    }
}
