using System;
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
using phothoflow.location;
using phothoflow.setting;
using System.Windows.Controls.Primitives;
using phothoflow.filemanager;
using System.Threading;

namespace phothoflow
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, StepCallback
    {

        private Point origin;
        private Point start;

        public void OnFinish()
        {
            waitingList.ItemsSource = arrangement.GetUnarrangedItems();
        }

        public void OnStep(Item one)
        {
            float height = arrangement.GetTotalHeight();
            int rate = DisplayOptions.DisplayRate;
            if (MainContainer.Height < height * rate)
            {
                MainContainer.Height = height * rate;
            }
            if (MainContainer.Width != SettingManager.GetWidth() * rate) {
                MainContainer.Width = SettingManager.GetWidth() * rate;
            }

            Image img = new Image();
            img.Width = one.Width * rate;
            img.Height = one.Height * rate;
            img.Source = one.Preview;

            img.MouseLeftButtonDown += ImageClick;
            img.MouseLeftButtonUp += ImageRelease;
            img.MouseMove += image_MouseMove;

            Canvas.SetLeft(img, one.Left * rate);
            Canvas.SetTop(img, one.Top * rate);
            MainContainer.Children.Add(img);
        }
        
        Arrangement arrangement;
        public MainWindow()
        {
            InitializeComponent();
            SettingManager.Init();

            arrangement = new Arrangement(this);
            
        }

        void DrawContainer()
        {
            List<Item> arrange = arrangement.GetarrangedItems();
            MainContainer.Children.Clear();

            foreach (Item one in arrange)
            {
                OnStep(one);
            }

        }

        private void waitingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            Image image = e.Source as Image;
            if (!image.IsMouseCaptured) return;
            Point p = e.MouseDevice.GetPosition(MainContainer);

            Matrix m = image.RenderTransform.Value;
            m.OffsetX = origin.X + (p.X - start.X);
            m.OffsetY = origin.Y + (p.Y - start.Y);

            image.RenderTransform = new MatrixTransform(m);
        }

        private void ImageRelease(object sender, MouseButtonEventArgs e)
        {
            Image image = e.Source as Image;
            image.ReleaseMouseCapture();
        }

        private void ImageClick(object sender, MouseButtonEventArgs e)
        {
            Image image = e.Source as Image;
            if (image.IsMouseCaptured) return;
            image.CaptureMouse();

            start = e.GetPosition(MainContainer);
            origin.X = image.RenderTransform.Value.OffsetX;
            origin.Y = image.RenderTransform.Value.OffsetY;
        }


        private void Thumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            UIElement thumb = e.Source as UIElement;

            Canvas.SetLeft(thumb, Canvas.GetLeft(thumb) + e.HorizontalChange);
            Canvas.SetTop(thumb, Canvas.GetTop(thumb) + e.VerticalChange);
        }

        private void PopSetting(string str)
        {
            SettingDialog.IsOpen = true;
            SettingTitle.Text = str;
            PopUpdate();
        }

        private void PopUpdate()
        {
            Managable settingItem = SettingManager.Get(SettingTitle.Text);
            ChooseList.ItemsSource = settingItem.Get();
            ChooseList.SelectedIndex = settingItem.Current();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.Source as MenuItem;
            string str = (string)item.Header;
            switch(str) {
                case SettingManager.WIDTHWORDING:
                case SettingManager.MARGINGWORDING:
                case SettingManager.DPIWORDING:
                    PopSetting(str);
                    break;
                case "帮助":
                    break;
                case "关于":
                    MessageBox.Show("图片排列器");
                    break;
                case "导出":
                    SaveFile();
                    break;
                case "选择文件夹":
                    OpenFolder();
                    break;
                default:
                    break;
            }
        }


        void OpenFolder()
        {
            using (System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                folderBrowserDialog.ShowNewFolderButton = false;
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string path = folderBrowserDialog.SelectedPath;
                    Thread thread = new Thread(new ThreadStart(() =>
                    {
                        arrangement.Load(path);
                    }
                        ));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.IsBackground = true;
                    thread.Start();

                }
            }
            
        }

        void SaveFile()
        {
            FileWriter fw = new FileWriter();
            fw.Write("output.tif", arrangement.GetarrangedItems(), arrangement.GetTotalHeight());
        }

        private void PerformAdd()
        {
            Managable settingItem = SettingManager.Get(SettingTitle.Text);
            settingItem.Add(float.Parse(CreateData.Text));
            PopUpdate();
        }

        private void PerformConfirm()
        {
            SettingDialog.IsOpen = false;
            arrangement.Update();

        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            
            Button btn = e.Source as Button;
            switch ((string)btn.Tag)
            {
                case "AddData":
                    PerformAdd();
                    break;
                case "Confirm":
                case "Cancel":
                    PerformConfirm();
                    break;
                default:
                    break;
            }
        }

        private void ChooseList_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox mCB = sender as ComboBox;
            Managable settingItem = SettingManager.Get(SettingTitle.Text);
            settingItem.Select(mCB.SelectedIndex);
        }


    }
}
