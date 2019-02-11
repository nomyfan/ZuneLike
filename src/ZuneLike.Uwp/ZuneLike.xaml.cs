using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace ZuneLike.Uwp
{
    public sealed partial class ZuneLike : UserControl
    {
        #region Resources
        private DoubleAnimation FadeOutAnimation { get; set; }
        private Storyboard FadeOutStoryboard { get; set; }

        private DoubleAnimation FadeInAnimation { get; set; }
        private Storyboard FadeInStoryboard { get; set; }

        private void LoadResources()
        {
            FadeOutAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };
            Storyboard.SetTargetProperty(FadeOutAnimation, "Opacity");
            FadeInAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(1.5))
            };
            Storyboard.SetTargetProperty(FadeInAnimation, "Opacity");

            FadeOutStoryboard = new Storyboard();
            FadeOutStoryboard.Children.Add(FadeOutAnimation);
            FadeInStoryboard = new Storyboard();
            FadeInStoryboard.Children.Add(FadeInAnimation);
        }
        #endregion // Resources

        #region CTORs
        public ZuneLike()
        {
            InitializeComponent();
            SyncContext = SynchronizationContext.Current;
            LoadResources();
            BackgroundColor = Colors.Black;

            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(Interval)
            };
            Timer.Tick += (s, e) => Flip();
            Timer.Start();
        }

        public ZuneLike(int gridLength, IEnumerable<Uri> uris) : this()
        {
            GridLength = gridLength;
            SetUris(uris);
        }

        #endregion // CTORs

        #region Public methods
        public void InitializeGrid(int rows = 10, int cols = 18)
        {
            containerGrid.Children.Clear();
            containerGrid.RowDefinitions.Clear();
            containerGrid.ColumnDefinitions.Clear();
            Rows = rows;
            Columns = cols;

            for (int i = 0; i < Rows; i++) containerGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GridLength) });
            for (int i = 0; i < Columns; i++) containerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GridLength) });

            Blocks.Clear();

            var matrix = new bool[Rows, Columns];
            var areas = Depart(Rows, Columns);

            foreach (var area in areas)
            {
                FindLargestBlock(area, matrix, out var block);
                Blocks.Add(block);
            }

            LoopRestBlocks(matrix);
        }

        public void Render()
        {
            foreach (var block in Blocks)
            {
                var uri = ImageSourceMap.Uris[rnd.Next(0, ImageSourceMap.Count)];
                try
                {
                    var source = ImageSourceMap[uri];
                    var next = new Image { Source = source };

                    Grid.SetRow(next, block.Y);
                    Grid.SetColumn(next, block.X);
                    Grid.SetRowSpan(next, block.Size);
                    Grid.SetColumnSpan(next, block.Size);
                    containerGrid.Children.Add(next);
                    next.SetValue(MarginProperty, new Thickness(1));
                }
                catch (ArgumentException) { }
                catch (FileNotFoundException) { }
            }
        }

        public void SetUris(IEnumerable<Uri> uris)
        {
            ImageSourceMap.SetUris(uris);
        }

        #endregion // Public methods

        #region Private methods
        private IEnumerable<Part> Depart(int rows, int cols)
        {
            int upRows = rows / 2;
            int downRows = rows - upRows;
            int leftCols = cols / 2;
            int rightCols = cols - leftCols;
            var parts = new Part[4];
            parts[0] = new Part { X = 0, Y = 0, Width = leftCols, Height = upRows };
            parts[1] = new Part { X = leftCols, Y = 0, Width = rightCols, Height = upRows };
            parts[2] = new Part { X = 0, Y = upRows, Width = leftCols, Height = downRows };
            parts[3] = new Part { X = leftCols, Y = upRows, Width = rightCols, Height = downRows };

            return parts;
        }

        private void FindLargestBlock(Part area, bool[,] matrix, out Block block)
        {
            while (true)
            {
                var x = rnd.Next(0, area.Width);
                var y = rnd.Next(0, area.Height);
                if (x + LargestSize >= area.Width
                    || y + LargestSize >= area.Height)
                {
                    continue;
                }

                block = new Block { X = x + area.X, Y = y + area.Y, Size = LargestSize };

                for (int r = y; r < y + LargestSize; ++r)
                {
                    for (int c = x; c < x + LargestSize; ++c)
                    {
                        matrix[r + area.Y, c + area.X] = true;
                    }
                }
                break;
            }
        }

        private void LoopRestBlocks(bool[,] matrix)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (matrix[i, j]) continue;
                    var lartestSize = LargestSizeAt(i, j, matrix);
                    var size = rnd.Next(1, lartestSize + 1);
                    Blocks.Add(new Block { X = j, Y = i, Size = size });
                    for (int r = i; r < i + size; r++)
                    {
                        for (int c = j; c < j + size; c++)
                        {
                            matrix[r, c] = true;
                        }
                    }
                }
            }
        }

        private int LargestSizeAt(int y, int x, bool[,] matrix)
        {
            int largestSize = 1;
            for (int size = LargestSize - 1; size >= 1; size--)
            {
                bool toBreak = false;
                for (int r = 0; r < size; r++)
                {
                    for (int c = 0; c < size; c++)
                    {
                        if (r + y >= Rows || c + x >= Columns || matrix[r + y, c + x])
                        {
                            toBreak = true;
                            break;
                        }
                    }
                    if (toBreak) break;
                }
                if (!toBreak)
                {
                    largestSize = size;
                    break;
                }
            }

            return largestSize;
        }

        private void Flip()
        {
            if (containerGrid.Children != null && containerGrid.Children.Count > 0)
            {
                var image = containerGrid.Children[rnd.Next(0, containerGrid.Children.Count)] as Image;
                var uri = ImageSourceMap.Uris[rnd.Next(0, ImageSourceMap.Count)];
                var source = ImageSourceMap[uri];

                FadeOutStoryboard.Stop();
                FadeInStoryboard.Stop();
                Storyboard.SetTarget(FadeOutAnimation, image);
                Storyboard.SetTarget(FadeInAnimation, image);

                Task.Run(() =>
                {
                    SyncContext.Post((o) => FadeOutStoryboard.Begin(), null);
                    Task.Delay(1_500).GetAwaiter().GetResult();
                    SyncContext.Post((o) =>
                    {
                        ImageSourceMap.MinusOneReference(image.Source);
                        image.Source = source;
                        FadeInStoryboard.Begin();
                    }, null);
                });
            }
        }
        #endregion // Private methods

        #region Properties
        private readonly SynchronizationContext SyncContext;

        private Random rnd = new Random();
        //private System.Windows.Forms.Timer Timer { get; set; }
        private DispatcherTimer Timer { get; set; }

        private ImageSourceMap ImageSourceMap { get; set; } = new ImageSourceMap();

        private Color backgroundColor;
        public Color BackgroundColor
        {
            get => backgroundColor;
            set
            {
                backgroundColor = value;
                rootGrid.Background = new SolidColorBrush(backgroundColor);
            }
        }

        public int GridLength { get; set; } = 80;

        private int largestSize = 4;
        public int LargestSize
        {
            get => largestSize;
            set => largestSize = value > 1 ? value : 4;
        }

        private int interval = 10_000;
        public int Interval
        {
            get => interval;
            set => interval = value >= 1_000 ? value : interval;
        }

        private int rows = 10;
        public int Rows
        {
            get => rows;
            set
            {
                if (value > 2 * LargestSize) rows = value;
            }
        }

        private int columns = 18;
        public int Columns
        {
            get => columns;
            set
            {
                if (value > 2 * LargestSize) columns = value;
            }
        }

        private List<Block> Blocks { get; } = new List<Block>();

        #endregion // Properties

    }
}
