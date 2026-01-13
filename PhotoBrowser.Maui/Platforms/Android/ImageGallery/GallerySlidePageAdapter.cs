using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using Com.Davemorrissey.Labs.Subscaleview;
using FFImageLoading.Helpers;
using FFImageLoading;
using System.Net;
using JObject = Java.Lang.Object;

namespace PhotoBrowser.Maui.Platforms.Android.ImageGallery
{
    public class GallerySlidePageAdapter : InfiniteViewPagerAdapter
    {
        private const int MaxZoom = 10;

        private readonly IList<string> _pages;
        private readonly LayoutInflater _inflater;
        public GallerySlidePageAdapter(Context context, IList<string> images)
        {
            _pages = images;
            _inflater = LayoutInflater.From(context);
        }

        public override int RealCount => _pages.Count;

        public override bool IsViewFromObject(global::Android.Views.View view, JObject obj)
        {
            return view == obj;
        }

        public override JObject InstantiateItem(ViewGroup container, int position)
        {
            var listPosition = position;

            global::Android.Views.View convertView;
            {
                convertView = _inflater.Inflate(Resource.Layout.gallery_image, null);
                BindView(convertView, listPosition);
                container.AddView(convertView);
            }

            return convertView;
        }

        public override void DestroyItem(ViewGroup container, int position, JObject view)
        {
            container.RemoveView((global::Android.Views.View)view);
        }

        private void BindView(global::Android.Views.View view, int listPosition)
        {
            var galleryImageView = view.FindViewById<Com.Davemorrissey.Labs.Subscaleview.SubsamplingScaleImageView>(Resource.Id.galleryImageView);
            galleryImageView.SetMinimumDpi(MaxZoom);
            var imageUrl = _pages[listPosition];
            IImageService _imageService = ServiceHelper.GetService<IImageService>();
            var activityIndicatorView = view.FindViewById<global::Android.Widget.ProgressBar>(Resource.Id.activityIndicatorView);
            // Check if it's a local file path or remote URL
            if (IsLocalFilePath(imageUrl))
            {
                // Handle local file
                if (File.Exists(imageUrl))
                {
                    ImageService.Instance
                        .LoadFile(imageUrl)
                        .WithCache(FFImageLoading.Cache.CacheType.Disk)
                        .IntoAsync(new SImageViewTarget(galleryImageView), _imageService);
                }
                else
                {
                    // Handle missing file - you might want to show a placeholder or error image
                    // ImageService.Instance.LoadCompiledResource("placeholder").Into(galleryImageView);
                    activityIndicatorView.Visibility = ViewStates.Gone;
                }
            }
            else
            {
                // Handle remote URL
                ImageService.Instance
                    .LoadUrl(imageUrl)
                    .WithCache(FFImageLoading.Cache.CacheType.Disk)
                    .IntoAsync(new SImageViewTarget(galleryImageView), _imageService);
            }
            if (!galleryImageView.IsImageLoaded)
            {
                activityIndicatorView.Visibility = ViewStates.Visible;
            }
            galleryImageView.ImageLoaded += (s, e) =>
            {
                if (galleryImageView.IsImageLoaded)
                {
                    activityIndicatorView.Visibility = ViewStates.Gone;
                }
            };
            galleryImageView.Click += GalleryImageView_Click;
        }

        private bool IsLocalFilePath(string path)
        {
            // Check if it's a local file path (not a URL)
            return !string.IsNullOrEmpty(path) &&
                   !path.StartsWith("http://") &&
                   !path.StartsWith("https://") &&
                   (path.StartsWith("/") || System.IO.Path.IsPathRooted(path));
        }

        private void GalleryImageView_Click(object sender, EventArgs e)
        {

        }
    }
}
