using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using Microsoft.Win32;
using WPFFolderBrowser;

namespace LMT_Images_Compress
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ButtonStart.DataContext = btnProgressBar;
            ListViewData.AllowDrop = true;

            #region Send to exe

            args = App.args;
            if (args != null)
            {
                foreach (var path in args)
                {
                    var att = File.GetAttributes(path);
                    if (att.HasFlag(FileAttributes.Directory))
                        dsFolder.Add(path);
                    else if (exAllow.Contains(System.IO.Path.GetExtension(path)))
                        dsImageFile.Add(path);
                }
                workerListView.RunWorkerAsync();
            }
            #endregion

            #region Worker
            workerListView.DoWork += WorkerListView_DoWork;
            workerListView.RunWorkerCompleted += WorkerListView_RunWorkerCompleted;
            workerCompressImage.DoWork += WorkerCompressImage_DoWork;
            workerCompressImage.RunWorkerCompleted += WorkerCompressImage_RunWorkerCompleted;

            #endregion
        }



        #region Worker

        private void WorkerListView_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ListViewData.ItemsSource = dsImages;
            ListViewData.Items.Refresh();
            folderPath = "";
            dsImageFile.Clear();
            dsFolder.Clear();
            ButtonAddFile.IsEnabled = ButtonAddFolder.IsEnabled = ButtonStart.IsEnabled = ListViewData.IsEnabled = true;
        }

        private void WorkerListView_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ButtonAddFile.IsEnabled = ButtonAddFolder.IsEnabled = ButtonStart.IsEnabled = ListViewData.IsEnabled = false;
            });
            foreach (var item in dsFolder)
            {
                FileinFoler(item);
            }
            foreach (string item in dsImageFile)
            {
                FileInfo info = new FileInfo(item);
                _filename = info.Name;
                _path = info.FullName;
                _size = info.Length;
                dsImages.Add(new ListViewImage() { FileName = _filename, Path = _path, Size = Convert.ToString(_size) });
            }
        }

        private void WorkerCompressImage_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SliderMucNenAnh.IsEnabled = RadioButtonLMTCompress.IsEnabled = RadioButtonOptiPng.IsEnabled= ButtonStart.IsEnabled=ButtonRenew.IsEnabled=ButtonFolderOutput.IsEnabled  = true;
            dsFile.Clear();
            ListViewData.Items.Refresh();
            dsImageCompressed.Clear();
            isRunning = false;
            btnProgressBar.ButtonProgressValueString = "Bắt đầu";
            btnProgressBar.ButtonProgressValue = 0;
        }

        private void WorkerCompressImage_DoWork(object sender, DoWorkEventArgs e)
        {
            int _optivalue = 0;

            Dispatcher.Invoke(() =>
            {
                ButtonAddFile.IsEnabled = ButtonAddFolder.IsEnabled=ButtonRenew.IsEnabled=ButtonFolderOutput.IsEnabled = SliderMucNenAnh.IsEnabled = RadioButtonLMTCompress.IsEnabled = RadioButtonOptiPng.IsEnabled = false;
                if (!compressOptiPNG)
                {
                    if (optiValue == 1) _optivalue = 80;
                    if (optiValue == 2) _optivalue = 60;
                    if (optiValue == 3) _optivalue = 40;
                }
            });
            dsFile = GetAllItemListview();
            for (int i = 0; i < dsFile.Count; i++)
            {
                ImagesCompress.CompressImage(dsFile[i], folderOutput, _optivalue, 100, SupportedMimeType.JPEG);
                btnProgressBar.ButtonProgressValue = (int)Math.Truncate((double)(i + 1) / dsFile.Count * 100);
                btnProgressBar.ButtonProgressValueString = btnProgressBar.ButtonProgressValue + " %";
            }
            dsImages.Clear();
            dsImageCompressed = DeleteDuplicates(dsImageCompressed);
            foreach (string item in dsImageCompressed)
            {
                FileInfo info = new FileInfo(item);
                _filename = info.Name;
                _path = info.FullName;
                _size = info.Length;
                dsImages.Add(new ListViewImage() { FileName = _filename, Path = _path, Size = Convert.ToString(_size) });
            }
        }
        #endregion

        #region Var

        private string[] args;
        private bool compressOptiPNG = false;
        private bool isRunning = false;
        private string folderPath;
        private string folderOutput;
        private string _path;
        private string _filename;
        private long _size;
        private int optiValue=1;
        List<string> dsImageCompressed = new List<string>();
        List<string> dsImageFile = new List<string>();
        List<string> dsFile = new List<string>();
        private List<string> dsFolder = new List<string>();
        List<ListViewImage> dsImages = new List<ListViewImage>();
        private List<string> exAllow = new List<string>(new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff" });

        BackgroundWorker workerListView = new BackgroundWorker();
        BackgroundWorker workerCompressImage = new BackgroundWorker();

        private CancellationTokenSource _cancelTokenSource;


        private FileSystemWatcher watcher;

        private ButtonProgressBar btnProgressBar = new ButtonProgressBar() { ButtonProgressValue = 0, ButtonProgressValueString = "Bắt đầu" };
        #endregion

        #region Func

        /// <summary>
        /// CHon cac file hinh
        /// </summary>
        private void SelectFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp, *.tif, *.tiff) | *.jpg; *.jpeg; *.bmp; *.tif; *tiff; *.png";
            dlg.Multiselect = true;
            bool? result = dlg.ShowDialog();
            if (result == true)
            {
                dsImageFile.AddRange(dlg.FileNames);
            }
        }

        /// <summary>
        /// Chon thu muc hinh anh
        /// </summary>
        private void SelectFolder()
        {
            //FolderSelectDialog dlg = new FolderSelectDialog();
            //dlg.Title = "Chọn thư mục hình ảnh";
            //if (dlg.ShowDialog())
            //    dsFolder.Add(dlg.Folder);
            WPFFolderBrowserDialog dlg= new WPFFolderBrowserDialog("Chọn thư mục hình ảnh");
            bool? result = dlg.ShowDialog();
            if(result==true)
                dsFolder.Add(dlg.FileName);
        }

        /// <summary>
        /// Lay danh sach cac file hinh anh trong thu muc 
        /// </summary>
        /// <param name="location"></param>
        private void FileinFoler(string location)
        {
            try
            {
                string[] files = Directory.GetFiles(location);
                string[] folders = Directory.GetDirectories(location);
                for (int i = 0; i < files.Length; i++)
                {
                    if (exAllow.Contains(System.IO.Path.GetExtension(files[i]).ToLower()))
                        dsImageFile.Add(files[i]);
                }
                for (int i = 0; i < folders.Length; i++)
                {
                    FileinFoler(folders[i]);
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <summary>
        /// Lay duong dan hinh anh tu listview
        /// </summary>
        /// <returns></returns>
        private List<string> GetAllItemListview()
        {
            List<string> result = new List<string>();
            foreach (var item in ListViewData.Items)
            {
                var a = (ListViewImage)item;
                result.Add(a.Path);
            }
            return result;
        }

        /// <summary>
        /// Theo doi cac file nen duoc tao ra trong thu muc output
        /// </summary>
        private void WatchFolderOutput()
        {
            watcher = new FileSystemWatcher();
            watcher.Path = folderOutput;
            watcher.Filter = "*.*";
            watcher.NotifyFilter = NotifyFilters.Attributes |
                                   NotifyFilters.CreationTime |
                                   NotifyFilters.DirectoryName |
                                   NotifyFilters.FileName |
                                   NotifyFilters.LastAccess |
                                   NotifyFilters.LastWrite |
                                   NotifyFilters.Security |
                                   NotifyFilters.Size;
            //watcher.Created += Watcher_Created;
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string ex = System.IO.Path.GetExtension(e.FullPath);
            if (ex.ToLower() == ".png" || ex.ToLower() == ".jpg")
                dsImageCompressed.Add(e.FullPath);
        }

        

        /// <summary>
        /// An enter de xem anh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewData_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var a = (ListViewImage)ListViewData.SelectedItems[0];
                if (a != null)
                    Process.Start(a.Path);
            }
            if (e.Key == Key.Delete)
            {
                try
                {
                    dsImages.RemoveAt(ListViewData.SelectedIndex);
                    ListViewData.Items.Refresh();
                }
                catch (Exception)
                {

                }
            }
        }

        /// <summary>
        /// Double click de xem hinh 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            var a = (ListViewImage)ListViewData.SelectedItems[0];
            if (a != null)
                Process.Start(a.Path);

        }

        /// <summary>
        /// Ke tha thu muc va file vao phan mem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewData_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true) == true)
            {
                string[] dsPath = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                foreach (var path in dsPath)
                {
                    var att = File.GetAttributes(path);
                    if (att.HasFlag(FileAttributes.Directory))
                        dsFolder.Add(path);
                    else if (exAllow.Contains(System.IO.Path.GetExtension(path)))
                        dsImageFile.Add(path);
                }
                workerListView.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Xoa trung lap trong list
        /// </summary>
        /// <param name="inputList"></param>
        /// <returns></returns>
        private List<string> DeleteDuplicates(List<string> inputList)
        {
            List<string> list = new List<string>();
            foreach (string current in inputList)
            {
                if (!Contains(list, current) && current!=null)
                {
                    list.Add(current);
                }
            }
            return list;
        }

        private bool Contains(List<string> list, string comparedValue)
        {
            bool result;
            foreach (string current in list)
            {
                if (current == comparedValue)
                {
                    result = true;
                    return result;
                }
            }
            result = false;
            return result;
        }

        /// <summary>
        /// danh sach anh se duoc nen bang optipng
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<string> OptiPNGImageList(List<string> list)
        {
            List<string> result = new List<string>();
            foreach (string item in list)
            {
                string ex = System.IO.Path.GetExtension(item);
                if (ex.ToLower() == ".png" || ex.ToLower() == ".bmp" || ex.ToLower() == ".tiff")
                    result.Add(item);
            }
            return result;
        }

        private string FormatArguments(string input, int optLevel, string output)
        {
            return string.Format(CultureInfo.InvariantCulture, "-o{0} −clobber \"{1}\" -dir {2}",
                optLevel, input, output);
        }

        /// <summary>
        /// Tien hanh nen anh
        /// </summary>
        /// <returns></returns>
        private async Task OptiPng()
        {
            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Dispose();
            }
            _cancelTokenSource = new CancellationTokenSource();
            double progress = 0;
            ButtonAddFile.IsEnabled = ButtonAddFolder.IsEnabled=ButtonRenew.IsEnabled=ButtonFolderOutput.IsEnabled = SliderMucNenAnh.IsEnabled = RadioButtonLMTCompress.IsEnabled = RadioButtonOptiPng.IsEnabled = false;
            List<string> img = OptiPNGImageList(GetAllItemListview());
            await Task.Factory.StartNew(() =>
        {
            try
            {
                Parallel.ForEach(img,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = 3,
                        CancellationToken = _cancelTokenSource.Token
                    }, (s) =>
                    {
                        using (var p = new Process
                        {
                            StartInfo = new ProcessStartInfo("optipng")
                            {
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                Arguments = FormatArguments(s, optiValue, folderOutput),
                                RedirectStandardError = true,
                            }
                        })
                        {
                            p.Start();
                            p.BeginErrorReadLine();
                            p.WaitForExit();
                            
                            btnProgressBar.ButtonProgressValue = (int)Math.Truncate(++progress / img.Count * 100);
                            btnProgressBar.ButtonProgressValueString =
                                btnProgressBar.ButtonProgressValue + " %";
                        }
                    });

                #region End task
                dsImages.Clear();
                dsImageCompressed = DeleteDuplicates(dsImageCompressed);
                foreach (string item in dsImageCompressed)
                {
                    var bak = item + ".bak";
                    if (File.Exists(bak))
                        File.Delete(bak);
                    FileInfo info = new FileInfo(item);
                    _filename = info.Name;
                    _path = info.FullName;
                    _size = info.Length;
                    dsImages.Add(new ListViewImage() { FileName = _filename, Path = _path, Size = Convert.ToString(_size) });
                }
                Dispatcher.Invoke(() =>
                {
                    ListViewData.Items.Refresh();
                    ButtonStart.IsEnabled = true;
                    dsImageCompressed.Clear();
                    isRunning = false;
                    btnProgressBar.ButtonProgressValueString = "Bắt đầu";
                    btnProgressBar.ButtonProgressValue = 0;
                    SliderMucNenAnh.IsEnabled = RadioButtonLMTCompress.IsEnabled = RadioButtonOptiPng.IsEnabled = ButtonStart.IsEnabled = ButtonRenew.IsEnabled=ButtonFolderOutput.IsEnabled = true;
                });
                
                #endregion
            }
            catch (Exception)
            {
                //Hien thi tren button start khi cancel
                btnProgressBar.ButtonProgressValueString = "Bắt đầu";
                btnProgressBar.ButtonProgressValue = 0;
                dsImages.Clear();
                dsImageCompressed = DeleteDuplicates(dsImageCompressed);
                foreach (string item in dsImageCompressed)
                {
                    var bak = item + ".bak";
                    if (File.Exists(bak))
                        File.Delete(bak);
                    FileInfo info = new FileInfo(item);
                    _filename = info.Name;
                    _path = info.FullName;
                    _size = info.Length;
                    dsImages.Add(new ListViewImage() { FileName = _filename, Path = _path, Size = Convert.ToString(_size) });
                }
                Dispatcher.Invoke(() =>
                {
                    ListViewData.Items.Refresh();
                    SliderMucNenAnh.IsEnabled = RadioButtonLMTCompress.IsEnabled = RadioButtonOptiPng.IsEnabled = ButtonStart.IsEnabled = ButtonRenew.IsEnabled=ButtonFolderOutput.IsEnabled = true;
                });
                dsImageCompressed.Clear();
                isRunning = false;
                
            }

        });
        }
        #endregion



        #region Button,Radio button, slider

        private void ButtonAddFile_OnClick(object sender, RoutedEventArgs e)
        {
            SelectFile();
            workerListView.RunWorkerAsync();
        }

        private void ButtonAddFolder_OnClick(object sender, RoutedEventArgs e)
        {
            SelectFolder();
            workerListView.RunWorkerAsync();
        }

        /// <summary>
        /// Chon thu muc output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonFolderOutput_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                
                WPFFolderBrowserDialog dlg= new WPFFolderBrowserDialog("Chọn thư mục lưu hình ảnh");
                bool? result = dlg.ShowDialog();
                if (result == true)
                {
                    folderOutput = TextBoxFolderOutput.Text = dlg.FileName;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString());
            }
        }
        private void RadioButtonLMTCompress_OnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                compressOptiPNG = false;
                SliderMucNenAnh.Minimum = 1;
                SliderMucNenAnh.Maximum = 3;
                SliderMucNenAnh.Value = 1;
            }
            catch (Exception)
            {

            }
        }

        private void RadioButtonOptiPng_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                compressOptiPNG = true;
                SliderMucNenAnh.Minimum = 1;
                SliderMucNenAnh.Maximum = 7;
                SliderMucNenAnh.Value = 1;
            }
            catch (Exception)
            {

            }
        }

        private void SliderMucNenAnh_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                optiValue = (int)e.NewValue;
                if (!compressOptiPNG)
                {
                    switch (optiValue)
                    {
                        case 1:
                            TextBlockOptivalue.Text =
                                "Nén mức tiêu chuẩn-Hình ảnh sẽ được nén 1 cách tối ưu và nhanh nhất (tác giả khuyên bạn nên chọn mức này)";
                            break;
                        case 2:
                            TextBlockOptivalue.Text = "Nén ảnh mức 2";
                            break;
                        case 3:
                            TextBlockOptivalue.Text =
                                "Nén ảnh mức 3";
                            break;
                    }
                }
                else
                {
                    switch (optiValue)
                    {
                        case 1:
                            TextBlockOptivalue.Text =
                                "Nén mức tiêu chuẩn-Hình ảnh sẽ được nén 1 cách tối ưu và nhanh nhất (tác giả khuyên bạn nên chọn mức này)";
                            break;
                        case 2:
                            TextBlockOptivalue.Text = "Nén ảnh mức 2";
                            break;
                        case 3:
                            TextBlockOptivalue.Text = "Nén ảnh mức 3";
                            break;
                        case 4:
                            TextBlockOptivalue.Text = "Nén ảnh mức 4";
                            break;
                        case 5:
                            TextBlockOptivalue.Text = "Nén ảnh mức 5";
                            break;
                        case 6:
                            TextBlockOptivalue.Text = "Nén ảnh mức 6";
                            break;
                        case 7:
                            TextBlockOptivalue.Text =
                                "Nén ảnh mức siêu cấp (Tác giả khuyên bạn không nên chọn mức này vì có thể sẽ rất lâu)";
                            break;
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private async void ButtonStart_OnClick(object sender, RoutedEventArgs e)
        {
            TextBoxSearch.Text = "";
            if (!isRunning)
            {
                if (!compressOptiPNG)
                {
                    if (!String.IsNullOrEmpty(TextBoxFolderOutput.Text))
                    {
                        if (ListViewData.Items.Count > 0)
                        {
                            WatchFolderOutput();
                            isRunning = true;
                            btnProgressBar.ButtonProgressValueString = "0%";
                            btnProgressBar.ButtonProgressValue = 0;
                            workerCompressImage.RunWorkerAsync();
                        }
                        else
                            MessageBox.Show("Bạn chưa chọn hình ảnh để nén!", "Thông báo", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                    }
                    else
                        MessageBox.Show("Bạn chưa chọn thư mục lưu ảnh!", "Thông báo", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                }
                else
                {
                    if (!String.IsNullOrEmpty(TextBoxFolderOutput.Text))
                    {
                        if (ListViewData.Items.Count > 0)
                        {
                            isRunning = true;
                            btnProgressBar.ButtonProgressValueString = "0%";
                            btnProgressBar.ButtonProgressValue = 0;
                            WatchFolderOutput();
                            await OptiPng();
                        }
                        else
                            MessageBox.Show("Bạn chưa chọn hình ảnh để nén!", "Thông báo", MessageBoxButton.OK,
                                MessageBoxImage.Error);
                    }
                    else
                        MessageBox.Show("Bạn chưa chọn thư mục lưu ảnh!", "Thông báo", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                }
            }
            else
            {
                if (!compressOptiPNG)
                {
                    dsFile.Clear();
                    isRunning = false;
                    ButtonStart.IsEnabled = false;
                }
                else
                {
                    if (!_cancelTokenSource.IsCancellationRequested)
                    {
                        _cancelTokenSource.Cancel();
                        isRunning = false;
                        ButtonStart.IsEnabled = false;
                        var chromeDriverProcesses = Process.GetProcesses().
                            Where(pr => pr.ProcessName == "optipng");

                        foreach (var process in chromeDriverProcesses)
                        {
                            process.Kill();
                        }
                    }
                }
            }

        }

        private void ButtonRenew_OnClick(object sender, RoutedEventArgs e)
        {
            ListViewData.ItemsSource = null;
            ListViewData.Items.Clear();
            dsImages.Clear();
            ButtonAddFile.IsEnabled = ButtonAddFolder.IsEnabled = true;
            //watcher.EnableRaisingEvents = false;
        }

        private void TextBoxSearch_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxSearch.Text != "")
            {
                var list = from c in dsImages
                    where c.FileName.Contains(TextBoxSearch.Text)
                    select c;
                if (list != null) ListViewData.ItemsSource = list.ToList();
            }
            else ListViewData.ItemsSource = dsImages;
        }
        #endregion


        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
    }

    class ListViewImage : INotifyPropertyChanged
    {
        private string filename;
        public string FileName
        {
            get { return filename; }
            set
            {
                filename = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public string Size { get; set; }

        public long newSize { get; set; }


        public string OldSize
        {
            get
            {
                return GetFileSize(Size);
            }
            set
            {
                Size = value;
                OnPropertyChanged(nameof(OldSize));
            }
        }
        private string GetFileSize(string size)
        {
            string result;
            double sum = Convert.ToDouble(size);
            if (sum >= 1048576.0 && sum < 1073741824.0)
            {
                sum /= 1047576.0;
                result = string.Format("{0:0.00}", sum) + " MB";
            }
            else if (sum >= 1073741824.0)
            {
                sum /= 1073741824.0;
                result = string.Format("{0:0.00}", sum) + " GB";
            }
            else
            {
                sum /= 1024.0;
                result = string.Format("{0:0.00}", sum) + " KB";
            }

            return result;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class ButtonProgressBar : INotifyPropertyChanged
    {
        private string buttonProgressValueString;

        public string ButtonProgressValueString
        {
            get { return buttonProgressValueString; }
            set
            {
                buttonProgressValueString = value;
                OnPropertyChanged(nameof(ButtonProgressValueString));
            }
        }

        public int ButtonProgressValue
        {
            get { return buttonProgressValue; }

            set
            {
                buttonProgressValue = value;
                OnPropertyChanged(nameof(ButtonProgressValue));
            }
        }

        private int buttonProgressValue;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
