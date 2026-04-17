# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & run

This is a .NET 10 MAUI solution targeting `net10.0`, `net10.0-android`, and `net10.0-ios`. There is no test project.

```bash
# Restore + build the whole solution (all TFMs)
dotnet build PhotoBrowser.Maui.sln

# Build a single project for one TFM (faster iteration)
dotnet build PhotoBrowser.Maui/PhotoBrowser.Maui.csproj -f net10.0-android
dotnet build PhotoBrowser.Maui/PhotoBrowser.Maui.csproj -f net10.0-ios

# Pack a NuGet (GeneratePackageOnBuild is already on; artifacts land in bin/<Config>/)
dotnet pack PhotoBrowser.Maui/PhotoBrowser.Maui.csproj -c Release

# Run the sample on Android
dotnet build PhotoBrowser.Sample/PhotoBrowser.Sample.csproj -t:Run -f net10.0-android
```

iOS builds only succeed on macOS (or with a Mac build host paired). The Windows dev machine can build the Android TFM and the netstandard surface but not `-ios`.

## Solution layout & architectural seams

Five projects, three roles:

| Project | Role |
|---|---|
| `PhotoBrowser.Maui` | Public MAUI library, multi-targeted. Consumers reference this NuGet. |
| `PhotoBrowser.Sample` | Demo app for manual testing (Android + iOS). |
| `SubsamplingScaleImageView` | **Android Java binding.** Wraps `subsampling-scale-image-view-3.10.0.aar` and is published as the standalone NuGet `SubsamplingScaleImageView.Android`. |
| `MWPhotoBrowserBinding` | **iOS ObjC binding.** Wraps `MWPhotoBrowser.a` + `SDWebImage.a` + `MBProgressHUD.a` + `DACircularProgress.a`. Published as `MWPhotoBrowser.iOS`. |
| `IDMPhotoBrowserBindings` | **Alternate iOS binding** (IDMPhotoBrowser). Not referenced by `PhotoBrowser.Maui` — it's an available swap-in if MWPhotoBrowser is ever replaced. |

`PhotoBrowser.Maui` consumes the native bindings via `PackageReference` (not `ProjectReference`), so after changing either binding project you must `dotnet pack` it and either publish or point `PhotoBrowser.Maui.csproj` at a local feed before changes flow through.

## How the platform dispatch works

The core of the library (in `PhotoBrowser.Maui/`) is this three-file pattern:

- `Photo.cs` — declares `Photo`, `PhotoBrowser` (the public API with `Show()`/`Close()`), and the `IPhotoBrowser` seam. `PhotoBrowser.Show()` resolves `IPhotoBrowser` from the DI container and forwards.
- `Extensions.cs` — `ConfigurePhotoBrowser()` is the entry point consumers call on `MauiAppBuilder`. It registers the correct `IPhotoBrowser` implementation per platform using `#if ANDROID` / `#if IOS` and also wires FFImageLoading.
- `ServiceHelpers.cs` — an `IMauiInitializeService` that captures the built `IServiceProvider` into a static so `PhotoBrowser.Show()` can resolve `IPhotoBrowser` from anywhere without the caller holding the container.

The `#if` symbols `ANDROID` / `IOS` come from the `DefineConstants` in `PhotoBrowser.Maui.csproj` (set to `MONOANDROID` and `IOS` respectively), **and** the platform files under `Platforms\Android\*.cs` / `Platforms\iOS\*.cs` are excluded from all other TFMs via the `<Compile Remove>` conditions in the csproj. When adding new platform code, place it under the right `Platforms\<OS>\` folder so that TFM filtering works.

### Android implementation path
`PhotoBrowserImplementation` (Android) serializes photo URLs into an `Intent` bundle and launches `GallerySlideActivity` (under `Platforms/Android/ImageGallery/`). The activity hosts an Android `ViewPager` whose adapter (`GallerySlidePageAdapter`) inflates `gallery_image.xml`, wires `SubsamplingScaleImageView` for pinch-zoom, and routes image loading through FFImageLoading (`LoadUrl` for http/https, `LoadFile` for local paths detected via `IsLocalFilePath`). The `InfiniteViewPagerAdapter` / `InfinitePageChangeListener` are scaffolding for pseudo-infinite paging but the infinite-scroll branch in `GallerySlideActivity.ViewPager_PageScrollStateChanged` is currently commented out.

### iOS implementation path
`PhotoBrowserImplementation` (iOS) creates a `MyMWPhotoBrower` (subclass of `MWPhotoBrowserDelegate` from the binding) which converts each `Photo` into an `MWPhoto` — `MWPhoto.FromUrl` for remote URLs, `MWPhoto.FromImage(UIImage.FromFile(...))` for local paths. It then walks the `UIApplication.SharedApplication.GetKeyWindow()` view-controller chain to find the topmost presented VC and presents a `UINavigationController(browser)` modally.

Both platforms independently branch on "is this a URL or a local path?" — if you change the URL detection heuristic on one side, update the other (`MyMWPhotoBrower.Show` for iOS, `GallerySlidePageAdapter.IsLocalFilePath` for Android) to stay consistent.

## Releasing

Version strings live in the `<Version>` element of each csproj — they are not centralized:

- `PhotoBrowser.Maui/PhotoBrowser.Maui.csproj`
- `MWPhotoBrowserBinding/MWPhotoBrowserBinding.csproj`
- `SubsamplingScaleImageView/SubsamplingScaleImageView.csproj`
- `IDMPhotoBrowserBindings/IDMPhotoBrowserBindings.csproj`

All four have `GeneratePackageOnBuild=true` and produce `.nupkg` + `.snupkg` on every build. When bumping `PhotoBrowser.Maui`, bump the native binding versions first (if they changed) and update the `PackageReference` versions inside `PhotoBrowser.Maui.csproj` to match, otherwise consumers will resolve stale native bits.
