using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ZuneLike
{
    class ImageSourceMap
    {
        class ImageSourceCounter
        {
            public Uri Uri { get; set; }
            public int Count { get; set; }
            public ImageSource Source { get; set; }
        }

        private readonly List<ImageSourceCounter> sources = new List<ImageSourceCounter>();

        public IReadOnlyList<Uri> Uris => sources.Select(s => s.Uri).ToList();

        public int Count => sources.Count;

        public ImageSource this[Uri uri]
        {
            get
            {
                if (uri != null)
                {
                    var first = sources.FirstOrDefault(s => s.Uri == uri);
                    if (first != null)
                    {
                        if (first.Count == 0)
                        {
                            first.Source = new BitmapImage(uri);
                        }
                        first.Count++;
                        return first.Source;
                    }
                }
                return null;
            }
        }

        public void MinusOneReference(ImageSource source)
        {
            if (source != null)
            {
                var first = sources.FirstOrDefault(s => s.Source == source);
                if (first != null)
                {
                    first.Count--;
                    if (first.Count == 0)
                    {
                        first.Source = null;
                    }
                }
            }
        }


        public void SetUris(IEnumerable<Uri> uris)
        {
            if (uris != null)
            {
                sources.Clear();
                foreach (var uri in uris)
                {
                    sources.Add(new ImageSourceCounter { Uri = uri });
                }
            }
        }

        // CTORs
        public ImageSourceMap() { }

        public ImageSourceMap(IEnumerable<Uri> uris)
        {
            SetUris(uris);
        }
    }
}
