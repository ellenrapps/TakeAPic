using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.UI.Popups;
using Windows.ApplicationModel;
using Windows.System.Display;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Media.MediaProperties;
using Windows.Storage;
using System.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace photo221220200302pm
{
    public sealed partial class MainPage : Page
    {
        private MediaCapture erMediaCapture;
        private readonly DisplayRequest displayRequest = new DisplayRequest();


        public MainPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Disabled;
            Application.Current.Suspending += Application_Suspending;
            Application.Current.Resuming += Application_Resuming;
        }

        #region Suspending, Resuming
        private void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            Dispose();
            deferral.Complete();
        }

        private async void Application_Resuming(object sender, object o)
        {
            await InitializeMediaCapture();
        }

        #endregion Suspending, Resuming

        #region Navigation
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitializeMediaCapture();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Dispose();
        }

        #endregion Navigation

        #region Init

        private async Task InitializeMediaCapture()
        {
            try
            {
                if (erMediaCapture == null)
                {
                    var cameraDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                }
                erMediaCapture = new MediaCapture();
                await erMediaCapture.InitializeAsync(new MediaCaptureInitializationSettings { });

                var resolutions = this.erMediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo);

                if (resolutions.Count >= 1)
                {
                    var hires = resolutions.OrderByDescending(item => ((VideoEncodingProperties)item).Width).First();

                    await erMediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, hires);
                }

                ErCaptureElem.Source = erMediaCapture;
                await erMediaCapture.StartPreviewAsync();
                displayRequest.RequestActive();
            }

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong. One of the causes of this issue " +
                    "is that the computer's camera and microphone are disabled. Please " +
                    "enable the computer's camera and microphone and refresh the app.");
                {
                    _ = await dialog.ShowAsync();
                }

            }
        }

        #endregion Init      

        #region Buttons
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name == "HamburgerButton")
            {
                AlterHamburger();
            }

            if ((sender as Button).Name == "TakePhotoButton")
            {
                AlterTakePhotoButton();
            }

            else if ((sender as Button).Name == "RefreshButton")
            {
                AlterReset();
            }

            else if ((sender as Button).Name == "FaqButton")
            {
                AlterFaq();
            }

        }

        #endregion Buttons

        #region Dispose
        private void Dispose()
        {
            if (erMediaCapture != null)
            {
                erMediaCapture.Dispose();
                erMediaCapture = null;
            }

            if (ErCaptureElem.Source != null)
            {
                ErCaptureElem.Source.Dispose();
                ErCaptureElem.Source = null;
            }
        }

        #endregion Dispose 

        #region AlterHamburger
        private void AlterHamburger()
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        #endregion AlterHamburger

        #region AlterTakePhotoButton
        private async void AlterTakePhotoButton()
        {
            try
            {
                    var storageFolder = KnownFolders.PicturesLibrary;
                    var file = await storageFolder.CreateFileAsync("Photo.jpg", CreationCollisionOption.GenerateUniqueName);
                    await erMediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), file);
                    var dialog = new MessageDialog("Success! Your photo is saved in the Pictures folder.");
                    {
                        _ = await dialog.ShowAsync();
                    }
                    TakePhotoButton.IsEnabled = false;
                    await Task.Delay(01);
                    TakePhotoButton.IsEnabled = true;

            }
           

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong. " +
                    "Possible cause of error #1: Computer's camera and microphone are disabled. Please enable " +
                    "the computer's camera and microphone and refresh the app. " +
                    "Possible cause of error #2: Don't double click the Take a Photo button. " +
                    "Give the camera one second to prepare to take another photo.");
                   
                {
                    _ = await dialog.ShowAsync();
                }

            }
        }

        #endregion AlterTakePhotoButton

        #region AlterReset 
        private async void AlterReset()
        {
            try
            {
                var result = await CoreApplication.RequestRestartAsync("Application Restart Programmatically ");

                if (result == AppRestartFailureReason.NotInForeground ||
                    result == AppRestartFailureReason.RestartPending ||
                    result == AppRestartFailureReason.Other)
                {
                    var msgBox = new MessageDialog("Restart Failed", result.ToString());
                    await msgBox.ShowAsync();
                }
            }

            catch (Exception)
            {
                var dialog = new MessageDialog("Something is wrong.");
                {
                    _ = await dialog.ShowAsync();
                }

            }
        }
        #endregion AlterReset              

        #region AlterFaq
        private void AlterFaq()
        {
            this.Frame.Navigate(typeof(FaqPage));
        }

        #endregion AlterFaq

    }
}