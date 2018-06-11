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
using Yzmeir.InterProcessComm;
using Yzmeir.NamedPipes;
using NamedPipesServer;
using System.Diagnostics;
using phothoflow.ipc;

namespace phothoflow
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, LoadCallback, ArrangeCallback
    {
        public static IChannelManager PipeManager;
        Arrangement arrangement;
        private Point start;
        ObservableCollection<Item> unarranged;

        Color Normal = Color.FromRgb(176, 196, 222);
        Color Chosen = Color.FromRgb(220, 20, 60);


        public void OnArrangeStart()
        {
            arrangement.checkRotate();
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (MainContainer.Width != SettingManager.GetWidth() * DisplayOptions.DisplayRate)
                {
                    MainContainer.Width = SettingManager.GetWidth() * DisplayOptions.DisplayRate;
                }
                MainContainer.Height = 100;
                MainContainer.Children.Clear();
            }));
        }

        public void OnArrangeFinish()
        {
            foreach (Item val in arrangement.GetarrangedItems())
            {
                AddItem(val);
                this.Dispatcher.Invoke(new Action<Item>((one) =>
                {
                    unarranged.Remove(one);
                }), val);
            }
        }

        public void OnDrawBack(Item item)
        {
            unarranged.Add(item);
        }

        public void OnLoadStart()
        {
            unarranged.Clear();
        }

        public void OnLoadFinish()
        {
            arrangement.Arrange();
        }

        public void OnLoadStep(string str)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                Item val = new Item(str);                
                unarranged.Add(val);
                arrangement.AddElement(val);

            }));
        }

        UIElement CreateMovable(Item one)
        {
            int rate = DisplayOptions.DisplayRate;
            Image img = new Image();
            img.Width = one.RealWidth * rate;
            img.Height = one.RealHeight * rate;
            if (one.Rotated)
            {
                var rotate = new RotateTransform(90);
                img.Source = new TransformedBitmap(one.Preview, rotate);
            } else {
                img.Source = one.Preview;
            }            

            Border border = new Border();
            border.Width = one.Width * rate;
            border.Height = one.Height * rate;
            border.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            border.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            border.BorderBrush = new SolidColorBrush(Normal);
            border.BorderThickness = new Thickness(0.5);

            border.Child = img;
            border.Tag = one;
            return border;
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

                UIElement photo = CreateMovable(val);
                photo.MouseLeftButtonDown += ImageClick;
                photo.MouseLeftButtonUp += ImageRelease;
                photo.MouseMove += ImageMouseMove;
                photo.MouseRightButtonUp += ImageSelect;

                Canvas.SetLeft(photo, one.Left * rate);
                Canvas.SetTop(photo, one.Top * rate);
                MainContainer.Children.Add(photo);
            }), val);
        }

        public MainWindow()
        {
            InitializeComponent();
            SettingManager.Init();
            arrangement = new Arrangement(this);
            unarranged = new ObservableCollection<Item>();
            waitingList.ItemsSource = unarranged;
            PipeManager = new PipeManager(this);
            PipeManager.Initialize();
        }

        private void ImageSelect(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("是否删除图片", "点击确定删除图片", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                // do something
                Border delta = sender as Border;
                Item target = delta.Tag as Item;
                arrangement.DrawBackItem(target);
            }

        }


        private void ImageMouseMove(object sender, MouseEventArgs e)
        {
            UIElement delta = sender as UIElement;
            if (!delta.IsMouseCaptured) return;
            Point p = e.MouseDevice.GetPosition(MainContainer);

            Matrix m = delta.RenderTransform.Value;
            m.OffsetX = p.X - start.X;
            m.OffsetY = p.Y - start.Y;

            delta.RenderTransform = new MatrixTransform(m);
        }

        private void ImageRelease(object sender, MouseButtonEventArgs e)
        {
            Border delta = sender as Border;
            delta.BorderBrush = new SolidColorBrush(Normal);
            delta.ReleaseMouseCapture();

            Matrix m = delta.RenderTransform.Value;
            float x = (float)((Canvas.GetLeft(delta) + m.OffsetX)/ DisplayOptions.DisplayRate);
            float y = (float)((Canvas.GetTop(delta) + m.OffsetY)/ DisplayOptions.DisplayRate);
            Item target = delta.Tag as Item;

            arrangement.AdjustItem(target, x, y);
  
        }

        private void ImageClick(object sender, MouseButtonEventArgs e)
        {
            Border delta = sender as Border;
            if (delta.IsMouseCaptured) return;
            delta.BorderBrush = new SolidColorBrush(Chosen);
            start = e.MouseDevice.GetPosition(MainContainer);

            delta.CaptureMouse();
            
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
                case "保存":
                    SaveFile();
                    break;
                case "导出图片":
                    SavePic();
                    break;
                case "打开文件":
                    LoadPersonal();
                    break;
                case "打开文件夹":
                    OpenFolder();
                    break;
                default:
                    break;
            }
        }

        void LoadPersonal()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //设置打开的文件的类型，注意过滤器的语法  
            ofd.Filter = "pbf文件|*.pbf";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮  
            if (ofd.ShowDialog() == true)
            {
                Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + "imgmerge.exe ",  "-f " + ofd.FileName);
            }
            else
            {
                MessageBox.Show("没有选择文件");
            }  
            
        }

        void OpenFolder()
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (CommonFileDialogResult.Ok == dialog.ShowDialog()) {
                string path = dialog.FileName;
                new Thread(() => {
                    ProcessExecutor.ExecuteSilent(System.AppDomain.CurrentDomain.BaseDirectory + "imgmerge.exe", "-l " + path);

                }).Start();
            }
            
        }

        void SavePic()
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

        void SaveFile()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            //设置保存的文件的类型，注意过滤器的语法  
            sfd.Filter = "pbf文件|*.pbf";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮  
            if (sfd.ShowDialog() == true)
            {
                FileWriter fw = new FileWriter();
                fw.Save(sfd.FileName, arrangement.GetarrangedItems(), arrangement.GetTotalHeight());
            }
            else
            {
                MessageBox.Show("取消保存");
            }  
        }

        string origin;

        private void PopSetting(string str)
        {
            SettingDialog.IsOpen = true;
            SettingTitle.Text = str;

            PopUpdate();
            origin = ChooseList.SelectedValue as string;
        }

        private void PopUpdate()
        {
            CreateData.Text = "";
            Managable settingItem = SettingManager.Get(SettingTitle.Text);
            ChooseList.ItemsSource = settingItem.Get();
            ChooseList.SelectedIndex = settingItem.Current();

        }

        private void ChooseList_DropDownClosed(object sender, EventArgs e)
        {
            ComboBox mCB = sender as ComboBox;
            Managable settingItem = SettingManager.Get(SettingTitle.Text);
            settingItem.Select(mCB.SelectedIndex);
        }


        private void Confirm_Click(object sender, RoutedEventArgs e)
        {

            Button btn = e.Source as Button;
            switch ((string)btn.Tag)
            {
                case "AddData":
                    Managable settingItem = SettingManager.Get(SettingTitle.Text);
                    bool added = settingItem.Add(float.Parse(CreateData.Text));
                    PopUpdate();
                    break;
                case "Confirm":
                    string confirmVal = ChooseList.SelectedValue as string;
                    if (origin != confirmVal && SettingTitle.Text != SettingManager.DPIWORDING)
                    {
                        if (SettingTitle.Text == SettingManager.MARGINGWORDING)
                            arrangement.Remargin();
                        arrangement.Arrange();
                    }
                    SettingDialog.IsOpen = false;
                    break;
                default:
                    break;
            }
        }

        private void waitingList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = waitingList.SelectedIndex;
            Item tar = unarranged[index];
            
            if (arrangement.ArrangeElement(tar))
                unarranged.Remove(tar);
        }

    }
}
