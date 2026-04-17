using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.ViewPager.Widget;
using PhotoBrowser.Maui.Platforms.Android.ImageGallery;
using System;
using System.Collections.Generic;
using System.Linq;
using Resource = Microsoft.Maui.Resource;
using Color = Android.Graphics.Color;

namespace GPSMobile.BA
{
    [Activity(Label = "")]
    public class GallerySlideActivity : Activity
    {
        private GallerySlidePageAdapter _adapter;
        private ViewPager _viewPager;
        private Android.Views.View _rootView;
        private bool _isClosing;

        protected Android.Widget.ImageButton btnAction;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);
            SuppressOpenTransition();

            ApplyWindowStyle();

            SetContentView(Resource.Layout.gallery_slide);
            Intent myIntent = this.Intent;
            var startindex = myIntent.GetIntExtra("PhotoBrowserIndex", 0);
            var items = myIntent.GetStringArrayListExtra("PhotoBrowser");
            if (items != null && items.Count > 0)
            {
                InitComponents(items.ToList(), startindex);
            }

            _rootView = FindViewById<Android.Views.View>(Resource.Id.lauoutpager);
            if (_rootView != null)
            {
                ViewCompat.SetOnApplyWindowInsetsListener(_rootView, new SystemBarsInsetApplier());
                _rootView.Alpha = 0f;
                _rootView.Animate().Alpha(1f).SetDuration(220).Start();
            }
        }

        private sealed class SystemBarsInsetApplier : Java.Lang.Object, IOnApplyWindowInsetsListener
        {
            public WindowInsetsCompat OnApplyWindowInsets(Android.Views.View v, WindowInsetsCompat insets)
            {
                var bars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars());
                v.SetPadding(bars.Left, bars.Top, bars.Right, bars.Bottom);
                return insets;
            }
        }

        private void ApplyWindowStyle()
        {
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            Window.SetBackgroundDrawable(new ColorDrawable(Color.Black));

            // Force edge-to-edge on every API level; the root view pads itself via the
            // insets listener in OnCreate. API 35+ is edge-to-edge by default anyway.
            WindowCompat.SetDecorFitsSystemWindows(Window, false);

            // Transparent bars so the black window background shows through.
            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
#pragma warning disable CA1422
                Window.SetStatusBarColor(Color.Transparent);
                Window.SetNavigationBarColor(Color.Transparent);
#pragma warning restore CA1422
            }

            var insets = WindowCompat.GetInsetsController(Window, Window.DecorView);
            if (insets != null)
            {
                insets.AppearanceLightStatusBars = false;
                insets.AppearanceLightNavigationBars = false;
            }
        }

        public override void Finish()
        {
            if (_isClosing || _rootView == null)
            {
                base.Finish();
                SuppressCloseTransition();
                return;
            }

            _isClosing = true;
            _rootView.Animate().Alpha(0f).SetDuration(180).Start();
            _rootView.PostDelayed(() =>
            {
                base.Finish();
                SuppressCloseTransition();
            }, 180);
        }

        private void SuppressOpenTransition()
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(34))
            {
                OverrideActivityTransition(Android.App.OverrideTransition.Open, 0, 0);
            }
            else
            {
#pragma warning disable CA1422
                OverridePendingTransition(0, 0);
#pragma warning restore CA1422
            }
        }

        private void SuppressCloseTransition()
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(34))
            {
                OverrideActivityTransition(Android.App.OverrideTransition.Close, 0, 0);
            }
            else
            {
#pragma warning disable CA1422
                OverridePendingTransition(0, 0);
#pragma warning restore CA1422
            }
        }

        protected void InitComponents(List<string> urls, int startindex = 0)
        {
            _adapter = new GallerySlidePageAdapter(this, urls);
            _viewPager = FindViewById<ViewPager>(Resource.Id.pager);
            _viewPager.Adapter = _adapter;
            _viewPager.SetCurrentItem(startindex, false);
            _viewPager.OffscreenPageLimit = 10;
            _viewPager.PageSelected += OnPageSelected;
            _viewPager.PageScrollStateChanged += ViewPager_PageScrollStateChanged;

            btnAction = FindViewById<Android.Widget.ImageButton>(Resource.Id.btnclose);
            btnAction.Click += (o, e) =>
            {
                Finish();
            };
        }

        private void ViewPager_PageScrollStateChanged(object sender, ViewPager.PageScrollStateChangedEventArgs e)
        {
        }

        private void OnPageSelected(object s, ViewPager.PageSelectedEventArgs e)
        {
        }
    }
}
