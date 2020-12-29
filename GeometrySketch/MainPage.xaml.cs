using GeometrySketch.Commons;
using GeometrySketch.DataProvider;
using GeometrySketch.UndoRedoOperations;
using GeometrySketch.ViewModels;
using GeometrySketch.Views;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Graphics.Printing;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Core.Preview;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace GeometrySketch
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; }

        public InkCanvas CurrentInkCanvas { get { return InkCanvas_GeometrySketch; } }

        private CoreDispatcher CurrentThread { get; }

        public bool SaveNecessity { get; set; } = false;

        public float ScaleFactor { get; set; } = 1;

        public ContentDialog CD_SaveQuery { get; set; }

        public InkPresenterRuler Lineal { get; }
        public InkPresenterProtractor Zirkel { get; }

        public MainPage()
        {
            ViewModel = new MainViewModel(new InkPageDataprovider(), new SettingsDataProvider(), InkCanvas_GeometrySketch, Rectangle_Eraser);
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
                RequestedTheme = ViewModel.CurrentTheme,
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

            coreInkIndependentInputSource = CoreInkIndependentInputSource.Create(InkCanvas_GeometrySketch.InkPresenter);
            coreWetStrokeUpdateSource = CoreWetStrokeUpdateSource.Create(InkCanvas_GeometrySketch.InkPresenter);
            InkCanvas_GeometrySketch.InkPresenter.InputDeviceTypes = CoreInputDeviceTypes.Pen | CoreInputDeviceTypes.Mouse;
            InkCanvas_GeometrySketch.InkPresenter.StrokesErased += InkPresenter_StrokesErased;
            InkCanvas_GeometrySketch.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;

            InkCanvas_GeometrySketch.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
            InkCanvas_GeometrySketch.InkPresenter.UnprocessedInput.PointerEntered += UnprocessedInput_PointerEntered;
        }


        //AppLifeCycle and DataEvents
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.AutoLoadAsync();
            ScrollViewer_InkCanvas.ChangeView(null, null, 0.5f, true);

            FirstStartDialog firstStartDialog = new FirstStartDialog(ViewModel);
            if (ViewModel.FirstStartOnBuild == true)
            {
                await firstStartDialog.ShowAsync();
            }   
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
                try
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
                catch { }
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



        //InkinkToolsChanged        
        private void InkToolbar_ActiveToolChanged(InkToolbar sender, object args)
        {
            if (BallPointPen_Button.IsChecked == true)
            {
                ViewModel.SelectedInkingToolIndex = 0;
                ViewModel.SelectedPen = BallPointPen_Button;
            }
            else if (Pencil_Button.IsChecked == true)
            {
                ViewModel.SelectedInkingToolIndex = 1;
                ViewModel.SelectedPen = Pencil_Button;
            }
            else if (Highlighter_Button.IsChecked == true)
            {
                ViewModel.SelectedInkingToolIndex = 2;
                ViewModel.SelectedPen = Highlighter_Button;
            }
            else if (Laserpointer_Button.IsChecked == true)
            {
                ViewModel.SelectedInkingToolIndex = 3;
                ViewModel.ActivateLaserpointer(InkCanvas_GeometrySketch, Ellipse_Laserpointer, TranslateTransform_Ellipse_Laserpointer);
            }
            else if (Eraser_Button.IsChecked == true)
            {
                ViewModel.SelectedInkingToolIndex = 4;
                ViewModel.ActivateEraser(InkCanvas_GeometrySketch, Rectangle_Eraser, TranslateTransform_Rectangle_Eraser);
            }

            if (IsInkingToolAutoChanged != true)
            {
                LastInkingTool = ViewModel.SelectedInkingToolIndex;
            }

            GridView_Colors.DataContext = null;
            ViewModel.PenAttributesChanged(PreviewInkStrokeCanvas);
            GridView_Colors.DataContext = ViewModel.SelectedPen;
        }
        private void GridView_Colors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.PenAttributesChanged(PreviewInkStrokeCanvas);
        }
        private void ListViewEraser_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel.ActivateEraser(InkCanvas_GeometrySketch, Rectangle_Eraser, TranslateTransform_Rectangle_Eraser);
        }

        //Register Pen Turning
        private int LastInkingTool { get; set; }
        private void SelectLastInkingTool()
        {
            switch (LastInkingTool)
            {
                case 0:
                    BallPointPen_Button.IsChecked = true;
                    break;
                case 1:
                    Pencil_Button.IsChecked = true;
                    break;
                case 2:
                    Highlighter_Button.IsChecked = true;
                    break;
                case 3:
                    Laserpointer_Button.IsChecked = true;
                    break;
                case 4:
                    Eraser_Button.IsChecked = true;
                    break;
                default:
                    BallPointPen_Button.IsChecked = true;
                    break;
            }
            IsInkingToolAutoChanged = false;
        }
        private bool IsInkingToolAutoChanged { get; set; } = false;
        private void UnprocessedInput_PointerEntered(InkUnprocessedInput sender, PointerEventArgs args)
        {
            if (args.CurrentPoint.Properties.IsInverted == true && args.CurrentPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Pen)
            {
                IsInkingToolAutoChanged = true;
                Eraser_Button.IsChecked = true;
            }
            else if (args.CurrentPoint.Properties.IsInverted == false && args.CurrentPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Pen)
            {
                IsInkingToolAutoChanged = true;
                SelectLastInkingTool();
            }
        }



        //ConstructionTools(Geodreieck, Zirkel und Lineal) selection changed
        private void ConstructionTools_Checked(object sender, RoutedEventArgs e)
        {
            var drt = (InkToolbarCustomToggleButton)sender;

            if (drt == ITBCTB_Lineal)
            {
                PeriodicTimer?.Cancel();
                ITBCTB_Zirkel.IsChecked = false;
                ITBCTB_Geodreieck.IsChecked = false;

                ViewModel.SelectedConstructionTool = "Lineal";
                ViewModel.SelectedConstructionToolsIndex = 1;
            }
            else if (drt == ITBCTB_Geodreieck)
            {
                PeriodicTimer?.Cancel();

                ITBCTB_Lineal.IsChecked = false;
                ITBCTB_Zirkel.IsChecked = false;

                ViewModel.SelectedConstructionTool = "Geodreieck";
                ViewModel.SelectedConstructionToolsIndex = 2;

                //Reset Geodreieck Position                
                ViewModel.GeodreieckDZ = new Point(800,799);
                ViewModel.GeodreieckAngle = 0;
                Geodreieck_TranslateTransform.Y = 0;
                Geodreieck_TranslateTransform.X = 0;
                Geodreieck_RotateTransform.Angle = 0;
                ViewModel.GeodreieckVisibility = Visibility.Visible;

                coreInkIndependentInputSource.PointerHovering += coreInkIndependentInputSource_PointerHovering;
                Geodreieck.PointerMoved += Geodreieck_PointerMoved;

                coreWetStrokeUpdateSource.WetStrokeStarting += coreWetStrokeUpdateSource_StrokeStarting;
                coreWetStrokeUpdateSource.WetStrokeContinuing += coreWetStrokeUpdateSource_StrokeContinuing;
                coreWetStrokeUpdateSource.WetStrokeStopping += coreWetStrokeUpdateSource_StrokeStopping;
            }
            else if (drt == ITBCTB_Zirkel)
            {
                ITBCTB_Lineal.IsChecked = false;
                ITBCTB_Geodreieck.IsChecked = false;

                ViewModel.SelectedConstructionTool = "Zirkel";
                ViewModel.SelectedConstructionToolsIndex = 3;

                try
                {
                    PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
                    {
                        Dispatcher.RunAsync(CoreDispatcherPriority.High,
                            () =>
                            {
                                Slider_ZirkelRadius.Value = Zirkel.Radius;
                            });
                    }, period);
                }
                catch
                {

                }
            }
        }
        private void ConstructionTools_Unchecked(object sender, RoutedEventArgs e)
        {
            PeriodicTimer?.Cancel();
            ViewModel.SelectedConstructionTool = "";
            ViewModel.SelectedConstructionToolsIndex = 0;

            coreInkIndependentInputSource.PointerHovering -= coreInkIndependentInputSource_PointerHovering;
            Geodreieck.PointerMoved -= Geodreieck_PointerMoved;

            coreWetStrokeUpdateSource.WetStrokeStarting -= coreWetStrokeUpdateSource_StrokeStarting;
            coreWetStrokeUpdateSource.WetStrokeContinuing -= coreWetStrokeUpdateSource_StrokeContinuing;
            coreWetStrokeUpdateSource.WetStrokeStopping -= coreWetStrokeUpdateSource_StrokeStopping;
        }



        //Activate PointerEvents in InkCanvas for the DeltaManipulation of Geodreieck      
        private CoreInkIndependentInputSource coreInkIndependentInputSource { get; set; }
        private async void coreInkIndependentInputSource_PointerHovering(CoreInkIndependentInputSource sender, PointerEventArgs args)
        {
            if (GeometryHelper.PointIsInPolygon(P1, P2, P3, args.CurrentPoint.RawPosition) == true)
            {
                await CurrentThread.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    Geodreieck.IsHitTestVisible = true;
                });
            }
            else
            {
                await CurrentThread.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Enabled;
                    ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Enabled;
                });
            }
        }
        private void Geodreieck_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (GeometryHelper.PointIsInPolygon(P1, P2, P3, e.GetCurrentPoint(InkCanvas_GeometrySketch).RawPosition) == false)
            {
                Geodreieck.IsHitTestVisible = false;

                ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Enabled;
                ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Enabled;
                ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Enabled;
            }
        }
        //Geodreieck DeltaManipulation        
        public Point P1
        {
            get
            {
                Point p = new Point()
                {
                    X = ViewModel.GeodreieckDZ.X + Math.Cos((180 - ViewModel.GeodreieckAngle) / 180 * Math.PI) * 800,
                    Y = ViewModel.GeodreieckDZ.Y - Math.Sin((180 - ViewModel.GeodreieckAngle) / 180 * Math.PI) * 800
                };
                return p;
            }
        }
        public Point P2
        {
            get
            {
                Point p = new Point()
                {
                    X = ViewModel.GeodreieckDZ.X + Math.Cos((-ViewModel.GeodreieckAngle) / 180 * Math.PI) * 800,
                    Y = ViewModel.GeodreieckDZ.Y - Math.Sin((-ViewModel.GeodreieckAngle) / 180 * Math.PI) * 800
                };
                return p;
            }
        }
        public Point P3
        {
            get
            {
                Point p = new Point()
                {
                    X = ViewModel.GeodreieckDZ.X + Math.Cos((90 - ViewModel.GeodreieckAngle) / 180 * Math.PI) * 799,
                    Y = ViewModel.GeodreieckDZ.Y - Math.Sin((90 - ViewModel.GeodreieckAngle) / 180 * Math.PI) * 799
                };
                return p;
            }
        }
        private void Geodreieck_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double x, y, dw;
            ScaleFactor = ScrollViewer_InkCanvas.ZoomFactor;

            x = 1 / ScaleFactor * e.Delta.Translation.X;
            y = 1 / ScaleFactor * e.Delta.Translation.Y;
            dw = Math.Round(e.Delta.Rotation, 1);

            Geodreieck_TranslateTransform.X = Geodreieck_TranslateTransform.X + x;
            Geodreieck_TranslateTransform.Y = Geodreieck_TranslateTransform.Y + y;
            
            Point p = new Point(ViewModel.GeodreieckDZ.X + x, ViewModel.GeodreieckDZ.Y + y);
            ViewModel.GeodreieckDZ = p;           

            ViewModel.GeodreieckAngle = ViewModel.GeodreieckAngle + dw;                        

            //Bindung an Koordinatenachsen
            if (-0.5 <= ViewModel.GeodreieckAngle && ViewModel.GeodreieckAngle <= 0.5)
            {
                ViewModel.GeodreieckAngle = 0;
            }
            else if (89.5 <= ViewModel.GeodreieckAngle && ViewModel.GeodreieckAngle <= 90.5)
            {
                ViewModel.GeodreieckAngle = 90;
            }
            else if (179.5 <= ViewModel.GeodreieckAngle && ViewModel.GeodreieckAngle <= 180.5)
            {
                ViewModel.GeodreieckAngle = 180;
            }
            else if (269.5 <= ViewModel.GeodreieckAngle && ViewModel.GeodreieckAngle <= 270.5)
            {
                ViewModel.GeodreieckAngle = 270;
            }
            else if (359.5 <= ViewModel.GeodreieckAngle)
            {
                ViewModel.GeodreieckAngle = 0;
            }      
        }
        private void Geodreieck_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            Point p = new Point()
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
        }
        private void Geodreieck_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) == false)
            {
                if (e.GetCurrentPoint(Grid_InkCanvas).Properties.MouseWheelDelta > 0)
                {
                    ViewModel.GeodreieckAngle = ViewModel.GeodreieckAngle - 1;
                }
                else
                {
                    ViewModel.GeodreieckAngle = ViewModel.GeodreieckAngle + 1;
                }                

                ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Disabled;
                ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Disabled;
                ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Disabled;
            }
            else
            {
                ScrollViewer_InkCanvas.VerticalScrollMode = ScrollMode.Enabled;
                ScrollViewer_InkCanvas.HorizontalScrollMode = ScrollMode.Enabled;
                ScrollViewer_InkCanvas.ZoomMode = ZoomMode.Enabled;
            }
        }
        
        //Enables MouseInput and Scroll-Ability by Touch        
        private void InkCanvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Point p = new Point()
            {
                X = e.Position.X,
                Y = e.Position.Y,
            };

            if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == true && e.IsInertial == false && ViewModel.GeodreieckVisibility == Visibility.Visible)
            {
                Geodreieck_ManipulationDelta(sender, e);
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

        //Snap to Geodreieck
        private bool Snap { get; set; }
        private bool IsInkSpace { get; set; }
        CoreWetStrokeUpdateSource coreWetStrokeUpdateSource { get; set; }
        private void SnapPoints(IList<InkPoint> newInkPoints)
        {
            try
            {
                Point p = new Point();

                for (int i = 0; i < newInkPoints.Count; i++)
                {
                    p.X = newInkPoints[i].Position.X;
                    p.Y = newInkPoints[i].Position.Y;
                    if (GeometryHelper.PointIsInPolygon(P1, P2, P3, p) == false)
                    {
                        Point pt = GeometryHelper.NearestPointOnGeodreieck(P1, P2, P3, p);
                        Point np = GeometryHelper.NewInkPoint(pt, p, ViewModel.CurrentStrokeWidth);
                        newInkPoints[i] = new InkPoint(np, newInkPoints[i].Pressure);
                    }
                    else
                    {
                        newInkPoints.RemoveAt(i);
                    }
                }
            }
            catch { }
        }
        private void coreWetStrokeUpdateSource_StrokeStarting(CoreWetStrokeUpdateSource sender, CoreWetStrokeUpdateEventArgs args)
        {
            InkPoint firstPoint = args.NewInkPoints.First();
            Point p = new Point() { X = firstPoint.Position.X, Y = firstPoint.Position.Y };

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
                Point p = new Point() { X = firstPoint.Position.X, Y = firstPoint.Position.Y };
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
                Point p = new Point() { X = firstPoint.Position.X, Y = firstPoint.Position.Y };
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

        //Update Slider_ZirkelRadius.Value
        public TimeSpan period { get; set; } = TimeSpan.FromSeconds(0.01);
        public ThreadPoolTimer PeriodicTimer { get; set; }



        //Undo, Redo
        private void AppBarButton_Undo_Click(object sender, RoutedEventArgs e)
        {
            try { ViewModel.UndoRedoBase.Undo(InkCanvas_GeometrySketch); } catch { }
            SaveNecessity = true;
        }
        private void AppBarButton_Redo_Click(object sender, RoutedEventArgs e)
        {
            try { ViewModel.UndoRedoBase.Redo(InkCanvas_GeometrySketch); } catch { }
            SaveNecessity = true;
        }
        private void AppBarButton_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteAllOperation dao = new DeleteAllOperation(InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.GetStrokes());
            ViewModel.UndoRedoBase.AddOperationToUndoneOperations(dao);

            InkCanvas_GeometrySketch.InkPresenter.StrokeContainer.Clear();

            SaveNecessity = true;
        }



        //SaveNecessity
        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            AddStrokeOperation addStrokeOperation = new AddStrokeOperation(args.Strokes.Last());
            ViewModel.UndoRedoBase.AddOperationToUndoneOperations(addStrokeOperation);

            SaveNecessity = true;
        }
        private void InkPresenter_StrokesErased(InkPresenter sender, InkStrokesErasedEventArgs args)
        {
            EraseStrokeOperation eraseStrokeOperation = new EraseStrokeOperation(args.Strokes.Last());
            ViewModel.UndoRedoBase.AddOperationToUndoneOperations(eraseStrokeOperation);

            SaveNecessity = true;
        }



        //Print and Export
        PrintHelper printHelper { get; set; }
        private async void AppBarButton_Print_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch { }
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
            try
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
            catch
            {

            }
        }
        private async void AppBarButton_Exportbmp_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch
            {

            }
        }
        private async void AppBarButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
            catch
            {

            }
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



        //Settings Dialog
        SettingsDialog settingsDialog { get; set; }
        private async void AppBarButton_Settings_Click(object sender, RoutedEventArgs e)
        {
            settingsDialog = new Views.SettingsDialog(ViewModel);
            await settingsDialog.ShowAsync();
        }
    }
}
