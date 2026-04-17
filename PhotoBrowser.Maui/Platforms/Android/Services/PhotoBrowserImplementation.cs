using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using AndroidX.ViewPager2.Widget;
using GPSMobile.BA;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using PhotoBrowser.Maui.Platforms.Android;
using PhotoBrowser.Maui.Platforms.Android.ImageGallery;
using Resource = Microsoft.Maui.Resource;
using Application = Android.App.Application;
using Android.OS;

namespace PhotoBrowsers.Platforms.Android
{
    public class PhotoBrowserImplementation : IPhotoBrowser
    {
        public void Show(PhotoBrowser photoBrowser)
        {
            PhotoBrowserCallbacks.Current = photoBrowser;
            Intent intent = new Intent(Application.Context, typeof(GallerySlideActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            Bundle b = new Bundle();
            b.PutStringArrayList("PhotoBrowser", photoBrowser.Photos.Select(x => x.URL).ToArray());
            b.PutStringArrayList("PhotoBrowserTitles", photoBrowser.Photos.Select(x => x.Title ?? string.Empty).ToArray());
            b.PutInt("PhotoBrowserIndex", photoBrowser.StartIndex);
            intent.PutExtras(b);
            Application.Context.StartActivity(intent);
        }
        public void Close()
        {
        }
    }
}