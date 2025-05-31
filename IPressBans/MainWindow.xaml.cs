using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IPressBans
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            KeyboardInterceptor.Start();

            KeyboardInterceptor.ProfilesChanged += OnProfilesChanged;
        }

        protected override void OnClosed(System.EventArgs e)
        {
            KeyboardInterceptor.Stop();
            base.OnClosed(e);
        }

        private void SetTextSourcesButton_Click(object sender, RoutedEventArgs e)
        {
            KeyboardInterceptor.SetTextSources(
                () => TextBlock1.Text,
                () => TextBlock2.Text,
                () => TextBlock3.Text
            );
        }

        private void OnProfilesChanged(HashSet<int> activeProfiles)
        {
            // UI updates must be done on UI thread
            Dispatcher.Invoke(() =>
            {
                HighlightBorder(Border1, activeProfiles.Contains(1));
                HighlightBorder(Border2, activeProfiles.Contains(2));
                HighlightBorder(Border3, activeProfiles.Contains(3));
            });
        }

        private void HighlightBorder(Border border, bool highlight)
        {
            if (highlight)
            {
                border.BorderBrush = Brushes.Red;
                border.Background = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)); // light red
            }
            else
            {
                border.BorderBrush = Brushes.Gray;
                border.Background = Brushes.Transparent;
            }
        }
    }
}