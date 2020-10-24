using GeometrySketch.Base;
using GeometrySketch.Commons;
using GeometrySketch.DataProvider;
using GeometrySketch.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace GeometrySketch.ViewModels
{
    public class MainViewModel : Observable
    {
        public InkPage inkPage { get; private set; }
        public Settings Settings { get; private set; }

        private IInkPageDataprovider _inkPageDataProvider;
        private ISettingsDataProvider _settingsDataProvider;

        private double _currentScaleFactor;
        public double CurrentScaleFactor { get { return _currentScaleFactor; } set { _currentScaleFactor = value; OnPropertyChanged(); } }

        private string _selectedDrawingTool;
        public string SelectedDrawingTool { get { return _selectedDrawingTool; } set { _selectedDrawingTool = value; OnPropertyChanged(); } }

        public MainViewModel(IInkPageDataprovider inkPageDataProvider, ISettingsDataProvider settingsDataProvider)
        {
            Settings = new Settings();
            inkPage = new InkPage();
            _settingsDataProvider = settingsDataProvider;
            _inkPageDataProvider = inkPageDataProvider;

            CurrentScaleFactor = 1;

            //DefaultColors
            Colors_BallpointPen = new BrushCollection()
            {
            new SolidColorBrush(Windows.UI.Colors.Black),
            /*new SolidColorBrush(Windows.UI.Colors.White),
            new SolidColorBrush(Windows.UI.Colors.LightGray),
            new SolidColorBrush(Windows.UI.Colors.DarkGray),*/
            new SolidColorBrush(Windows.UI.Colors.Gray),
            //new SolidColorBrush(Windows.UI.Colors.DimGray),
            new SolidColorBrush(Windows.UI.Colors.MediumVioletRed),
            new SolidColorBrush(Windows.UI.Colors.Red),
            //new SolidColorBrush(Windows.UI.Colors.OrangeRed),
            new SolidColorBrush(Windows.UI.Colors.Orange),
            new SolidColorBrush(Windows.UI.Colors.Gold),
            //new SolidColorBrush (Windows.UI.Colors.Transparent),
            //new SolidColorBrush (Windows.UI.Colors.White),

            //new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FFE600").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#A2E61B").Color),
            //new SolidColorBrush(ColorsHelper.GetColorFromHexa("#26E600").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#008055").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#00AACC").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#004DE6").Color),
            //new SolidColorBrush(ColorsHelper.GetColorFromHexa("#3D00B8").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#6600CC").Color),
            /*new SolidColorBrush(ColorsHelper.GetColorFromHexa("#600080").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#F7D7C4").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#BB9167").Color),*/
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#8E562E").Color),
            /*new SolidColorBrush(ColorsHelper.GetColorFromHexa("#613D30").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FF80FF").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FFC680").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FFFF80").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#80FF9E").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#80D6FF").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#BCB3FF").Color)  */          
            };
            Colors_Pencil = new BrushCollection()
            {
            new SolidColorBrush(Windows.UI.Colors.Black),
            /*new SolidColorBrush(Windows.UI.Colors.White),
            new SolidColorBrush(Windows.UI.Colors.LightGray),
            new SolidColorBrush(Windows.UI.Colors.DarkGray),*/
            new SolidColorBrush(Windows.UI.Colors.Gray),
            //new SolidColorBrush(Windows.UI.Colors.DimGray),
            new SolidColorBrush(Windows.UI.Colors.MediumVioletRed),
            new SolidColorBrush(Windows.UI.Colors.Red),
            //new SolidColorBrush(Windows.UI.Colors.OrangeRed),
            new SolidColorBrush(Windows.UI.Colors.Orange),
            new SolidColorBrush(Windows.UI.Colors.Gold),
            //new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FFE600").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#A2E61B").Color),
            //new SolidColorBrush(ColorsHelper.GetColorFromHexa("#26E600").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#008055").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#00AACC").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#004DE6").Color),
            //new SolidColorBrush(ColorsHelper.GetColorFromHexa("#3D00B8").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#6600CC").Color),
            /*new SolidColorBrush(ColorsHelper.GetColorFromHexa("#600080").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#F7D7C4").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#BB9167").Color),*/
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#8E562E").Color),
            /*new SolidColorBrush(ColorsHelper.GetColorFromHexa("#613D30").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FF80FF").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FFC680").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#FFFF80").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#80FF9E").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#80D6FF").Color),
            new SolidColorBrush(ColorsHelper.GetColorFromHexa("#BCB3FF").Color)  */
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
        public void UpdateModel()
        {
            if(AlignmentGridVisibilty == Visibility.Visible)
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
            if (Settings.Theme == 1)
            {
                ThemeLight = true;
            }
            else if (Settings.Theme == 2)
            {
                ThemeDark = true;
            }
            else if (Settings.Theme == 3)
            {
                ThemeSystem = true;
            }
            else
            {
                ThemeSystem = true;
            }
        }

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

        private bool _themeLight;
        public bool ThemeLight { get { return _themeLight; } set { _themeLight = value; OnPropertyChanged(); } }

        private bool _themeDark;
        public bool ThemeDark { get { return _themeDark; } set { _themeDark = value; OnPropertyChanged(); } }

        private bool _themeSystem;
        public bool ThemeSystem { get { return _themeSystem; } set { _themeSystem = value; OnPropertyChanged(); } }

        public bool InkPageSaveNecessity { get; set; }

        private bool _progressRingActive = false;
        public bool ProgressRingActive { get { return _progressRingActive; } set { _progressRingActive = value; OnPropertyChanged(); } }

        private InkToolbarPenButton _selectedPen;
        public InkToolbarPenButton SelectedPen { get { return _selectedPen; } set { _selectedPen = value; OnPropertyChanged(); } }

        private int selectedColor_BallpointPen = 0;
        public int SelectedColor_BallpointPen { get { return selectedColor_BallpointPen; } set { selectedColor_BallpointPen = value; OnPropertyChanged(); } }
        private int selectedColor_Pencil = 0;
        public int SelectedColor_Pencil { get { return selectedColor_Pencil; } set { selectedColor_Pencil = value; OnPropertyChanged(); } }
        private int selectedColor_Highlighter = 0;
        public int SelectedColor_Highlighter { get { return selectedColor_Highlighter; } set { selectedColor_Highlighter = value; OnPropertyChanged(); } }

        private BrushCollection colors_BallpointPen;
        public BrushCollection Colors_BallpointPen { get { return colors_BallpointPen; } set { colors_BallpointPen = value; OnPropertyChanged(); } }
        private BrushCollection colors_Pencil;
        public BrushCollection Colors_Pencil { get { return colors_Pencil; } set { colors_Pencil = value; OnPropertyChanged(); } }
        private BrushCollection colors_Highlighter;
        public BrushCollection Colors_Highlighter { get { return colors_Highlighter; } set { colors_Highlighter = value; OnPropertyChanged(); } }

        private Visibility _drawingToolsDetailsVisibility;
        public Visibility DrawingToolsDetailsVisibility { get { return _drawingToolsDetailsVisibility; } set { _drawingToolsDetailsVisibility = value; OnPropertyChanged(); } }

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

        private double _drehwinkel;
        public double Drehwinkel { get { return _drehwinkel; } set { if (value > 360) { _drehwinkel = value - 360; } else if (value < 0) { _drehwinkel = value + 360; } else { _drehwinkel = value; }; OnPropertyChanged(); } }

        private bool _inputEnabled;
        public bool InputEnabled
        {
            get { return _inputEnabled; }
            set { _inputEnabled = value; OnPropertyChanged(); }
        }

        private bool _scrollViewerManipulationAllowed;
        

        public bool ScrollViewerManipulationAllowed
        {
            get { return _scrollViewerManipulationAllowed; }
            set { _scrollViewerManipulationAllowed = value; OnPropertyChanged(); }
        }

        private ObservableCollection<InkStroke> _inkStrokes = new ObservableCollection<InkStroke>();
        public ObservableCollection<InkStroke> InkStrokes { get => _inkStrokes; set { _inkStrokes = value; OnPropertyChanged(); } } 

        private Visibility _alignmentGridVisibilty;
        public Visibility AlignmentGridVisibilty { get => _alignmentGridVisibilty; set { _alignmentGridVisibilty = value; OnPropertyChanged(); } }
    }
}
