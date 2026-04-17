namespace PhotoBrowsers.Platforms.Android
{
    internal static class PhotoBrowserCallbacks
    {
        public static PhotoBrowser Current { get; set; }

        public static void PageChanged(int index) => Current?.RaisePageChanged(index);

        public static void Closed()
        {
            var current = Current;
            Current = null;
            current?.RaiseClosed();
        }
    }
}
