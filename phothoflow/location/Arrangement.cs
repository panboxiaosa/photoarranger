using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using phothoflow.filemanager;
using System.Threading;
using phothoflow.setting;


namespace phothoflow.location
{
    class Arrangement
    {
        StepCallback callback;

        ArrangeCalcer calcer = new ArrangeCalcer();
        List<Item> currentArrange;
        List<Item> allItemList;
        
        public Arrangement(StepCallback callback_)
        {
            callback = callback_;
            
        }

        public float GetTotalHeight()
        {
            float bottom = 0;
            foreach (Item item in currentArrange)
            {
                if (item.Top + item.Height > bottom)
                {
                    bottom = item.Top + item.Height;
                }
            }
            return bottom;
        }

        public List<Item> GetarrangedItems()
        {
            return currentArrange;
        }

        public void Remargin()
        {
            if (allItemList == null)
            {
                return;
            }
            foreach (Item one in allItemList)
            {
                one.Width = one.RealWidth + SettingManager.GetMargin() * 2;
                one.Height = one.RealHeight + SettingManager.GetMargin() * 2;
            }
        }


        public void Load(string path)
        {
            allItemList = new List<Item>();
            List<string> images = ImageList.listDirectory(path);

            int seg = 1;// Environment.ProcessorCount;
            int each = images.Count / seg;

            List<Thread> ts = new List<Thread>();
            for (int i = 0; i < seg; i++)
            {
                List<string> part = new List<string>();
                int num = i == seg - 1 ? images.Count - each * (seg - 1) : each;
                part.AddRange(images.GetRange(i * each, num));

                Thread t = new Thread(new ParameterizedThreadStart((object partList) =>
                {
                    List<string> toBeLoad = (List<string>)partList;
                    foreach (String pathStr in toBeLoad)
                    {
                        Item item = new Item(pathStr);
                        lock (allItemList)
                        {
                            allItemList.Add(item);
                            callback.OnStep(item);
                        }
                    }
                }));
                t.Start(part);
                ts.Add(t);
            }

            foreach (Thread t in ts)
                t.Join();


            Arrange();
        }

        MemoryStream GetMemoryStream(string path)
        {
            BinaryReader myBR = new BinaryReader(File.Open(path, FileMode.Open));
            FileInfo myFI = new FileInfo(path);
            byte[] myBytes = myBR.ReadBytes((int)myFI.Length);
            myBR.Close();
            MemoryStream myMS2 = new MemoryStream(myBytes);
            return myMS2;
        }
        
        public void Arrange()
        {
            if (allItemList == null)
            {
                return;
            }
            callback.OnStart();
            currentArrange = new List<Item>();
            foreach (Item item in allItemList)
            {
                AutoArrange(item);
            }
            callback.OnFinish();
        }

        public void Recession(Item item)
        {
            currentArrange.Remove(item);
        }

        public void AutoArrange(Item item)
        {
            int position = calcer.FindSuitable(currentArrange, item);
            if (position != -1)
            {
                currentArrange.Insert(position, item);
            }
        }

        public void ManualArrange(Item item, int x, int y)
        {
            int position = calcer.FindSuitable(currentArrange, item, x, y);
            currentArrange.Insert(position, item);
        }

    }
}
