using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using GeometrySketch.ViewModels;
using GeometrySketch.DataProvider;
using Windows.UI.Core.Preview;
using Windows.UI.Core;
using GeometrySketch.Commons;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using Windows.Graphics.Printing;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Display;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.System.Threading;

namespace GeometrySketch
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public InkCanvas CurrentInkCanvas { get { return InkCanvas_GeometrySketch; } }

        private CoreDispatcher CurrentThread { get; }

        public bool SaveNecessity { get; set; } = false;

        public ContentDialog CD_SaveQuery { get; set; }

        public InkPresenterRuler Lineal { get; }
        public InkPresenterProtractor Zirkel { get; }

        private Windows.Foundation.Point _dz = new Windows.Foundation.Point(800, 799);
        public Windows.Foundation.Point DZ { get { return _dz; } set { _dz = value; } }

        public float ScaleFactor { get; set; } = 1;

        public Windows.Foundation.Point P1
        {
            get
            {
                Windows.Foundation.Point p = new Windows.Foundation.Point()
                {
                    X = _dz.X + Math.Cos((180 - ViewModel.Drehwinkel) / 180 * Math.PI) * 800,
                    Y = _dz.Y - Math.Sin((180 - ViewModel.Drehwinkel) / 180 * Math.PI) * 800
                };               
                return p;
            }
        }
        public Windows.Foundation.Point P2
        {
            get
            {
                Windows.Foundation.Point p = new Windows.Foundation.Point()
                {
                    X = _dz.X + Math.Cos((-ViewModel.Drehwinkel) / 180 * Math.PI) * 800,
                    Y = _dz.Y - Math.Sin((-ViewModel.Drehwinkel) / 180 * Math.PI) * 800
                };
                return p;
            }
        }
        public Windows.Foundation.Point P3
        {
            get
            {
                Windows.Foundation.Point p = new Windows.Foundation.Point()
                {
                    X = _dz.X + Math.Cos((90 - ViewModel.Drehwinkel) / 180 * Math.PI) * 799,
                    Y = _dz.Y - Math.Sin((90 - ViewModel.Drehwinkel) / 180 * Math.PI) * 799
                };
                return p;
            }
        }

        public CoreInputDeviceTypes CurrentInputDevices { get; set; }

        private Visibility VisibilityGeodreieck { get; set; }

        public MainPage()
        {
            ViewModel = new MainViewModel(new InkPageDataprovider(), new SettingsDataProvider());
            this.InitializeComponent();
            CurrentThread = CoreWindow.GetForCurrentThread().Dispatcher;            

            this.Loaded += MainPage_Loaded;
            App.Current.Suspending += Current_Suspending;
            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += this.OnCloseRequest;
            CD_SaveQuery = new ContentDialog()
            {
                Title = "GeometrySketch",
                Content = "Wollen Sie die aktuelle Skizze zuerst speichern?",
                PrimaryButtonText = "Ja",
                SecondaryButtonText = "Nein",
                CloseButtonText = "Abbrechen",
                RequestedTheme = this.RequestedTheme,
            };

            Lineal = new InkPresenterRuler(InkCanvas_GeometrySketch.InkPresenter)
            {
                Length = 2000,
                Width = 175,
                IsVisible = false,
                IsCompassVisible = true,
                AreTickMarksVisible = false
            };
            Zirkel = new InkPresenterProtractor(InkCanvas_GeometrySketch.InkPresenter)
            {
                IsVisible = false,
                IsAngleReadoutVisible = false,
            };            
            VisibilityGeodreieck = Visibility.Collapsed;

            coreInkIndependentInputSource = CoreInkIndependentInputSource.Create(InkCanvas_GeometrySketch.InkPresenter);            
            coreWetStrokeUpdateSource = CoreWetStrokeUpdateSource.Create(InkCanvas_GeometrySketch.InkPresenter);
            CurrentInputDevices = CoreInputDeviceTypes.Pen;
            InkCanvas_GeometrySketch.InkPresenter.InputDeviceTypes = CurrentInputDevices;
            InkCanvas_GeometrySketch.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
            InkCanvas_GeometrySketch.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;            
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.AutoLoadAsync();
            ScrollViewer_InkCanvas.ChangeView(null, null, 0.5f, true);
        }
        private async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await ViewModel.AutoSaveAsync();
            deferral.Complete();
        }
        private async void OnCloseRequest(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            var deferral = e.GetDeferral();
            e.Handled = true;
            if (SaveNecessity == true)
            {
                ContentDialogResult result = await CD_SaveQuery.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.FileSaveAsync(InkCanvas_GeometrySketch);
                    await ViewModel.AutoSaveAsync();
                    ViewModel.ProgressRingActive = false;
                    deferral.Complete();
                    Application.Current.Exit();
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    ViewModel.ProgressRingActive = false;
                    await ViewModel.AutoSaveAsync();
                    deferral.Complete();
                    Application.Current.Exit();
                }
                else
                {
                    deferral.Complete();
                }
            }
            else
            {
                ViewModel.ProgressRingActive = false;
                await ViewModel.AutoSaveAsync();
                deferral.Complete();
                Application.Current.Exit();
            }
        }
        private async void AppBarButton_Save_Click(object sender, RoutedEventArgs e)
        {            
            await ViewModel.FileSaveAsync(InkCanvas_GeometrySketch);
            SaveNecessity = false;
        }
        private async void AppBarButton_Open_Click(object sender, RoutedEventArgs e)
        {
            if (SaveNecessity == true && InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.GetStrokes().Count != 0)
            {
                ContentDialogResult result = await CD_SaveQuery.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.FileSaveAsync(InkCanvas_GeometrySketch);
                    await ViewModel.FileOpenAsync(InkCanvas_GeometrySketch);
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    await ViewModel.FileOpenAsync(InkCanvas_GeometrySketch);
                }
                else
                {

                }
            }
            else
            {
                await ViewModel.FileOpenAsync(InkCanvas_GeometrySketch);
            }            
            SaveNecessity = false;
        }

        //Geodreieck DeltaManipulation        
        private void Geodreieck_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {            
            double x, y, dw;
            ScaleFactor = ScrollViewer_InkCanvas.ZoomFactor;

            x = 1 / ScaleFactor * e.Delta.Translation.X;
            y = 1 / ScaleFactor * e.Delta.Translation.Y;
            dw = Math.Round(e.Delta.Rotation, 1);            

            Geodreieck_TranslateTransform.X = Geodreieck_TranslateTransform.X + x;
            Geodreieck_TranslateTransform.Y = Geodreieck_TranslateTransform.Y + y;

            _dz.X = _dz.X + x;
            _dz.Y = _dz.Y + y;

            ViewModel.Drehwinkel = ViewModel.Drehwinkel + dw;
            if (ViewModel.Drehwinkel >= 360)
            {
                ViewModel.Drehwinkel = ViewModel.Drehwinkel - 360;
            }

            //Bindung an Koordinatenachsen
            if (-0.5 <= ViewModel.Drehwinkel && ViewModel.Drehwinkel <= 0.5)
            {
                ViewModel.Drehwinkel = 0;
            }
            else if (89.5 <= ViewModel.Drehwinkel && ViewModel.Drehwinkel <= 90.5)
            {
                ViewModel.Drehwinkel = 90;
            }
            else if (179.5 <= ViewModel.Drehwinkel && ViewModel.Drehwinkel <= 180.5)
            {
                ViewModel.Drehwinkel = 180;
            }
            else if (269.5 <= ViewModel.Drehwinkel && ViewModel.Drehwinkel <= 270.5)
            {
                ViewModel.Drehwinkel = 270;
            }
            else if (359.5 <= ViewModel.Drehwinkel)
            {
                ViewModel.Drehwinkel = 0;
            }
            Slider_GeodreieckAngel.Value = Math.Round(360 - ViewModel.Drehwinkel, 0);

            Geodreieck_RotateTransform.CenterX = _dz.X;
            Geodreieck_RotateTransform.CenterY = _dz.Y;
            Geodreieck_RotateTransform.Angle = ViewModel.Drehwinkel;           
        }
        private void Geodreieck_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                Windows.Foundation.Point pt = new Windows.Foundation.Point()
                {
                    X = e.GetCurrentPoint(Grid_InkCanvas).Position.X,
                    Y = e.GetCurrentPoint(Grid_InkCanvas).Position.Y
                };

                if (GeometryHelper.PointIsInPolygon(P1, P2, P3, pt) == true && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) == false)
                {
                    ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Disabled;
                    ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Disabled;
                    ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Disabled;

                    var pointer = e.GetCurrentPoint(Grid_InkCanvas);
                    ViewModel.Drehwinkel = ViewModel.Drehwinkel - pointer.Properties.MouseWheelDelta / 120;

                    Slider_GeodreieckAngel.Value = 360 - ViewModel.Drehwinkel;

                    Geodreieck_RotateTransform.CenterX = _dz.X;
                    Geodreieck_RotateTransform.CenterY = _dz.Y;
                    Geodreieck_RotateTransform.Angle = ViewModel.Drehwinkel;
                }
                else if (GeometryHelper.PointIsInPolygon(P1, P2, P3, pt) == true && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) == true)
                {
                    ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Enabled;
                }
                else
                {
                    ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Enabled;
                }
            }
            catch
            {

            }
        }
        private void Geodreieck_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Windows.Foundation.Point p = new Windows.Foundation.Point()
            {
                X = e.Position.X,
                Y = e.Position.Y,
            };

            if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == true)
            { 
                Polygon_GeodreieckBackground.Opacity = 0.5; 
            }
        }
        private void Geodreieck_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            Polygon_GeodreieckBackground.Opacity = 0.75;
            this.Focus(FocusState.Programmatic);
        }

        private void InkCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Windows.Foundation.Point p = new Windows.Foundation.Point()
            {
                X = e.Position.X,
                Y = e.Position.Y,
            };

            if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == false)
            {
                double x = e.Delta.Translation.X;
                double y = e.Delta.Translation.Y;
                ScaleFactor = ScrollViewer_InkCanvas.ZoomFactor;
                ScaleFactor *= e.Delta.Scale;

                if (Math.Abs(x) > Math.Abs(y))
                {
                    ScrollViewer_InkCanvas.ChangeView(ScrollViewer_InkCanvas.HorizontalOffset - x, null, ScaleFactor);
                }
                else
                {
                    ScrollViewer_InkCanvas.ChangeView(null, ScrollViewer_InkCanvas.VerticalOffset - y, ScaleFactor);
                }
                ScaleFactor = ScrollViewer_InkCanvas.ZoomFactor;
            }
            else if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == true && e.IsInertial == false && VisibilityGeodreieck == Visibility.Visible)
            {
                double x, y, dw;
                ScaleFactor = ScrollViewer_InkCanvas.ZoomFactor;

                x = 1 / ScaleFactor * e.Delta.Translation.X;
                y = 1 / ScaleFactor * e.Delta.Translation.Y;
                dw = Math.Round(e.Delta.Rotation, 1);                

                Geodreieck_TranslateTransform.X = Geodreieck_TranslateTransform.X + x;
                Geodreieck_TranslateTransform.Y = Geodreieck_TranslateTransform.Y + y;

                _dz.X = _dz.X + x;
                _dz.Y = _dz.Y + y;

                ViewModel.Drehwinkel = ViewModel.Drehwinkel + dw;
                if (ViewModel.Drehwinkel >= 360)
                {
                    ViewModel.Drehwinkel = ViewModel.Drehwinkel - 360;
                }
                
                //Bindung an Koordinatenachsen
                if (-0.5 <= ViewModel.Drehwinkel && ViewModel.Drehwinkel <= 0.5)
                {
                    ViewModel.Drehwinkel = 0;
                }
                else if (89.5 <= ViewModel.Drehwinkel && ViewModel.Drehwinkel <= 90.5)
                {
                    ViewModel.Drehwinkel = 90;
                }
                else if (179.5 <= ViewModel.Drehwinkel && ViewModel.Drehwinkel <= 180.5)
                {
                    ViewModel.Drehwinkel = 180;
                }
                else if (269.5 <= ViewModel.Drehwinkel && ViewModel.Drehwinkel <= 270.5)
                {
                    ViewModel.Drehwinkel = 270;
                }
                else if (359.5 <= ViewModel.Drehwinkel)
                {
                    ViewModel.Drehwinkel = 0;
                }
                
                Slider_GeodreieckAngel.Value = Math.Round( 360 - ViewModel.Drehwinkel,0);
                if (Slider_GeodreieckAngel.Value == 360) 
                {
                    Slider_GeodreieckAngel.Value = 0;
                }

                Geodreieck_RotateTransform.CenterX = _dz.X;
                Geodreieck_RotateTransform.CenterY = _dz.Y;
                Geodreieck_RotateTransform.Angle = ViewModel.Drehwinkel;                
            }            
            else
            {
                double x = e.Delta.Translation.X;
                double y = e.Delta.Translation.Y;
                ScaleFactor = ScrollViewer_InkCanvas.ZoomFactor;
                ScaleFactor *= e.Delta.Scale;

                if (Math.Abs(x) > Math.Abs(y))
                {
                    ScrollViewer_InkCanvas.ChangeView(ScrollViewer_InkCanvas.HorizontalOffset - x, null, ScaleFactor);
                }
                else
                {
                    ScrollViewer_InkCanvas.ChangeView(null, ScrollViewer_InkCanvas.VerticalOffset - y, ScaleFactor);
                }
                ScaleFactor = ScrollViewer_InkCanvas.ZoomFactor;
            }
        }
        private void InkCanvas_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                Windows.Foundation.Point pt = new Windows.Foundation.Point()
                {
                    X = e.GetCurrentPoint(Grid_InkCanvas).Position.X,
                    Y = e.GetCurrentPoint(Grid_InkCanvas).Position.Y
                };

                if (GeometryHelper.PointIsInPolygon(P1, P2, P3, pt) == true && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) == false)
                {
                    ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Disabled;
                    ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Disabled;
                    ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Disabled;

                    var pointer = e.GetCurrentPoint(Grid_InkCanvas);
                    ViewModel.Drehwinkel = ViewModel.Drehwinkel - pointer.Properties.MouseWheelDelta / 120;

                    Slider_GeodreieckAngel.Value = 360 - ViewModel.Drehwinkel;

                    Geodreieck_RotateTransform.CenterX = _dz.X;
                    Geodreieck_RotateTransform.CenterY = _dz.Y;
                    Geodreieck_RotateTransform.Angle = ViewModel.Drehwinkel;
                }
                else if (GeometryHelper.PointIsInPolygon(P1, P2, P3, pt) == true && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) == true)
                {
                    ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Enabled;
                }
                else
                {
                    ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Enabled;
                }
            }
            catch
            {

            }
        }
        private void Slider_GeodreieckAngel_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {            
            ViewModel.Drehwinkel = 360 - e.NewValue;

            Geodreieck_RotateTransform.CenterX = _dz.X;
            Geodreieck_RotateTransform.CenterY = _dz.Y;
            Geodreieck_RotateTransform.Angle = ViewModel.Drehwinkel;
        }

        private void MousePenButton_Checked(object sender, RoutedEventArgs e)
        {
            CurrentInputDevices = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;
            InkCanvas_GeometrySketch.InkPresenter.InputDeviceTypes = CurrentInputDevices;
        }
        private void MousePenButton_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentInputDevices = CoreInputDeviceTypes.Pen;            
            InkCanvas_GeometrySketch.InkPresenter.InputDeviceTypes = CurrentInputDevices;
        }

        //Update Slider_ZirkelRadius.Value
        public TimeSpan period { get; set; } = TimeSpan.FromSeconds(0.01);
        public ThreadPoolTimer PeriodicTimer { get; set; }
        //DrawingTools Auswahl verändert
        private void DrawingTools_Checked(object sender, RoutedEventArgs e)
        {
            var drt = (InkToolbarCustomToggleButton)sender;

            if (drt == ITBCTB_Lineal)
            {
                PeriodicTimer?.Cancel();
                ITBCTB_Zirkel.IsChecked = false;
                ITBCTB_Geodreieck.IsChecked = false;
                ViewModel.SelectedDrawingTool = "Lineal";
                
            }
            else if (drt == ITBCTB_Zirkel)
            {
                ITBCTB_Lineal.IsChecked = false;
                ITBCTB_Geodreieck.IsChecked = false;
                ViewModel.SelectedDrawingTool = "Zirkel";

                PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
                {                    
                    Dispatcher.RunAsync(CoreDispatcherPriority.High,
                        () =>
                        {                            
                            Slider_ZirkelRadius.Value = Zirkel.Radius;
                        });
                }, period);
            }
            else if (drt == ITBCTB_Geodreieck)
            {
                PeriodicTimer?.Cancel();
                ITBCTB_Lineal.IsChecked = false;
                ITBCTB_Zirkel.IsChecked = false;

                ViewModel.SelectedDrawingTool = "Geodreieck";

                _dz.X = 800;
                _dz.Y = 799;
                ViewModel.Drehwinkel = 0;
                Geodreieck_TranslateTransform.Y = 0;
                Geodreieck_TranslateTransform.X = 0;
                Geodreieck_RotateTransform.Angle = 0;
                VisibilityGeodreieck = Visibility.Visible;


                coreInkIndependentInputSource.PointerHovering += coreInkIndependentInputSource_PointerHovering;
                Geodreieck.PointerMoved += Geodreieck_PointerMoved;

                coreWetStrokeUpdateSource.WetStrokeStarting += coreWetStrokeUpdateSource_StrokeStarting;
                coreWetStrokeUpdateSource.WetStrokeContinuing += coreWetStrokeUpdateSource_StrokeContinuing;
                coreWetStrokeUpdateSource.WetStrokeStopping += coreWetStrokeUpdateSource_StrokeStopping;
            }
        }       
        private void DrawingTools_Unchecked(object sender, RoutedEventArgs e)
        {
            PeriodicTimer?.Cancel();
            ViewModel.SelectedDrawingTool = "";
            VisibilityGeodreieck = Visibility.Collapsed;
                        
            coreInkIndependentInputSource.PointerHovering -= coreInkIndependentInputSource_PointerHovering; 
            Geodreieck.PointerMoved -= Geodreieck_PointerMoved;
            
            coreWetStrokeUpdateSource.WetStrokeStarting -= coreWetStrokeUpdateSource_StrokeStarting;
            coreWetStrokeUpdateSource.WetStrokeContinuing -= coreWetStrokeUpdateSource_StrokeContinuing;
            coreWetStrokeUpdateSource.WetStrokeStopping -= coreWetStrokeUpdateSource_StrokeStopping;            
        }

        //Aktiviert PointerEvents im InkCanvas für DeltaManipulation des Geodreiecks        
        private CoreInkIndependentInputSource coreInkIndependentInputSource { get; set; }        
        private async void coreInkIndependentInputSource_PointerHovering(CoreInkIndependentInputSource sender, PointerEventArgs args)
        {
            Windows.Foundation.Point p = new Windows.Foundation.Point()
            {
                X = args.CurrentPoint.Position.X,
                Y = args.CurrentPoint.Position.Y,
            };
            if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == true)
            {
                await CurrentThread.RunAsync(CoreDispatcherPriority.High, () =>
                {                    
                    Geodreieck.IsHitTestVisible = true;
                });             
            }            
        }
        private void Geodreieck_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Windows.Foundation.Point p = new Windows.Foundation.Point()
            {
                X = e.GetCurrentPoint(InkCanvas_GeometrySketch).Position.X,
                Y = e.GetCurrentPoint(InkCanvas_GeometrySketch).Position.Y,
            };

            if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == false)
            {
                Geodreieck.IsHitTestVisible = false; 
            }
        }
        
        //Snap to Geodreieck
        private bool Snap { get; set; }
        private bool IsInkSpace { get; set; }
        private double CurrentStrokeWidth { get; set; } = 1;
        CoreWetStrokeUpdateSource coreWetStrokeUpdateSource { get; set; }       
        private void SnapPoints(IList<InkPoint> newInkPoints)
        {
            Windows.Foundation.Point p = new Windows.Foundation.Point();
                      

            for (int i = 0; i < newInkPoints.Count; i++)
            {
                p.X = newInkPoints[i].Position.X;
                p.Y = newInkPoints[i].Position.Y;
                if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == false)
                {
                    Windows.Foundation.Point pt = GeometryHelper.NearestPointOnGeodreieck(P1, P2, P3, p);                  
                    Windows.Foundation.Point np = GeometryHelper.NewInkPoint(pt, p, CurrentStrokeWidth);
                    newInkPoints[i] = new InkPoint(np, newInkPoints[i].Pressure);
                }
                else
                {
                    newInkPoints.RemoveAt(i);
                }
            }
        }
        private void coreWetStrokeUpdateSource_StrokeStarting(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            InkPoint firstPoint = args.NewInkPoints.First();
            Windows.Foundation.Point p = new Windows.Foundation.Point() { X = firstPoint.Position.X, Y = firstPoint.Position.Y };

            if (GeometryHelper.MinimalDistanceToGeodreieck(P1, P2, P3, p) <= 72)
            {
                Snap = true;
                IsInkSpace = true;
                this.SnapPoints(args.NewInkPoints);                
            }
            else if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == true)
            {                
                IsInkSpace = false;
                Snap = false;               
                args.NewInkPoints.Clear();
            }            
            else
            {
                IsInkSpace = true;
                Snap = false;
            }
            SaveNecessity = true;
        }      
        private void coreWetStrokeUpdateSource_StrokeContinuing(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            if (args.NewInkPoints.Count > 0)
            {
                InkPoint firstPoint = args.NewInkPoints.First();
                Windows.Foundation.Point p = new Windows.Foundation.Point() { X = firstPoint.Position.X, Y = firstPoint.Position.Y };
                if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == true)
                {
                    IsInkSpace = false;
                    Snap = false;
                }
            } 

            if (Snap == true)
            {
                this.SnapPoints(args.NewInkPoints);                
            }
            else if (IsInkSpace == false)
            {                
                args.NewInkPoints.Clear();                
            }            
            SaveNecessity = true;            
        }
        private void coreWetStrokeUpdateSource_StrokeStopping(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            if (args.NewInkPoints.Count > 0)
            {
                InkPoint firstPoint = args.NewInkPoints.First();
                Windows.Foundation.Point p = new Windows.Foundation.Point() { X = firstPoint.Position.X, Y = firstPoint.Position.Y };
                if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == true)
                {
                    IsInkSpace = false;
                    Snap = false;
                    args.Disposition = CoreWetStrokeDisposition.Completed;
                }
            }

            if (Snap == true)
            {
                this.SnapPoints(args.NewInkPoints);
            }
            else if (IsInkSpace == false)
            {
                args.NewInkPoints.Clear();
            }
            SaveNecessity = true;       
        }
        
        //Undo Redo SaveNecessity
        private IReadOnlyList<InkStroke> CurrentInkStrokes { get; set; }
        private Stack<InkStroke> UndoStrokes { get; set; } = new Stack<InkStroke>();
        //Set Save Necessity
        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            SaveNecessity = true;
        }
        private void AppBarButton_Undo_Click(object sender, RoutedEventArgs e)
        {
            CurrentInkStrokes = InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.GetStrokes();
            if (CurrentInkStrokes.Count > 0)
            {
                CurrentInkStrokes[CurrentInkStrokes.Count - 1].Selected = true;
                UndoStrokes.Push(CurrentInkStrokes[CurrentInkStrokes.Count - 1]);
                InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.DeleteSelected();
            }
            SaveNecessity = true;
        }
        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            UndoStrokes.Push(args.Strokes.Last());
        }
        private void AppBarButton_Delete_Click(object sender, RoutedEventArgs e)
        {
            CurrentInkStrokes = InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.GetStrokes();
            foreach (InkStroke str in CurrentInkStrokes.ToList())
            {
                UndoStrokes.Push(str);
            }
            InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.Clear();
            SaveNecessity = true;
        }
        private void AppBarButton_Redo_Click(object sender, RoutedEventArgs e)
        {
            if (UndoStrokes.Count > 0)
            {
                var stroke = UndoStrokes.Pop();

                var strokeBuilder = new InkStrokeBuilder();
                strokeBuilder.SetDefaultDrawingAttributes(stroke.DrawingAttributes);
                System.Numerics.Matrix3x2 matr = stroke.PointTransform;
                IReadOnlyList<InkPoint> inkPoints = stroke.GetInkPoints();
                InkStroke stk = strokeBuilder.CreateStrokeFromInkPoints(inkPoints, matr);
                InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.AddStroke(stk);
            }
            SaveNecessity = true;
        }

        private void SelectedInkingToolChanged(object sender, RoutedEventArgs e)
        {            
            if (EraserButton.IsChecked == true)
            {
                ViewModel.DrawingToolsDetailsVisibility = Visibility.Collapsed;
            }
            else
            {
                ViewModel.DrawingToolsDetailsVisibility = Visibility.Visible;

                InkToolbarPenButton pb = (InkToolbarPenButton)sender;
                ViewModel.SelectedPen = pb;
                GridView_Colors.DataContext = null;
                GridView_Colors.DataContext = pb;

                Grid_DrawingToolDetails.Visibility = Visibility.Visible;
                pb.SelectedBrushIndex = ViewModel.SelectedPen.SelectedBrushIndex;
                PenAttributesChanged();              
            }            
        }               
        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            PenAttributesChanged();
            
        }
        private void GridView_Colors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PenAttributesChanged();            
        }       
        private void PenAttributesChanged()
        {
            PreviewInkStrokeCanvas.InkPresenter.StrokeContainer.Clear();
            ViewModel.CreatePreviewInkStroke();
            InkDrawingAttributes da = new InkDrawingAttributes();            

            SolidColorBrush scb = (SolidColorBrush)ViewModel.SelectedPen.SelectedBrush;
            Windows.UI.Color cl = new Windows.UI.Color();
            cl = scb.Color;
            
            da.Color = cl;

            if (Pencil_Button.IsChecked == true)
            {
                da = InkDrawingAttributes.CreateForPencil();                
            }
            
            da.Color = cl;
            da.Size = new Windows.Foundation.Size(ViewModel.SelectedPen.SelectedStrokeWidth, ViewModel.SelectedPen.SelectedStrokeWidth);
            da.IgnorePressure = true;
            ViewModel.UpdatePreviewInkStroke(da);
            PreviewInkStrokeCanvas.InkPresenter.StrokeContainer.AddStroke(ViewModel.PreviewInkStroke);

            CurrentStrokeWidth = Slider_StrokeWidth.Value;
        }

        //Print and Export
        PrintHelper printHelper { get; set; }
        private async void AppBarButton_Print_Click(object sender, RoutedEventArgs e)
        {
            var defaultPrintHelperOptions = new PrintHelperOptions()
            {
                Orientation = PrintOrientation.Portrait
            };            
            defaultPrintHelperOptions.AddDisplayOption(StandardPrintTaskOptions.Orientation);
                        
            printHelper = new PrintHelper(Container, defaultPrintHelperOptions);            
            
            var grid = new Grid()
            {
                Height = Grid_InkCanvas.Height,
                Width = Grid_InkCanvas.Width,
                Margin = Grid_InkCanvas.Margin,
                BorderThickness = Grid_InkCanvas.BorderThickness,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };            

            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(Grid_InkCanvas);
            
            IBuffer pixelBuffer = await rtb.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();
            
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

            var stream = new InMemoryRandomAccessStream();
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, displayInformation.RawDpiX, displayInformation.RawDpiY, pixels);

            await encoder.FlushAsync(); stream.Seek(0);
                  
            BitmapImage bimg = new BitmapImage();
            await bimg.SetSourceAsync(stream);            
            
            Image img = new Image()
            {
                Width = Grid_InkCanvas.Width,
                Height = Grid_InkCanvas.Height,
                Source = bimg
            };

            Viewbox vb = new Viewbox()
            {
                Child = grid               
            }; 

            grid.Children.Add(img);

            printHelper.AddFrameworkElementToPrint(vb);
            
            printHelper.OnPrintFailed += PrintHelper_OnPrintFailed;
            printHelper.OnPrintSucceeded += PrintHelper_OnPrintSucceeded;
                       
            await printHelper.ShowPrintUIAsync("GeometrySketch");
        }
        private async void PrintHelper_OnPrintSucceeded()
        {
            printHelper.Dispose();
            var dialog = new MessageDialog("Das Drucken war erfolgreich.");
            await dialog.ShowAsync();
        }
        private async void PrintHelper_OnPrintFailed()
        {
            printHelper.Dispose();
            var dialog = new MessageDialog("Das Drucken schlug leider fehl.");
            await dialog.ShowAsync();
        }
        private async void AppBarButton_Exportjpeg_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop,                
                DefaultFileExtension = ".jpeg",
                SuggestedFileName = "Skizze"
            };
            savePicker.FileTypeChoices.Add("jpeg", new List<string>() { ".jpeg" });

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                ViewModel.ProgressRingActive = true;
                CachedFileManager.DeferUpdates(file);
                
                var grid = new Grid()
                {
                    Height = Grid_InkCanvas.Height,
                    Width = Grid_InkCanvas.Width,
                    Margin = Grid_InkCanvas.Margin,
                    BorderThickness = Grid_InkCanvas.BorderThickness,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                var rtb = new RenderTargetBitmap();
                await rtb.RenderAsync(Grid_InkCanvas);
                
                IBuffer pixelBuffer = await rtb.GetPixelsAsync();
                byte[] pixels = pixelBuffer.ToArray();
                
                DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);                              
                
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, displayInformation.RawDpiX, displayInformation.RawDpiY, pixels);

                await encoder.FlushAsync();
                stream.Seek(0);

                Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    // File saved.
                }
                else
                {
                    // File couldn't be saved.
                }
            }
            else
            {
                // Operation cancelled.
            }
            ViewModel.ProgressRingActive = false;
        }
        private async void AppBarButton_Exportbmp_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new FileSavePicker()
            {
                SuggestedStartLocation = PickerLocationId.Desktop,
                DefaultFileExtension = ".bmp",
                SuggestedFileName = "Skizze"
            };
            savePicker.FileTypeChoices.Add("bmp", new List<string>() { ".bmp" });

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                ViewModel.ProgressRingActive = true;
                CachedFileManager.DeferUpdates(file);

                var grid = new Grid()
                {
                    Height = Grid_InkCanvas.Height,
                    Width = Grid_InkCanvas.Width,
                    Margin = Grid_InkCanvas.Margin,
                    BorderThickness = Grid_InkCanvas.BorderThickness,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                var rtb = new RenderTargetBitmap();
                await rtb.RenderAsync(Grid_InkCanvas);
                
                IBuffer pixelBuffer = await rtb.GetPixelsAsync();
                byte[] pixels = pixelBuffer.ToArray();
                
                DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite);

                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, displayInformation.RawDpiX, displayInformation.RawDpiY, pixels);

                await encoder.FlushAsync();
                stream.Seek(0);

                Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    // File saved.
                }
                else
                {
                    // File couldn't be saved.
                }
            }
            else
            {
                // Operation cancelled.
            }
            ViewModel.ProgressRingActive = false;
        }
        private async void AppBarButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            var grid = new Grid()
            {
                Height = Grid_InkCanvas.Height,
                Width = Grid_InkCanvas.Width,
                Margin = Grid_InkCanvas.Margin,
                BorderThickness = Grid_InkCanvas.BorderThickness,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(Grid_InkCanvas); 
            
            IBuffer pixelBuffer = await rtb.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();
            
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

            var stream = new InMemoryRandomAccessStream();
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied, (uint)rtb.PixelWidth, (uint)rtb.PixelHeight, displayInformation.RawDpiX, displayInformation.RawDpiY, pixels);

            await encoder.FlushAsync();
            stream.Seek(0);

            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));

            Clipboard.SetContent(dataPackage);
        }
        private async void AppBarButton_New_Click(object sender, RoutedEventArgs e)
        {
            if (SaveNecessity == true)
            {
                ContentDialogResult result = await CD_SaveQuery.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.FileSaveAsync(InkCanvas_GeometrySketch);
                    ViewModel.ProgressRingActive = false;
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    ViewModel.ProgressRingActive = false;
                    InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.Clear();
                    SaveNecessity = false;
                }
                else
                {

                }
            }
            else
            {
                InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.Clear();
                SaveNecessity = false;
                ViewModel.ProgressRingActive = false;
            }
        }

        private async void AppBarButton_Settings_Click(object sender, RoutedEventArgs e)
        {            
            await CD_Settings.ShowAsync();
        }
        private void Theme_CheckedChangedAsync(object sender, RoutedEventArgs e)
        {
            if (RadioButton_Light.IsChecked == true)
            {
                this.RequestedTheme = ElementTheme.Light;
            }
            else if (RadioButton_Dark.IsChecked == true)
            {
                this.RequestedTheme = ElementTheme.Dark;
            }
            else if (RadioButton_System.IsChecked == true)
            {
                var DefaultTheme = new Windows.UI.ViewManagement.UISettings();
                var uiTheme = DefaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
                if (uiTheme == "#FF000000")
                {
                    this.RequestedTheme = ElementTheme.Dark;
                }
                else if (uiTheme == "#FFFFFFFF")
                {
                    this.RequestedTheme = ElementTheme.Light;
                }
            }
            CD_SaveQuery.RequestedTheme = this.RequestedTheme;
        }
    }
}
