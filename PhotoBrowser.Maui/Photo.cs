namespace PhotoBrowsers
{
    public interface IPhotoBrowser
    {
        void Show(PhotoBrowser photoBrowser);

        void Close();
    }

    // All the code in this file is included in all platforms.
    public class Photo
    {
        public string URL { get; set; }

        public string Title { get; set; }
    }

    public class PhotoBrowser
    {
        public List<Photo> Photos { get; set; }

        public int StartIndex { get; set; } = 0;

        public event EventHandler<int> PageChanged;

        public event EventHandler Closed;

        public void Show()
        {
            ServiceHelpers.GetService<IPhotoBrowser>().Show(this);
        }

        public static void Close()
        {
            ServiceHelpers.GetService<IPhotoBrowser>().Close();
        }

        internal void RaisePageChanged(int index) => PageChanged?.Invoke(this, index);

        internal void RaiseClosed() => Closed?.Invoke(this, EventArgs.Empty);
    }
}