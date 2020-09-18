using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ImageDPI.Properties;
using JetBrains.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImageDPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            Folder = Settings.Folder;
            Refresh();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true, InitialDirectory = Folder, EnsurePathExists = true };
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            Settings.Folder = Folder = dialog.FileName;
            Refresh();
        }

        private void Refresh()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            if (Directory.Exists(Folder))
            {
                Images = Directory.EnumerateFiles(Folder, "*.png", SearchOption.AllDirectories)
                  .Select(GetImage)
                  .Where(image => image != null)
                  .ToList()!;
            }
            else
            {
                Images = null;
            }

            Mouse.OverrideCursor = null;
        }

        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            var images = Images;
            if (images == null)
                return;

            Mouse.OverrideCursor = Cursors.Wait;

            try
            {

                foreach (var image in images.Where(image => (Math.Abs(image.DpiX - 96) > double.Epsilon) || (Math.Abs(image.DpiY - 96) > double.Epsilon)))
                {
                    var fileName = image.UriSource?.LocalPath;
                    if (fileName == null)
                        continue;

                    try
                    {
                        if (!FixImage(fileName))
                            return;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to load image " + fileName + "\n\n" + ex.Message, "Error", MessageBoxButton.OK);
                    }
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
                Refresh();
            }
        }

        // ReSharper disable once CognitiveComplexity
        private static bool FixImage(string fileName)
        {
            const string resolutionTag = "pHYs";

            var chunks = PngChunk.Load(fileName);

            if (chunks.All(chunk => chunk.Tag != resolutionTag))
                return true;

            while (true)
            {
                try
                {
                    PngChunk.Save(fileName, chunks.Where(chunk => chunk.Tag != resolutionTag).ToList());
                    break;
                }
                catch (Exception ex)
                {
                    var result = MessageBox.Show("Error fixing image, retry?\n\n" + fileName + "\n\n" + ex.Message, "Error", MessageBoxButton.YesNoCancel);

                    if (result == MessageBoxResult.No)
                        break;

                    if (result == MessageBoxResult.Cancel)
                        return false;
                }
            }

            return true;
        }

        [CanBeNull]
        private BitmapImage? GetImage(string fileName)
        {
            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;  // to release file
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = new Uri(fileName);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        // ReSharper disable once AssignNullToNotNullAttribute
        private Settings Settings => Settings.Default;

        [CanBeNull, ItemNotNull]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public IEnumerable<BitmapImage>? Images
        {
            get => (IEnumerable<BitmapImage>)GetValue(ImagesProperty);
            set => SetValue(ImagesProperty, value);
        }
        public static readonly DependencyProperty ImagesProperty =
            DependencyProperty.Register("Images", typeof(IEnumerable<BitmapImage>), typeof(MainWindow));

        [CanBeNull]
        public string? Folder
        {
            get => (string)GetValue(FolderProperty);
            set => SetValue(FolderProperty, value);
        }
        public static readonly DependencyProperty FolderProperty =
            DependencyProperty.Register("Folder", typeof(string), typeof(MainWindow));
    }
}
