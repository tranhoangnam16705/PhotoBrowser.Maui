using MWPhotoBrowserBinding;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Platform;
using PhotoBrowsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace PhotoBrowsers.Platforms.iOS
{
    public class MyMWPhotoBrower : MWPhotoBrowserDelegate
    {
        protected PhotoBrowser _photoBrowser;

        protected List<MWPhoto> _photos = new List<MWPhoto>();

        public MyMWPhotoBrower(PhotoBrowser photoBrowser)
        {
            _photoBrowser = photoBrowser;
        }

        public void Show()
        {
            _photos = new List<MWPhoto>();

            foreach (Photo p in _photoBrowser.Photos)
            {
                MWPhoto mp;
                if (Uri.IsWellFormedUriString(p.URL, UriKind.Absolute) &&
                    (p.URL.StartsWith("http://") || p.URL.StartsWith("https://")))
                {
                    // Remote URL
                    mp = MWPhoto.FromUrl(new Foundation.NSUrl(p.URL));
                }
                else
                {
                    // Local file - ensure it exists and load as image
                    if (File.Exists(p.URL))
                    {
                        mp = MWPhoto.FromImage(UIImage.FromFile(p.URL));
                    }
                    else
                    {
                        // Handle missing file case
                        mp = MWPhoto.FromImage(UIImage.FromBundle("placeholder.png")); // or handle error
                    }
                }
                if (!string.IsNullOrWhiteSpace(p.Title))
                    mp.Caption = p.Title;
                _photos.Add(mp);
            }

            MWPhotoBrowser browser = new MWPhotoBrowser(this)
            {
                EnableGrid = false,
                ZoomPhotosToFill = true,
                DisplaySelectionButtons = false,
                DisplayActionButton = true,
                AlwaysShowControls = true,
            };


            browser.SetCurrentPhoto((nuint)_photoBrowser.StartIndex);


            var window = UIApplication.SharedApplication.KeyWindow;
            var vc = window.RootViewController;
            while (vc.PresentedViewController != null)
            {
                vc = vc.PresentedViewController;
            }

            vc.PresentViewController(new UINavigationController(browser), true, null);
        }

        public override MWPhoto GetPhoto(MWPhotoBrowser photoBrowser, nuint index) => _photos[(int)index];

        public override nuint NumberOfPhotosInPhotoBrowser(MWPhotoBrowser photoBrowser) => (nuint)_photos.Count;

        public void Close()
        {
            UIApplication.SharedApplication.KeyWindow.RootViewController.DismissViewController(true, null);
        }
    }
}
