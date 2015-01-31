using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Alphaleonis.Win32.Vss;

namespace Cobalt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

               
        public MainWindow()
        {
            InitializeComponent();
        }

        // COBALT methods start here
        public Tuple<string, string> getPaths()
        {
            string path_from = BoxPath1.Text;
            string path_to = BoxPath2.Text;
            Console.Write(path_from);
            Console.Write(path_to);
            var paths = Tuple.Create(path_from, path_to);
            return paths;
        }

        public void Backup(Tuple<string,string> paths)
        {
            String _Source1 = paths.Item1;
            String _Destination = paths.Item2;
            FileInfo MyFileInfo = new FileInfo(_Source1);
            String _Volume = MyFileInfo.Directory.Root.Name;
            IVssImplementation _vssImplementation = VssUtils.LoadImplementation();
            IVssBackupComponents _backup = _vssImplementation.CreateVssBackupComponents();
            _backup.InitializeForBackup(null);
            _backup.GatherWriterMetadata();
            _backup.SetContext(VssVolumeSnapshotAttributes.Persistent | VssVolumeSnapshotAttributes.NoAutoRelease);
            _backup.SetBackupState(false, true, Alphaleonis.Win32.Vss.VssBackupType.Full, false);
            Guid MyGuid01 = _backup.StartSnapshotSet();
            Guid MyGuid02 = _backup.AddToSnapshotSet(_Volume, Guid.Empty);
            // VSS step 5: Preparation (Writers & Provaiders need to start preparation)
            _backup.PrepareForBackup();
            // VSS step 6: Create a Snapshot For each volume in the "Snapshot Set"
            _backup.DoSnapshotSet();
            // GET drive labels in use
            var d_name = " ";
            var d_nam = " ";
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo d in allDrives)
            {
                if (d.Name != @"A:\")
                {
                    d_name = @"A:\";
                    d_nam  = @"A:";
                }
                else if (d.Name != @"B:\")
                {
                    d_name = @"B:\";
                    d_nam = @"A:";
                }
                else if (d.Name != @"Z:\")
                {
                    d_name = @"Z:\";
                    d_nam = @"Z:";
                }
            }
            _backup.ExposeSnapshot(MyGuid02, null, VssVolumeSnapshotAttributes.ExposedLocally, d_nam);
            String sVSSFile1 = _Source1.Replace(_Volume, d_name);
            if (File.Exists(sVSSFile1))
                System.IO.File.Copy(sVSSFile1, _Destination + @"\" + System.IO.Path.GetFileName(_Source1), true);
            foreach (VssSnapshotProperties prop in _backup.QuerySnapshots())
            {
                if (prop.ExposedName == d_name)
                {
                    Console.WriteLine("prop.ExposedNam Found!");
                    _backup.DeleteSnapshot(prop.SnapshotId, true);
                }
            }
            _backup = null;
            return;
        }
        // COBALT methods end here

        // EVENTS start here
        private void BtnGo_Click(object sender, RoutedEventArgs e)
        {
            imgOverlay.Visibility = Visibility.Visible;
            imgLoader.Visibility = Visibility.Visible;
        }
        private void BtnGo_Release(object sender, RoutedEventArgs e)
        {
            
            var paths = getPaths();
            System.Threading.Thread.Sleep(1000);
            Backup(paths);
            MessageBox.Show("Done.");
            imgOverlay.Visibility = Visibility.Hidden;
            imgLoader.Visibility = Visibility.Hidden;
        }

    }
}
