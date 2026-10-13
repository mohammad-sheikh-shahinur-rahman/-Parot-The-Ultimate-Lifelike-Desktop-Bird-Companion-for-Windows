using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Parot.Views;

public partial class DesktopMirrorWindow : Window
{
    public DesktopMirrorWindow(string skin = "ScarletMacaw")
    {
        InitializeComponent();

        // Load mirror reflection frame
        try
        {
            string skinFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Birds", skin);
            if (!Directory.Exists(skinFolder))
            {
                skinFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Birds", "Macaw");
            }

            string[] files = Directory.GetFiles(skinFolder, "*.png");
            if (files.Length > 0)
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(files[0], UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                MirrorAvatarImage.Source = bmp;
            }
        }
        catch
        {
            // Default fallback
        }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
