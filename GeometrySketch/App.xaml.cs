using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace GeometrySketch
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>        
        
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected async override void OnFileActivated(FileActivatedEventArgs args)
        {
            MainPage mainPage = null;

            ContentDialog CD_AuthorizationMessage = new ContentDialog()
            {
                Title = "Fehlende Berechtigung",
                Content = "Ihnen fehlt die Berechtigung auf die Datei zuzugreifen, da Sie evtl. verschoben wurde?  Öffnen sie die Skizze stattdessen über den Button 'Öffnen' oder erteilen Sie GeometrySketch unter Datenschutzeinstellungen die entsprechende Berechtigung.",
                CloseButtonText = "OK",
                PrimaryButtonText = "Datenschutzeinstellungen"               
            };
            CD_AuthorizationMessage.PrimaryButtonClick += CD_AuthorizationMessage_PrimaryButtonClick;

            if (Window.Current.Content is MainPage)
            {
                mainPage = Window.Current.Content as MainPage;
                Window.Current.Content = mainPage;
                var fileArgs = args as FileActivatedEventArgs;
                string _filePath = fileArgs?.Files[0].Path;
                if (mainPage.SaveNecessity == true)
                {
                    ContentDialogResult result = await mainPage.CD_SaveQuery.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        await mainPage.ViewModel.FileSaveAsync(mainPage.CurrentInkCanvas);
                        try
                        {
                            await mainPage.ViewModel.AppFileOpenAsync(_filePath, mainPage.CurrentInkCanvas);
                        }
                        catch
                        {
                            CD_AuthorizationMessage.ShowAsync();
                        }
                        mainPage.ViewModel.ProgressRingActive = false;
                    }
                    else if (result == ContentDialogResult.Secondary)
                    {
                        try
                        {
                            await mainPage.ViewModel.AppFileOpenAsync(_filePath, mainPage.CurrentInkCanvas);
                        }
                        catch
                        {
                            CD_AuthorizationMessage.ShowAsync();
                        }
                        mainPage.ViewModel.ProgressRingActive = false;
                    }
                    else
                    {

                    }
                }
                else
                {
                    try
                    {
                        await mainPage.ViewModel.AppFileOpenAsync(_filePath, mainPage.CurrentInkCanvas);
                    }
                    catch
                    {
                        CD_AuthorizationMessage.ShowAsync();
                    }
                    mainPage.ViewModel.ProgressRingActive = false;
                }
            }
            else
            {
                mainPage = new MainPage();
                Window.Current.Content = mainPage;
                var fileArgs = args as FileActivatedEventArgs;
                string _filePath = fileArgs?.Files[0].Path;
                try
                {
                    await mainPage.ViewModel.AppFileOpenAsync(_filePath, mainPage.CurrentInkCanvas);
                }
                catch
                {
                    CD_AuthorizationMessage.ShowAsync();
                }
                mainPage.ViewModel.ProgressRingActive = false;
                mainPage.ViewModel.ProgressRingActive = false;
            }
            Window.Current.Activate();
        }

        private async void CD_AuthorizationMessage_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-Settings:privacy-broadfilesystemaccess"));
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird. Weitere Einstiegspunkte
        /// werden z. B. verwendet, wenn die Anwendung gestartet wird, um eine bestimmte Datei zu öffnen.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Frame erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Frame im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                    // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                    // übergeben werden
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Sicherstellen, dass das aktuelle Fenster aktiv ist
                Window.Current.Activate();
                // Extend acrylic
                ExtendAcrylicIntoTitleBar();
            }
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Navigation auf eine bestimmte Seite fehlschlägt
        /// </summary>
        /// <param name="sender">Der Rahmen, bei dem die Navigation fehlgeschlagen ist</param>
        /// <param name="e">Details über den Navigationsfehler</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }

        private void ExtendAcrylicIntoTitleBar()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
        }
    }
}
