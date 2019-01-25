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
            public int Count { get; set; }
            public ImageSource Source { get; set; }
        }

        readonly Dictionary<Uri, ImageSourceCounter> mapper = new Dictionary<Uri, ImageSourceCounter>();

        public IReadOnlyList<Uri> Uris => mapper.Keys.ToList();

        public int Count => mapper.Keys.Count;

        public ImageSource this[Uri uri]
        {
            get
            {
                try
                {
                    if (mapper.TryGetValue(uri, out var counter))
                    {
                        if (counter.Count == 0)
                        {
                            counter.Source = new BitmapImage(uri);
                        }
                        counter.Count++;
                        return counter.Source;
                    }
                    return null;
                }
                catch (Exception) { return null; }
            }
        }

        public void MinusOneReference(ImageSource source)
        {
            if (source != null)
            {
                var first = mapper.Values.FirstOrDefault(v => v.Source == source);
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
                foreach (var uri in uris)
                {
                    mapper.Add(uri, new ImageSourceCounter());
                }
            }
        }

        public void ClearUris()
        {
            mapper.Clear();
        }

        // CTORs
        public ImageSourceMap() { }

        public ImageSourceMap(IEnumerable<Uri> uris)
        {
            SetUris(uris);
        }
    }
}
