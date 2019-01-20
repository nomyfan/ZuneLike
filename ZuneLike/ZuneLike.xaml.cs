﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ZuneLike
{
    // Resources
    public partial class ZuneLike : UserControl
    {
        #region Resources
        public DoubleAnimation FadeOutAnimation { get; set; }
        public Storyboard FadeOutStoryboard { get; set; }

        public DoubleAnimation FadeInAnimation { get; set; }
        public Storyboard FadeInStoryboard { get; set; }

        private void LoadResources()
        {
            FadeOutAnimation = (DoubleAnimation)FindResource("FadeOutAnimation");
            FadeInAnimation = (DoubleAnimation)FindResource("FadeInAnimation");
            FadeOutStoryboard = new Storyboard();
            FadeOutStoryboard.Children.Add(FadeOutAnimation);
            FadeInStoryboard = new Storyboard();
            FadeInStoryboard.Children.Add(FadeInAnimation);
        }
        #endregion // Resources

        #region CTORs
        public ZuneLike() : this(80, 10, 18)
        { }

        public ZuneLike(int gridLength, int rows, int cols) : this(gridLength, rows, cols, null)
        { }

        public ZuneLike(int gridLength, int rows, int cols, IEnumerable<Uri> uris)
        {
            InitializeComponent();

            GridLength = gridLength;
            Rows = rows;
            Columns = cols;
            SyncContext = SynchronizationContext.Current;
            LoadResources();
            BackgroundColor = Colors.Black;

            SetUris(uris);

            Timer = new System.Windows.Forms.Timer
            {
                Interval = 5_000
            };
            Timer.Tick += (s, e) => Flip(Images);
            Timer.Start();

        }

        #endregion // CTORs

        #region Public methods
        public void InitializeGrid()
        {
            containerGrid.Children.Clear();
            containerGrid.RowDefinitions.Clear();
            containerGrid.ColumnDefinitions.Clear();
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
                var uri = ImageUris[rnd.Next(0, ImageUris.Count)];
                try
                {
                    var source = new BitmapImage(uri);
                    var next = new Image { Source = source };

                    Grid.SetRow(next, block.Y);
                    Grid.SetColumn(next, block.X);
                    Grid.SetRowSpan(next, block.Size);
                    Grid.SetColumnSpan(next, block.Size);
                    containerGrid.Children.Add(next);
                    Images.Add(next);
                }
                catch (ArgumentException) { }
                catch (FileNotFoundException) { }
            }

            GC.Collect();
        }

        public void SetUris(IEnumerable<Uri> uris)
        {
            if (uris != null)
            {
                ImageUris.Clear();
                foreach (var uri in uris)
                {
                    ImageUris.Add(uri);
                }
            }
        }

        #endregion // Public methods

        #region Private methods
        private IEnumerable<Part> Depart(int rows, int cols)
        {
            int halfRows = rows / 2;
            int halfCols = cols / 2;
            var parts = new Part[4];
            parts[0] = new Part { X = 0, Y = 0, Width = halfCols, Height = halfRows };
            parts[1] = new Part { X = halfCols, Y = 0, Width = halfCols, Height = halfRows };
            parts[2] = new Part { X = 0, Y = halfRows, Width = halfCols, Height = halfRows };
            parts[3] = new Part { X = halfCols, Y = halfRows, Width = halfCols, Height = halfRows };

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

        private void Flip(IReadOnlyList<FrameworkElement> elements)
        {
            if (containerGrid.Children != null && containerGrid.Children.Count > 0)
            {
                var image = containerGrid.Children[rnd.Next(0, containerGrid.Children.Count)] as Image;
                var uri = ImageUris[rnd.Next(0, ImageUris.Count)];
                var source = new BitmapImage(uri);

                Storyboard.SetTarget(FadeOutAnimation, image);
                Storyboard.SetTarget(FadeInAnimation, image);

                Task.Run(() =>
                {
                    SyncContext.Post((o) => FadeOutStoryboard.Begin(), null);
                    Task.Delay(1_100).GetAwaiter().GetResult();
                    SyncContext.Post((o) => image.Source = source, null);
                    SyncContext.Post((o) => FadeInStoryboard.Begin(), null);
                });
            }
        }
        #endregion // Private methods

        #region Properties
        private readonly SynchronizationContext SyncContext;

        private Random rnd = new Random();
        public System.Windows.Forms.Timer Timer { get; set; }

        public List<Uri> ImageUris { get; set; } = new List<Uri>();

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

        public int GridLength { get; set; }
        public int LargestSize { get; set; } = 4;

        private int rows;
        public int Rows
        {
            get => rows;
            set
            {
                if (value > 2 * LargestSize) rows = value;
            }
        }

        private int columns;
        public int Columns
        {
            get => columns;
            set
            {
                if (value > 2 * LargestSize) columns = value;
            }
        }

        private List<Block> Blocks { get; } = new List<Block>();
        public List<Image> Images { get; } = new List<Image>();

        #endregion // Properties

    }
}
