using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace WowAddonSyncer
{
    public partial class MainWindow : MetroWindow
    {
        private string _wowDir;
        private string _dropboxDir;
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetStatus("Loaded!", true);
        }

        #region Handle Status Bar
        private void SetStatus(string message, bool state)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusMSG.Text = message;
                StatusMSG.Foreground = state ? Brushes.Green : Brushes.Red;
            }));
        }
        #endregion

        #region Move Data to Dropbox Task

        private Task<bool> MoveThemAll()
        {
            return Task.Run(() => { 
            if (!Directory.Exists(_dropboxDir + "\\WoW-Stuff"))
            {
                Directory.CreateDirectory(_dropboxDir + "\\WoW-Stuff");
                DirectoryCopier.CopyDirectory(_wowDir + "\\Interface", _dropboxDir + "\\WoW-Stuff\\Interface");
                DirectoryCopier.CopyDirectory(_wowDir + "\\WTF", _dropboxDir + "\\WoW-Stuff\\WTF");
                MakeSymLinks();
                return true;
            }
            else
            {
                var message = System.Windows.MessageBox.Show(
                    "Your data already exists in dropbox.\nDo you wish to setup links on this PC to your addon data?", 
                    "Caution", 
                    MessageBoxButton.YesNo);
                if (message == MessageBoxResult.No) return false;
                MakeSymLinks();
                return true;
            }
            });
        }
        #endregion

        #region Move data back to wow directory Task (rollback changes)
        private Task<bool> RollBack()
        {
            return Task.Run(() => { 
            if (!Directory.Exists(_dropboxDir + "\\WoW-Stuff"))
            {
                System.Windows.MessageBox.Show("WoW-Stuff folder in dropbox NOT FOUND!");
                return false;
            }
            if (Directory.Exists(_wowDir + "\\Interface"))
            {
                Directory.Delete(_wowDir + "\\Interface", true);
            }
            if (Directory.Exists(_wowDir + "\\WTF"))
            {
                Directory.Delete(_wowDir + "\\WTF", true);
            }

            if (Directory.Exists(_dropboxDir + "\\WoW-Stuff"))
            {
                DirectoryCopier.CopyDirectory(_dropboxDir + "\\WoW-Stuff\\Interface", _wowDir + "\\Interface");
                DirectoryCopier.CopyDirectory(_dropboxDir + "\\WoW-Stuff\\WTF", _wowDir + "\\WTF");
            }

            Directory.Delete(_dropboxDir + "\\WoW-Stuff", true);
            return true;
            });
        }
        #endregion

        #region Handle creation of Symbolic Link
        [DllImport("kernel32.dll")]
        static extern bool CreateSymbolicLink(
        string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        private void MakeSymLinks()
        {
            if (Directory.Exists(_wowDir + "\\Interface"))
            {
                Directory.Delete(_wowDir + "\\Interface", true);
            }
            if (Directory.Exists(_wowDir + "\\WTF"))
            {
                Directory.Delete(_wowDir + "\\WTF", true);
            }

            CreateSymbolicLink(_wowDir + "\\Interface", _dropboxDir + "\\WoW-Stuff\\Interface", SymbolicLink.Directory);
            CreateSymbolicLink(_wowDir + "\\WTF", _dropboxDir + "\\WoW-Stuff\\WTF", SymbolicLink.Directory);
        }
        #endregion

        #region Set Locations from WinForms Folder Browsers
        private void setWowFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var fb = new FolderBrowserDialog())
            {
                var result = fb.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.Cancel) return;
                _wowDir = fb.SelectedPath;
                WoWFolderBox.Text = _wowDir;
            }
            SetStatus("WoW Set", true);
        }

        private void setDropboxbutton_Click(object sender, RoutedEventArgs e)
        {
            using (var fb = new FolderBrowserDialog())
            {
                var result = fb.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.Cancel) return;
                _dropboxDir = fb.SelectedPath;
                DropBoxFolder.Text = _dropboxDir;
            }
            SetStatus("Dropbox Set", true);
        }
        #endregion

        #region Do it and Roll back button Handlers
        private async void DoIt_Click(object sender, RoutedEventArgs e)
        {
            CopyProgress.Visibility = System.Windows.Visibility.Visible;
            var processingDone = await MoveThemAll();
            if (processingDone == null || processingDone == false)
            {
                SetStatus("An Error Occured!", false);
            }
            if (processingDone)
            {
                SetStatus("All Done!", true);
            }
            CopyProgress.Visibility = System.Windows.Visibility.Hidden;
        }

        private async void RollBack_Click(object sender, RoutedEventArgs e)
        {
            CopyProgress.Visibility = System.Windows.Visibility.Visible;
            var processingDone = await RollBack();
            if (processingDone == null || processingDone == false)
            {
                SetStatus("An Error Occured!", false);
            }
            if (processingDone)
            {
                SetStatus("All Done!", true);
            }
            CopyProgress.Visibility = System.Windows.Visibility.Hidden;
        }
        #endregion

    }
}
