# PhotoBrowser.Maui

Full screen image viewer (.NET MAUI) with pinch-to-zoom, swipe-to-dismiss, and per-photo captions.

[![NuGet](https://img.shields.io/nuget/v/PhotoBrowser.Forms.svg)](https://www.nuget.org/packages/PhotoBrowser.Forms/)

Targets **.NET 10** MAUI on **Android** and **iOS**.

* Android: wraps [SubsamplingScaleImageView](https://github.com/davemorrissey/subsampling-scale-image-view) (tile-based zoom for very large images).
* iOS: wraps [MWPhotoBrowser](https://github.com/mwaterfall/MWPhotoBrowser).

## Features

* Pinch to zoom (tile-based subsampling on Android — no OOM on huge images).
* Swipe between photos.
* Swipe-to-dismiss (iOS).
* Per-photo title / caption.
* Page indicator (`3 / 9`).
* Remote URL **and** local file path support.
* `PageChanged` and `Closed` events.
* Custom action button (iOS native share sheet).
* Black, chrome-less fullscreen modal (code-only on Android — no Android theme dependency).

## Screen-Shots

| Android sample | iOS sample |
| ------------- | ---------------
| ![sample](android.gif) | ![sample](ios.gif)

## Setup

Install the [NuGet package](https://www.nuget.org/packages/PhotoBrowser.Maui) in the MAUI project.

### MauiProgram

In `MauiProgram.cs`:

```csharp
var builder = MauiApp.CreateBuilder();
builder
    .UseMauiApp<App>()
    .ConfigurePhotoBrowser();
```

`ConfigurePhotoBrowser()` also wires [FFImageLoading](https://github.com/luberda-molinet/FFImageLoading) (used internally for Android image loading). You don't need to call `UseFFImageLoading()` separately.

## Usage

```csharp
var browser = new PhotoBrowsers.PhotoBrowser
{
    StartIndex = 0,
    Photos = new List<Photo>
    {
        new Photo { URL = "https://raw.githubusercontent.com/stfalcon-studio/FrescoImageViewer/v.0.5.0/images/posters/Vincent.jpg", Title = "Vincent" },
        new Photo { URL = "https://raw.githubusercontent.com/stfalcon-studio/FrescoImageViewer/v.0.5.0/images/posters/Jules.jpg",   Title = "Jules"   },
        new Photo { URL = "/storage/emulated/0/Pictures/local.jpg",                                                                Title = "Local"   },
    },
};

browser.PageChanged += (s, index) => Debug.WriteLine($"User is viewing photo {index}");
browser.Closed      += (s, _)     => Debug.WriteLine("Viewer dismissed");

browser.Show();
```

Close it programmatically:

```csharp
PhotoBrowsers.PhotoBrowser.Close();
```

## API

### `Photo`

| Member | Description |
|---|---|
| `URL` | `http://` / `https://` remote URL, or an absolute local file path. |
| `Title` | Caption shown beneath the image (iOS toolbar / Android bottom overlay). Optional. |

### `PhotoBrowser`

| Member | Description |
|---|---|
| `Photos` | `List<Photo>` to display. |
| `StartIndex` | Index of the first photo shown. Default `0`. |
| `Show()` | Present the viewer. |
| `Close()` | *static* — dismiss the current viewer. |
| `PageChanged` | `EventHandler<int>` — fires when the displayed photo index changes (includes the initial page). |
| `Closed` | `EventHandler` — fires once after the viewer is dismissed, regardless of whether the user dismissed it or `Close()` was called. |

## Android notes

* The Android activity uses a purely code-based fullscreen style — no Android theme or XML style is required in the host app.
* On Android 15+ (`targetSdkVersion` 35+) the activity handles the forced edge-to-edge layout and applies system-bar insets as padding so content never slides under the status or navigation bar.

## Contributions

Contributions are welcome!
