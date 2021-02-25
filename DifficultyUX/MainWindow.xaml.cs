using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch.Difficulty;
using LiveCharts;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using LiveCharts.Definitions.Series;
using System.ComponentModel;
using System.Windows.Media.Imaging;


namespace DifficultyUX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string filePath = string.Empty;
        public SeriesCollection SeriesCollection { get; set; }

        private List<Ellipse> Fruits;
        private List<string> selectedMods = new List<string>();

        public Func<double, string> FormatterY { get; set; }
        public Func<double, string> FormatterX { get; set; }

        private double _xPointer;
        private double _yPointer;

        private bool isMapSet = false;
        private bool hideDiffData = false;
        private bool loadingBeatmap;

        public double XPointer
        {
            get { return _xPointer; }
            set
            {
                _xPointer = value;
                OnPropertyChanged("XPointer");
            }
        }

        public double YPointer
        {
            get { return _yPointer; }
            set
            {
                _yPointer = value;
                OnPropertyChanged("YPointer");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Chart.Visibility = Visibility.Collapsed;
            ScrollPlayfield.Visibility = Visibility.Collapsed;
            playFieldSlider.Visibility = Visibility.Collapsed;
            Tip.Visibility = Visibility.Collapsed;
            UpdateLayout();
            AllocConsole();
            XPointer = 0;
            YPointer = 0;
            FormatterY = x => x.ToString("N02");
            FormatterX = x => x.ToString("N00");   

            SeriesCollection = new SeriesCollection {
                new GLineSeries
                {
                    Title = "Stable strains",
                    Values = new GearedValues<double>(),
                    PointGeometry = null,
                    StrokeThickness = 0.5
                },
                new GLineSeries
                {
                    Title = "New strains",
                    Values = new GearedValues<double>(),
                    PointGeometry = null,
                    StrokeThickness = 0.5
                }
            };

            DataContext = this;
        }

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Beatmap files |*.osu";
            if (openFileDialog.ShowDialog() == true)
            {
                filePath = openFileDialog.FileName;
                mapName.Text = openFileDialog.FileName;
                isMapSet = true;
                loadingBeatmap = true;
                Init();
            }
        }

        private void Init()
        {
            CatchDifficulty difficulty = new CatchDifficulty();
            difficulty.Execute(filePath, selectedMods.ToArray());
            SRbox.Text = (difficulty.toString());
            ProcessorWorkingBeatmap beatmap = difficulty.getProcessedBeatmap();
            RulesetInfo r = beatmap.BeatmapInfo.Ruleset;
            CatchDifficultyAttributes attributes = difficulty.getCatchDifficultyAttributes() as CatchDifficultyAttributes;
            SRbox.Text += "New Star Rating: " + attributes.NewStarRating.ToString("N2");

            Render(beatmap.GetPlayableBeatmap(r, difficulty.getMods(new CatchRuleset())), attributes);

            if (loadingBeatmap)
            {
                var oldStrains = attributes.DifficultyFactor;
                var newStrains = attributes.NewDiff;
                SeriesCollection[0].Values = oldStrains.AsGearedValues();
                SeriesCollection[1].Values = newStrains.AsGearedValues();
            }

            openBeatmapLabel.Visibility = Visibility.Hidden;
            Chart.Visibility = Visibility.Visible;
            ScrollPlayfield.Visibility = Visibility.Visible;
            playFieldSlider.Visibility = Visibility.Visible;
            Tip.Visibility = Visibility.Visible;
        }

        private void Render(IBeatmap beatmap, CatchDifficultyAttributes attributes)
        {
            Fruits = new List<Ellipse>();
            playField.Children.Clear();

            double CircleSize = beatmap.BeatmapInfo.BaseDifficulty.CircleSize;
            if (selectedMods.Contains("hr"))
            {
                CircleSize = Math.Min(10, 1.3 * CircleSize);
            }
            if (selectedMods.Contains("ez"))
            {
                CircleSize = Math.Max(0.5, CircleSize / 2);
            }
            var Scale = 3.4 * (1.0f - 0.7f * (CircleSize - 5) / 5);

            var selectedObjects = beatmap.HitObjects.SelectMany(obj => obj switch {
                JuiceStream or BananaShower => obj.NestedHitObjects,
                _ => new[] { obj }
            })
            .Cast<CatchHitObject>()
            .OrderBy(x => x.StartTime)
            .ToList();

            HitObject lastObj = selectedObjects.Last();

            playField.Height = lastObj.StartTime;
            playFieldSlider.Maximum = playField.Height;
            playFieldSlider.Value = playField.Height;
            playFieldSlider.TickFrequency = playField.Height / 10;
            playFieldSlider.IsDirectionReversed = true;

            Rectangle area = new Rectangle();
            area.Width = 512 + 20 * Scale;
            area.Height = playField.Height;
            area.Stroke = Brushes.LightGray;
            area.StrokeThickness = 6;
            Canvas.SetLeft(area, 10 * Scale);
            playField.Children.Add(area);

            CreateFruits(selectedObjects, Scale, attributes);

        }

        private void CreateFruits(List<CatchHitObject> selectedObjects, double Scale, CatchDifficultyAttributes attributes)
        {

            BitmapImage bananaImg = new BitmapImage(new Uri("pack://application:,,,/img/banana.png", UriKind.Absolute));

            var firstObj = selectedObjects.First();
            foreach (var obj in selectedObjects)
            {
                if(!(obj is Banana))
                {
                    Ellipse fruit = new Ellipse();

                    var fruitSize = Scale * obj switch
                    {
                        TinyDroplet => 5,
                        Droplet => 13,
                        Fruit => 20,
                        _ => 0
                    };

                    fruit.Width = fruitSize;
                    fruit.Height = fruitSize;
                    fruit.Stroke = (obj.HyperDash) ? Brushes.Red : Brushes.Black;
                    fruit.StrokeThickness = 6;
                    Canvas.SetBottom(fruit, (double)obj.StartTime - firstObj.StartTime - fruitSize / 2 + 100);
                    Canvas.SetLeft(fruit, (double)obj.X + Scale * 20 - fruitSize / 2);

                    playField.Children.Add(fruit);
                
                    if(!(obj is TinyDroplet) && !(obj is BananaShower))
                    {
                        Fruits.Add(fruit);                    }
                }
                else
                { 
                    Image banana = new Image();
                    banana.Source = bananaImg;
                    var fruitSize = Scale * 13;
                    banana.Width = fruitSize;
                    banana.Height = fruitSize;
                    Canvas.SetBottom(banana, (double)obj.StartTime - firstObj.StartTime - fruitSize / 2 + 100);
                    Canvas.SetLeft(banana, (double)obj.X + Scale * 20 - fruitSize / 2);
                    playField.Children.Add(banana);
                }
            }

            if (!hideDiffData)
            {
                var strains = attributes.DifficultyFactor;
                var rawStrains = attributes.RawDiff;
                var newStrains = attributes.NewDiff;

                for (int i = 0; i < strains.Count; i++)
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = "#" + (i + 1) + "\nOld:" + Math.Round(strains[i], 3) + "\nNew: " + Math.Round(newStrains[i], 3);
                    textBlock.FontSize = 14;
                    Canvas.SetLeft(textBlock, Canvas.GetLeft(Fruits[i + 1]) + 22 * Scale);
                    Canvas.SetBottom(textBlock, Canvas.GetBottom(Fruits[i + 1]) + 5 * Scale);
                    playField.Children.Add(textBlock);
                }
            }
        }        

        public string GetFilePath()
        {
            return filePath;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ScrollPlayfield.ScrollToVerticalOffset(e.NewValue);
           /* int intOffset = Convert.ToInt32(playField.Height) - Convert.ToInt32(e.NewValue);
            int fruitNumber = Fruits.FindIndex((fruit) => {
                double coord = Canvas.GetBottom(fruit);
                return coord > intOffset - 150 && coord <= intOffset + 150;
            });

            if (fruitNumber == -1)
                return;
            var chart = Chart;
            var series = chart.Series[1];
            var point = ClosestPointTo(series, fruitNumber, AxisOrientation.X);

            if (point == null)
                return;

            this.XPointer = point.X;
            this.YPointer = point.Y;*/
        }

        private void ScrollPlayfield_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            playFieldSlider.Value = e.VerticalOffset;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
	        var chart = (LiveCharts.Wpf.CartesianChart) sender;
            var mouseCoordinate = e.GetPosition(chart);
            var p = chart.ConvertToChartValues(mouseCoordinate);
            var series = chart.Series[1];
            var point = ClosestPointTo(series, p.X, AxisOrientation.X);

            if (point == null)
                return;

            this.XPointer = point.X;
            this.YPointer = point.Y;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            var chart = (LiveCharts.Wpf.CartesianChart)sender;
            var mouseCoordinate = e.GetPosition(chart);
            var p = chart.ConvertToChartValues(mouseCoordinate);

            var index = Convert.ToInt32(Math.Round(p.X));
            if (index < 0) index = 1;
            var pointedFruit = Fruits[index];
            var location = Canvas.GetBottom(pointedFruit);

            playFieldSlider.Value = playFieldSlider.Maximum - location - ScrollPlayfield.ActualHeight / 2;
        }

        #region INotifyPropertyChanged implementation
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public ChartPoint ClosestPointTo(ISeriesView series, double value, AxisOrientation orientation)
        {
            ChartPoint t = null;
            var delta = double.PositiveInfinity;
            foreach (var point in series.Values.GetPoints(series))
            {
                var i = orientation == AxisOrientation.X ? point.X : point.Y;
                var di = Math.Abs(i - value);
                if (di < delta)
                {
                    t = point;
                    delta = di;
                }
            }
            return t;
        }

        private void cbHR_Checked(object sender, RoutedEventArgs e)
        {
            selectedMods.Add("hr");
            if (isMapSet)
            {
                var pos = playFieldSlider.Value;
                loadingBeatmap = true;
                Init();
                playFieldSlider.Value = pos;
            }
        }

        private void cbHR_Unchecked(object sender, RoutedEventArgs e)
        {
            selectedMods.Remove("hr");
            if (isMapSet)
            {
                var pos = playFieldSlider.Value;
                loadingBeatmap = true;
                Init();
                playFieldSlider.Value = pos;
            }
        }

        private void cbDT_Checked(object sender, RoutedEventArgs e)
        {
            selectedMods.Add("dt");
            if (isMapSet)
            {
                var pos = playFieldSlider.Value;
                loadingBeatmap = true;
                Init();
                playFieldSlider.Value = pos;
            }
        }

        private void cbDT_Unchecked(object sender, RoutedEventArgs e)
        {
            selectedMods.Remove("dt");
            if (isMapSet)
            {
                var pos = playFieldSlider.Value;
                loadingBeatmap = true;
                Init();
                playFieldSlider.Value = pos;
            }
        }

        private void cbEZ_Checked(object sender, RoutedEventArgs e)
        {
            selectedMods.Add("ez");
            if (isMapSet)
            {
                var pos = playFieldSlider.Value;
                loadingBeatmap = true;
                Init();
                playFieldSlider.Value = pos;
            }
        }

        private void cbEZ_Unchecked(object sender, RoutedEventArgs e)
        {
            selectedMods.Remove("ez");
            if (isMapSet)
            {
                var pos = playFieldSlider.Value;
                loadingBeatmap = true;
                Init();
                playFieldSlider.Value = pos;
            }
        }

        private void diffData_Checked(object sender, RoutedEventArgs e)
        {
            var pos = playFieldSlider.Value;
            hideDiffData = true;
            loadingBeatmap = false;
            Init();
            playFieldSlider.Value = pos;
        }

        private void diffData_Unchecked(object sender, RoutedEventArgs e)
        {
            var pos = playFieldSlider.Value;
            hideDiffData = false;
            loadingBeatmap = false;
            Init();
            playFieldSlider.Value = pos;
        }
    }
}
