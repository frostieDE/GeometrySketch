using GeometrySketch.Base;
using GeometrySketch.Commons;
using GeometrySketch.DataProvider;
using GeometrySketch.Model;
using GeometrySketch.UndoRedoOperations;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI.Core;

namespace GeometrySketch.ViewModels
{
    public class MainViewModel : Observable
    {
        public InkPage inkPage { get; private set; } = new InkPage();
        public Settings Settings { get; private set; } = new Settings();        

        private IInkPageDataprovider _inkPageDataProvider;
        private ISettingsDataProvider _settingsDataProvider;

        public InkCanvas CurrentInkCanvas { get; set; }
        public Rectangle Rectangle_Eraser { get; set; }
        public TranslateTransform TranslateTransform_Rectangle_Eraser { get; set; } = new TranslateTransform();

        //Undo Redo
        public UndoRedoBase UndoRedoBase { get; set; } = new UndoRedoBase();

        public void UpdateModel()
        {
            if (AlignmentGridVisibilty == Visibility.Visible)
            {
                inkPage.IsGridVisible = true;
            }
            else
            {
                inkPage.IsGridVisible = false;
            }
        }
        public void UpdateViewModel()
        {
            if (inkPage.IsGridVisible == true)
            {
                AlignmentGridVisibilty = Visibility.Visible;
            }
            else
            {
                AlignmentGridVisibilty = Visibility.Collapsed;
            }
        }
        public void UpdateModelSettings()
        {
            if (ThemeLight == true)
            {
                Settings.Theme = 1;
            }
            else if (ThemeDark == true)
            {
                Settings.Theme = 2;
            }
            else if (ThemeSystem == true)
            {
                Settings.Theme = 3;
            }
            else
            {
                Settings.Theme = 3;
            }
        }
        public void UpdateViewModelSettings()
        {
            switch (Settings.Theme)
            {
                case 1:
                    ThemeLight = true;
                    CurrentTheme = ElementTheme.Light;
                    break;
                case 2:
                    ThemeDark = true;
                    CurrentTheme = ElementTheme.Dark;
                    break;
                case 3:
                default:
                    ThemeSystem = true;
                    CurrentTheme = ElementTheme.Default;
                    break;              
            }            
        }

        public MainViewModel(IInkPageDataprovider inkPageDataProvider, ISettingsDataProvider settingsDataProvider, InkCanvas inkCanvas, Rectangle rectangle)
        {            
            _settingsDataProvider = settingsDataProvider;
            _inkPageDataProvider = inkPageDataProvider;

            CurrentInkCanvas = new InkCanvas();
            CurrentInkCanvas = inkCanvas;
            Rectangle_Eraser = new Rectangle();
            Rectangle_Eraser = rectangle;            

            //DefaultColors
            Colors_BallpointPen = new BrushCollection()
            {
            new SolidColorBrush(Windows.UI.Colors.Black),
            new SolidColorBrush(Windows.UI.Colors.Gray),
            new SolidColorBrush(Windows.UI.Colors.MediumVioletRed),
            new SolidColorBrush(Windows.UI.Colors.Red),
            new SolidColorBrush(Windows.UI.Colors.Orange),
            new SolidColorBrush(Windows.UI.Colors.Gold),
            
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#A2E61B").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#008055").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#00AACC").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#004DE6").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#6600CC").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#8E562E").Color),
            };
            Colors_Pencil = new BrushCollection()
            {
            new SolidColorBrush(Windows.UI.Colors.Black),
            new SolidColorBrush(Windows.UI.Colors.Gray),
            new SolidColorBrush(Windows.UI.Colors.MediumVioletRed),
            new SolidColorBrush(Windows.UI.Colors.Red),
            new SolidColorBrush(Windows.UI.Colors.Orange),
            new SolidColorBrush(Windows.UI.Colors.Gold),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#A2E61B").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#008055").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#00AACC").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#004DE6").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#6600CC").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#8E562E").Color),
            };
            Colors_Highlighter = new BrushCollection()
            {
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FFE600").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#26E600").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#44C8F5").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#EC008C").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FF5500").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#6600CC").Color),
            };                                  
        }

        public bool SaveNecessity { get; set; } = false;

