﻿using System;
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
using phothoflow.ipc;

namespace phothoflow
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, StepCallback
    {


        private Point start;
        ObservableCollection<Item> unarranged;
        Color Normal = Color.FromRgb(176, 196, 222);
        Color Chosen = Color.FromRgb(220, 20, 60);

        public void OnStart()
        {
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

        UIElement CreateMovable(Item one)
        {
            int rate = DisplayOptions.DisplayRate;
            Image img = new Image();
            img.Width = one.RealWidth * rate;
            img.Height = one.RealHeight * rate;
            if (!(one.Preview is BitmapFrame))
            {
                one.Preview = BitmapFrame.Create(one.Preview);
            }
            img.Source = one.Preview;

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

                Canvas.SetLeft(photo, one.Left * rate);
                Canvas.SetTop(photo, one.Top * rate);
                MainContainer.Children.Add(photo);
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
            ServerNamedPipe.PipManager = new PipeManager();
            ServerNamedPipe.PipManager.Initialize();
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

            if (!arrangement.AdjustItem(target, x, y))
            {
                OnStart();
                OnFinish();
            }
  
        }

        private void ImageClick(object sender, MouseButtonEventArgs e)
        {
            Border delta = sender as Border;
            if (delta.IsMouseCaptured) return;
            delta.BorderBrush = new SolidColorBrush(Chosen);
            start = e.MouseDevice.GetPosition(MainContainer);


            delta.CaptureMouse();
            
        }

        private void PopSetting(string str)
        {
            SettingDialog.IsOpen = true;
            SettingTitle.Text = str;
            PopUpdate();
        }

        private void PopUpdate()
        {
            CreateData.Text = "";
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
