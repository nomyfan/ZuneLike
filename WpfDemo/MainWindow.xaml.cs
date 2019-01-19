using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Generate(null, null);
        }

        private IEnumerable<FrameworkElement> GetUIElements()
        {
            var paths = Directory.GetFiles(@"C:\Users\Nomyfan\Desktop\ZuneLike\WpfDemo\bin\Debug\Covers");
            foreach (var path in paths)
            {
                var bitmap = new BitmapImage(new Uri(path));
                yield return new Image { Source = bitmap };
            }
        }

        private IEnumerable<FrameworkElement> GetUIElements2()
        {
            var rnd = new Random();
            for (int i = 0; i < 100; i++)
            {
                var r = (byte)rnd.Next(0, 255);
                var g = (byte)rnd.Next(0, 255);
                var b = (byte)rnd.Next(0, 255);
                var color = Color.FromRgb(r, g, b);

                yield return new Rectangle { Fill = new SolidColorBrush(color) };
            }
        }

        private void Generate(object sender, RoutedEventArgs e)
        {
            zunelike.InitializeGrid();
            zunelike.SetElements(GetUIElements());
            zunelike.Render();
        }
    }
}