        public async Task AutoLoadAsync()
        {
            try
            {
                Settings = await _settingsDataProvider.AutoLoadSettingsAsync();                
                UpdateViewModelSettings();
                ProgressRingActive = false;
            }
            catch
            {
                ProgressRingActive = false;
            }
        }
        public async Task AutoSaveAsync()
        {
            UpdateModelSettings();
            //UpdateModel();
            try
            {
                await _settingsDataProvider.AutoSaveSettingsAsync(Settings);
                //await _punkteschluesselDataProvider.AutoSavePunkteschluesselAsync(Punkteschluessel);
                ProgressRingActive = false;
            }
            catch
            {
                ProgressRingActive = false;
            }
        }
        public async Task AppFileOpenAsync(string path, InkCanvas inkCanvas)
        {
            StorageFile file;
            file = await StorageFile.GetFileFromPathAsync(path);

            if (file != null)
            {
                ProgressRingActive = true;
                await _inkPageDataProvider.OpenInkPageAsync(inkCanvas, inkPage, file);                
                UpdateViewModel();
                ProgressRingActive = false;               
            }
            else
            {
                //Operation abgebrochen
            }                       
        }
        public async Task FileOpenAsync(InkCanvas inkCanvas)
        {            
            try
            {
                var openPicker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.Desktop
                };
                openPicker.FileTypeFilter.Add(".gsk");
                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file != null)
                {
                    ProgressRingActive = true;
                    
                    await _inkPageDataProvider.OpenInkPageAsync(inkCanvas, inkPage, file);
                    
                    // Add to FA without metadata
                    string faToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                    UpdateViewModel();
                    ProgressRingActive = false;
                    //SaveNecessity = false;
                }
                else
                {
                    //Operation abgebrochen
                }
            }
            catch
            {
                ProgressRingActive = false;
            }
        }
        public async Task FileSaveAsync(InkCanvas inkCanvas)
        {            
            try
            {                
                UpdateModel();
                var savePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.Desktop,
                    DefaultFileExtension = ".gsk"
                };
                savePicker.FileTypeChoices.Add("Skizze", new List<string>() { ".gsk" });
                savePicker.SuggestedFileName = "Neue Skizze";

                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    ProgressRingActive = true;
                    CachedFileManager.DeferUpdates(file);

                    await _inkPageDataProvider.SaveInkPageAsync(inkCanvas, inkPage, file);
                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                    // Add to FA without metadata
                    string faToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);

                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        //gespeichert
                        ProgressRingActive = false;
                        //PunkteschluesselSaveNecessity = false;
                    }
                    else
                    {
                        ProgressRingActive = false;
                        //konnte nicht gespeichert werden
                    }
                }
                else
                {
                    //Operation abgebrochen
                }
            }
            catch
            {
                ProgressRingActive = false;
            }
        }                
        
        //Eraser
        private int selectedEraser = 0;
        public int SelectedEraser { get { return selectedEraser; } set { selectedEraser = value; OnPropertyChanged(); } }

        private int eraserWidth = 8;
        public int EraserWidth { get { return eraserWidth; } set { eraserWidth = value; OnPropertyChanged(); } }        
        
        public void EraserChanged(InkCanvas inkCanvas, Rectangle rectangle_Eraser, TranslateTransform translateTransform)
        {
            CurrentInkCanvas = inkCanvas;
            Rectangle_Eraser = rectangle_Eraser;
            TranslateTransform_Rectangle_Eraser = translateTransform;
            switch (SelectedEraser)
            {
                case 0:
                    CurrentInkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.Erasing;
                    CurrentInkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.AllowProcessing;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerPressed -= UnprocessedInput_PointerPressed;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerMoved -= UnprocessedInput_PointerMoved;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerReleased -= UnprocessedInput_PointerReleased;
                    break;
                case 1:
                    EraserWidth = 12;

                    CurrentInkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.None;
                    CurrentInkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
                    break;
                case 2:
                    EraserWidth = 25;

                    CurrentInkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.None;
                    CurrentInkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
                    break;
                case 3:
                    EraserWidth = 37;

                    CurrentInkCanvas.InkPresenter.InputProcessingConfiguration.Mode = InkInputProcessingMode.None;
                    CurrentInkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerPressed += UnprocessedInput_PointerPressed;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerMoved += UnprocessedInput_PointerMoved;
                    CurrentInkCanvas.InkPresenter.UnprocessedInput.PointerReleased += UnprocessedInput_PointerReleased;
                    break;
            }                
        }

        //EraseByPoint
        List<InkStroke> StrokesBefore { get; set; }
        List<InkStroke> StrokesAfter { get; set; }
        private void UnprocessedInput_PointerPressed(InkUnprocessedInput sender, PointerEventArgs args)
        {
            
            TranslateTransform_Rectangle_Eraser.X = (float)args.CurrentPoint.RawPosition.X - EraserWidth;
            TranslateTransform_Rectangle_Eraser.Y = (float)args.CurrentPoint.RawPosition.Y - EraserWidth;
            Rectangle_Eraser.Visibility = Visibility.Visible;
            StrokesBefore = new List<InkStroke>();
            foreach (InkStroke isk in CurrentInkCanvas.InkPresenter.StrokeContainer.GetStrokes())
            {
                StrokesBefore.Add(isk);
            }
        }
        public void ErasePoints(PointerEventArgs args)
        {
            List<InkStroke> SelectedStrokes = new List<InkStroke>();
            foreach (InkStroke insr in CurrentInkCanvas.InkPresenter.StrokeContainer.GetStrokes())
            {
                if (insr.Selected == true)
                {
                    SelectedStrokes.Add(insr);
                }
            }

            InkDrawingAttributes ida;
            List<Point> pointsOnStroke;
            List<Point> PointsA;
            List<Point> PointsB;

            for (int i = 0; i < SelectedStrokes.Count; i++)
            {
                ida = SelectedStrokes[i].DrawingAttributes;
                pointsOnStroke = EraserHelper.GetPointsOnStroke(SelectedStrokes[i]);

                PointsA = new List<Point>();
                PointsB = new List<Point>();

                bool IsA = true;

                foreach (Point pt in pointsOnStroke)
                {
                    if (EraserHelper.PointInRectangle(pt, args.CurrentPoint.RawPosition, EraserWidth) == true)
                    {
                        IsA = false;
                    }
                    else
                    {
                        if (IsA == true)
                        {
                            PointsA.Add(pt);
                        }
                        else
                        {
                            PointsB.Add(pt);
                        }
                    }
                }

                if (PointsA.Count > 0 || PointsB.Count > 0)
                {
                    var strokeBuilder = new InkStrokeBuilder();
                    strokeBuilder.SetDefaultDrawingAttributes(ida);

                    if (PointsA.Count > 0)
                    {
                        InkStroke stkA = strokeBuilder.CreateStroke(PointsA);
                        CurrentInkCanvas.InkPresenter.StrokeContainer.AddStroke(stkA);

                        if (PointsB.Count > 0)
                        {
                            InkStroke stkB = strokeBuilder.CreateStroke(PointsB);
                            CurrentInkCanvas.InkPresenter.StrokeContainer.AddStroke(stkB);
                        }
                    }
                    else
                    {
                        InkStroke stkB = strokeBuilder.CreateStroke(PointsB);
                        CurrentInkCanvas.InkPresenter.StrokeContainer.AddStroke(stkB);
                    }
                }
            }
            CurrentInkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }
        private void UnprocessedInput_PointerMoved(InkUnprocessedInput sender, PointerEventArgs args)
        {
            TranslateTransform_Rectangle_Eraser.X = (float)args.CurrentPoint.RawPosition.X - EraserWidth;
            TranslateTransform_Rectangle_Eraser.Y = (float)args.CurrentPoint.RawPosition.Y - EraserWidth;

            Point p1 = new Point()
            {
                X = args.CurrentPoint.RawPosition.X - EraserWidth,
                Y = args.CurrentPoint.RawPosition.Y - EraserWidth,
            };

            Point p2 = new Point()
            {
                X = args.CurrentPoint.RawPosition.X + EraserWidth,
                Y = args.CurrentPoint.RawPosition.Y - EraserWidth,
            };

            Point p3 = new Point()
            {
                X = args.CurrentPoint.RawPosition.X + EraserWidth,
                Y = args.CurrentPoint.RawPosition.Y + EraserWidth,
            };

            Point p4 = new Point()
            {
                X = args.CurrentPoint.RawPosition.X - EraserWidth,
                Y = args.CurrentPoint.RawPosition.Y + EraserWidth,
            };

            CurrentInkCanvas.InkPresenter.StrokeContainer.SelectWithLine(p1, p2);
            ErasePoints(args);
            CurrentInkCanvas.InkPresenter.StrokeContainer.SelectWithLine(p2, p3);
            ErasePoints(args);
            CurrentInkCanvas.InkPresenter.StrokeContainer.SelectWithLine(p3, p4);
            ErasePoints(args);
            CurrentInkCanvas.InkPresenter.StrokeContainer.SelectWithLine(p4, p1);
            ErasePoints(args);
            CurrentInkCanvas.InkPresenter.StrokeContainer.SelectWithLine(p1, p3);
            ErasePoints(args);
            CurrentInkCanvas.InkPresenter.StrokeContainer.SelectWithLine(p2, p4);
            ErasePoints(args);
        }
        private void UnprocessedInput_PointerReleased(InkUnprocessedInput sender, PointerEventArgs args)
        {
            Rectangle_Eraser.Visibility = Visibility.Collapsed;
            StrokesAfter = new List<InkStroke>();
            foreach (InkStroke isk in CurrentInkCanvas.InkPresenter.StrokeContainer.GetStrokes())
            {
                StrokesAfter.Add(isk);
            }

            EraseByPointOperation eraseByPointOperation = new EraseByPointOperation(StrokesBefore, StrokesAfter);
            UndoRedoBase.AddOperationToUndoneOperations(eraseByPointOperation);
        }

        //InkingToolsPropertiers      
        private InkToolbarPenButton _selectedPen;
        public InkToolbarPenButton SelectedPen { get { return _selectedPen; } set { _selectedPen = value; OnPropertyChanged();  } }
        
        private int selectedPenIndex = 0;
        public int SelectedPenIndex { get { return selectedPenIndex; } set { selectedPenIndex = value; OnPropertyChanged(); OnPropertyChanged(nameof(InkingToolsDetailsVisibility)); } }

        private BrushCollection colors_BallpointPen;
        public BrushCollection Colors_BallpointPen { get { return colors_BallpointPen; } set { colors_BallpointPen = value; OnPropertyChanged(); } }
        private BrushCollection colors_Pencil;
        public BrushCollection Colors_Pencil { get { return colors_Pencil; } set { colors_Pencil = value; OnPropertyChanged(); } }
        private BrushCollection colors_Highlighter;
        public BrushCollection Colors_Highlighter { get { return colors_Highlighter; } set { colors_Highlighter = value; OnPropertyChanged(); } }

        //InkingToolsDetailsProperties
        public Visibility InkingToolsDetailsVisibility { get { if (SelectedPenIndex != 3) { return Visibility.Visible; } else { return Visibility.Collapsed; } } }

        private double currentStrokeWidth = 1;
        public double CurrentStrokeWidth { get { return currentStrokeWidth; } set { currentStrokeWidth = value; OnPropertyChanged(); } }

        public void PenAttributesChanged(InkCanvas PreviewInkStrokeCanvas)
        {
            PreviewInkStrokeCanvas.InkPresenter.StrokeContainer.Clear();
            CreatePreviewInkStroke();
            InkDrawingAttributes da = new InkDrawingAttributes();

            SolidColorBrush scb = (SolidColorBrush)SelectedPen.SelectedBrush;
            Windows.UI.Color cl = new Windows.UI.Color();
            cl = scb.Color;

            da.Color = cl;

            if (SelectedPenIndex == 1)
            {
                da = InkDrawingAttributes.CreateForPencil();
            }

            da.Color = cl;
            da.Size = new Windows.Foundation.Size(SelectedPen.SelectedStrokeWidth, SelectedPen.SelectedStrokeWidth);
            da.IgnorePressure = true;
            UpdatePreviewInkStroke(da);
            PreviewInkStrokeCanvas.InkPresenter.StrokeContainer.AddStroke(PreviewInkStroke);            
        }        
        public InkStroke PreviewInkStroke { get; set; }
        public InkStrokeContainer PreviewStrokeContainer { get; set; }
        public void CreatePreviewInkStroke()
        {
            var strokePreviewInkPoints = new List<InkPoint>();
            for (var i = 0; i < Commons.Constants.PreviewStrokeCoordinates.Length; i += 2)
            {
                var newPoint = new Windows.Foundation.Point(Commons.Constants.PreviewStrokeCoordinates[i], Commons.Constants.PreviewStrokeCoordinates[i + 1] + 10);
                var inkPoint = new InkPoint(newPoint, 1f);
                strokePreviewInkPoints.Add(inkPoint);
            }

            var inkStrokeBuilder = new InkStrokeBuilder();
            var newStroke = inkStrokeBuilder.CreateStrokeFromInkPoints(strokePreviewInkPoints, Matrix3x2.Identity);
            PreviewInkStroke = newStroke;
        }
        public void UpdatePreviewInkStroke(InkDrawingAttributes ida)
        {
            InkDrawingAttributes inkDrawingAttributes = new InkDrawingAttributes();
            inkDrawingAttributes = ida;

            if (PreviewInkStroke == null)
            {
                CreatePreviewInkStroke();
            }
            PreviewInkStroke.DrawingAttributes = inkDrawingAttributes;

            InkStrokeContainer strokeContainer = PreviewStrokeContainer;
            if (strokeContainer == null)
            {
                strokeContainer = new InkStrokeContainer();
            }
            else
            {
                foreach (var stroke in strokeContainer.GetStrokes())
                {
                    stroke.Selected = true;
                }
            }

            strokeContainer.AddStroke(PreviewInkStroke.Clone());
            strokeContainer.DeleteSelected();
        }


        //DrawingToolsDetails (Geodreieck, Zirkel, Lineal)
        private string _selectedDrawingTool = "";
        public string SelectedDrawingTool { get { return _selectedDrawingTool; } set { _selectedDrawingTool = value; OnPropertyChanged(); OnPropertyChanged(nameof(DrawingToolsDetailsVisibility)); } }
                
        public Visibility DrawingToolsDetailsVisibility { get { if (SelectedDrawingTool != "") { return Visibility.Visible; } else { return Visibility.Collapsed; } } }

        //GeodreieckProperties
        private double _geodreieckDrehwinkel;
        public double GeodreieckDrehwinkel { get { return _geodreieckDrehwinkel; } set { if (value > 360) { _geodreieckDrehwinkel = value - 360; } else if (value < 0) { _geodreieckDrehwinkel = value + 360; } else { _geodreieckDrehwinkel = value; }; OnPropertyChanged(); } }
        private Visibility _geodreieckVisibilty = Visibility.Collapsed;
        public Visibility GeodreieckVisibilty { get => _geodreieckVisibilty; set { _geodreieckVisibilty = value; OnPropertyChanged(); } }

        private Point _geodreieck_Dz = new Point(800, 799);
        public Point Geodreieck_Dz { get { return _geodreieck_Dz; } set { _geodreieck_Dz = value; } }
        //Points Geodreieck
        /*
        public Point Geodreieck_P1
        {
            get
            {
                Windows.Foundation.Point p = new Windows.Foundation.Point()
                {
                    X = _geodreieck_Dz.X + Math.Cos((180 - GeodreieckDrehwinkel) / 180 * Math.PI) * 800,
                    Y = _geodreieck_Dz.Y - Math.Sin((180 - GeodreieckDrehwinkel) / 180 * Math.PI) * 800
                };
                return p;
            }
        }
        public Point Geodreíeck_P2
        {
            get
            {
                Windows.Foundation.Point p = new Windows.Foundation.Point()
                {
                    X = _geodreieck_Dz.X + Math.Cos((-GeodreieckDrehwinkel) / 180 * Math.PI) * 800,
                    Y = _geodreieck_Dz.Y - Math.Sin((-GeodreieckDrehwinkel) / 180 * Math.PI) * 800
                };
                return p;
            }
        }
        public Point Geodreíeck_P3
        {
            get
            {
                Windows.Foundation.Point p = new Windows.Foundation.Point()
                {
                    X = _geodreieck_Dz.X + Math.Cos((90 - GeodreieckDrehwinkel) / 180 * Math.PI) * 799,
                    Y = _geodreieck_Dz.Y - Math.Sin((90 - GeodreieckDrehwinkel) / 180 * Math.PI) * 799
                };
                return p;
            }
        }
        */

        //AligmentGridProperties
        private Visibility _alignmentGridVisibilty;
        public Visibility AlignmentGridVisibilty { get => _alignmentGridVisibilty; set { _alignmentGridVisibilty = value; OnPropertyChanged(); } }

        //GlobalProgressRing
        private bool _progressRingActive = false;
        public bool ProgressRingActive { get { return _progressRingActive; } set { _progressRingActive = value; OnPropertyChanged(); } }

        //SettingsViewModel
        private bool _themeLight;
        public bool ThemeLight { get { return _themeLight; } set { _themeLight = value; OnPropertyChanged(); ThemeChanged(); } }
        private bool _themeDark;
        public bool ThemeDark { get { return _themeDark; } set { _themeDark = value; OnPropertyChanged(); ThemeChanged(); } }
        private bool _themeSystem;
        public bool ThemeSystem { get { return _themeSystem; } set { _themeSystem = value; OnPropertyChanged(); ThemeChanged(); } }
        private ElementTheme _currentTheme;
        public ElementTheme CurrentTheme { get { return _currentTheme; } set { _currentTheme = value; OnPropertyChanged(); } }
        public void ThemeChanged()
        {
            if (ThemeLight == true)
            {
                CurrentTheme = ElementTheme.Light;
            }
            else if (ThemeDark == true)
            {
                CurrentTheme = ElementTheme.Dark;
            }
            else if (ThemeSystem == true)
            {
                var DefaultTheme = new Windows.UI.ViewManagement.UISettings();
                var uiTheme = DefaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString();
                if (uiTheme == "#FF000000")
                {
                    CurrentTheme = ElementTheme.Dark;
                }
                else if (uiTheme == "#FFFFFFFF")
                {
                    CurrentTheme = ElementTheme.Light;
                }
            }
        }
    }
}
