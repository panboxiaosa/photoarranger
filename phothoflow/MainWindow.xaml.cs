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
using System.Collections.ObjectModel;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace phothoflow
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, StepCallback
    {

        private Point origin;
        private Point start;
        ObservableCollection<Item> unarranged;

        public void OnStart()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                MainContainer.Width = 100;
                MainContainer.Children.Clear();
                unarranged.Clear();
            }));
        }

        public void OnFinish()
        {
            foreach (Item val in arrangement.GetarrangedItems())
            {
                AddItem(val);
                this.Dispatcher.Invoke(new Action<Item>((one) =>{
                    unarranged.Remove(one);
                }), val);
            }

        }

        void AddItem(Item val)
        {
            this.Dispatcher.Invoke(new Action<Item>((one) =>
            {
                float height = arrangement.GetTotalHeight();
                int rate = DisplayOptions.DisplayRate;
                if (MainContainer.Height < height * rate)
                {
                    MainContainer.Height = height * rate;
                }
                if (MainContainer.Width != SettingManager.GetWidth() * rate)
                {
                    MainContainer.Width = SettingManager.GetWidth() * rate;
                }

                Image img = new Image();
                img.Width = one.RealWidth * rate;
                img.Height = one.RealHeight * rate;
                if (!(one.Preview is BitmapFrame))
                {
                    one.Preview = BitmapFrame.Create(one.Preview); 
                }
                img.Source = one.Preview;

                img.MouseLeftButtonDown += ImageClick;
                img.MouseLeftButtonUp += ImageRelease;
                img.MouseMove += image_MouseMove;

                Canvas.SetLeft(img, (one.Left + SettingManager.GetMargin()) * rate);
                Canvas.SetTop(img, (one.Top + SettingManager.GetMargin()) * rate);
                MainContainer.Children.Add(img);
            }), val);
        }

        public void OnStep(Item val)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (!(val.Preview is BitmapFrame))
                {
                    val.Preview = BitmapFrame.Create(val.Preview);
                }
                unarranged.Add(val);
            }));
        }
        
        Arrangement arrangement;
        public MainWindow()
        {
            InitializeComponent();
            SettingManager.Init();
            arrangement = new Arrangement(this);
            unarranged = new ObservableCollection<Item>();
            waitingList.ItemsSource = unarranged;
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
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (CommonFileDialogResult.Ok == dialog.ShowDialog()) {
                string path = dialog.FileName;
                new Thread(() => { arrangement.Load(path); }).Start();
            }
            
            
        }

        void SaveFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            //设置保存的文件的类型，注意过滤器的语法  
            sfd.Filter = "tif文件|*.tif";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮  
            if (sfd.ShowDialog() == true)
            {
                FileWriter fw = new FileWriter();
                fw.Write(sfd.FileName, arrangement.GetarrangedItems(), arrangement.GetTotalHeight());
            }
            else
            {
                MessageBox.Show("取消保存");
            }  

            
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
            arrangement.Remargin();
            arrangement.Arrange();

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
