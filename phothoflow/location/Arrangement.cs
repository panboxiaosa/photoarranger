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
        private List<Item> currentArrange;
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

            int seg = 1; // Environment.ProcessorCount;
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

        public bool AdjustItem(Item adjust, float x, float y)
        {
            if (adjust.Width + x > SettingManager.GetWidth()) return false;
            if (y < 0 || x < 0) return false;
            List<Item> prepare = new List<Item>();
            List<Item> abondon = new List<Item>();
            adjust.Top = y;
            adjust.Left = x;
            foreach (Item inpos in currentArrange)
            {
                if (inpos == adjust) continue;
                if (adjust.IsOverlap(inpos))
                {
                    abondon.Add(inpos);
                }
                else
                {
                    prepare.Add(inpos);
                }
            }
            int postion = calcer.FindSuitable(prepare, adjust, x, y);
            if (postion != -1)
            {
                prepare.Insert(postion, adjust);
            }

            abondon.AddRange(prepare.GetRange(postion + 1, prepare.Count - postion -1));
            prepare.RemoveRange(postion + 1, prepare.Count - postion -1);

            ReArrange(prepare, abondon);
            return true;
        }

        private void ReArrange(List<Item> already, List<Item> rest)
        {
            callback.OnStart();
            foreach (Item item in rest)
            {
                int position = calcer.FindSuitable(already, item);
                if (position != -1)
                {
                    already.Insert(position, item);
                }
            }
            currentArrange = already;

            callback.OnFinish();
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
                int position = calcer.FindSuitable(currentArrange, item);
                if (position != -1)
                {
                    currentArrange.Insert(position, item);
                }
            }
            callback.OnFinish();
        }

        public void Recession(Item item)
        {
            currentArrange.Remove(item);
        }

    }
}
