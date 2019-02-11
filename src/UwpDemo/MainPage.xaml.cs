using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZuneLike;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UwpDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Generate();
        }

        private async Task<List<Uri>> GetUris()
        {
            var list = new List<Uri>();
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
            Debug.WriteLine(folder.Path);
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                list.Add(new Uri(file.Path));
            }

            return list;
        }

        private void Generate()
        {
            zunelike.InitializeGrid();
            zunelike.Interval = 10_000;
            zunelike.SetUris(AsyncHelper.RunSync(async () => await GetUris()));
            zunelike.Render();
        }
    }
}
