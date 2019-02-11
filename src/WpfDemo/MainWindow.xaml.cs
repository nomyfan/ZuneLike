using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

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

        private IEnumerable<Uri> GetUris()
        {
            var paths = Directory.GetFiles(@"C:\Users\Nomyfan\Dev\Github\ZuneLike\src\WpfDemo\bin\Debug\Covers");
            foreach (var path in paths)
            {
                yield return new Uri(path);
            }
        }

        private void Generate(object sender, RoutedEventArgs e)
        {
            zunelike.InitializeGrid();
            zunelike.Interval = 10_000;
            zunelike.SetUris(GetUris());
            zunelike.Render();
        }
    }
}
