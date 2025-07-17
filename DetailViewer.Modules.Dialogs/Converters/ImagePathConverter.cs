using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace DetailViewer.Modules.Dialogs.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.Generic.List<string> imagePaths && imagePaths.Count > 0)
            {
                string imagePath = imagePaths[0];
                string fullPath = Path.Combine(Environment.CurrentDirectory, imagePath);
                System.Diagnostics.Debug.WriteLine($"Attempting to load image from: {fullPath}");
                if (File.Exists(fullPath))
                {
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(fullPath);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        System.Diagnostics.Debug.WriteLine($"Successfully loaded image from: {fullPath}");
                        return bitmap;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error loading image {fullPath}: {ex.Message}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Image file not found: {fullPath}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"No image paths provided or list is empty.");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}