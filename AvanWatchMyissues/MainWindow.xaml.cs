using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AvanWatchMyissues.ViewModel;
using System.Globalization;
using System.Diagnostics;

namespace AvanWatchMyissues.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            try {
                InitializeComponent();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString() + "\n\n" + ex.Message);
            }
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            Export();
        }
        void Export()
        {
            try
            {
                DataGrid datagrid = ReportTab.SelectedIndex == 0 ? IssueReportGrid : ResultGrid;
                string filename = "resultgrid" + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".xls";
                string exportedfile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + filename;
                datagrid.SelectAllCells();
                datagrid.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
                ApplicationCommands.Copy.Execute(null, datagrid);
                String resultat = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
                String result = (string)Clipboard.GetData(DataFormats.Text);
                datagrid.UnselectAllCells();
                System.IO.StreamWriter file1 = new System.IO.StreamWriter(exportedfile);
                file1.WriteLine(result.Replace(',', ' '));
                file1.Close();
                System.Diagnostics.Process.Start(exportedfile);
            }
            catch(Exception ex)
            {
                string str = ex.ToString();
            }
        }
        private void SendEmail_Click(object sender, RoutedEventArgs e)
        {
            SendMail();
        }
        void SendMail()
        {

        }
        private void WebPageClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = e.OriginalSource as Hyperlink;
            Process.Start(link.NavigateUri.AbsoluteUri);
        }
    }
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            return !booleanValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool booleanValue = (bool)value;
            return !booleanValue;
        }
    }
    class BoolToVisibleOrHidden : IValueConverter
    {
        #region Constructors
        /// <summary>
        /// The default constructor
        /// </summary>
        public BoolToVisibleOrHidden() { }
        #endregion

        #region Properties
        public bool Collapse { get; set; }
        public bool Reverse { get; set; }
        #endregion

        #region IValueConverter Members
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool bValue = (bool)value;

            if (bValue != Reverse)
            {
                return Visibility.Visible;
            }
            else
            {
                if (Collapse)
                    return Visibility.Collapsed;
                else
                    return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility visibility = (Visibility)value;

            if (visibility == Visibility.Visible)
                return !Reverse;
            else
                return Reverse;
        }
        #endregion
    }
    public class StoryToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string story = value as string;
            if(story.Substring(0,1).CompareTo("S") == 0 || story.Substring(0, 1).CompareTo("D") == 0)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    public class UrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string url = value.ToString();
                int index = url.LastIndexOf("/");
                string alias = url.Substring(index+1,url.Length-index-1);
                return alias;
            }
            else
            {
                string url = "";
                return url;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Uri email = new Uri((string)value);
            return email;
        }
    }
}
