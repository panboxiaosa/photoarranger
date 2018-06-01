using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using phothoflow.filemanager;

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

        public List<Item> GetWorkSpaceItems()
        {
            return allItemList;
        }

        public List<Item> GetUnarrangedItems()
        {
            List<Item> result = new List<Item>(); 
            foreach (Item item in allItemList)
            {
                if (!currentArrange.Contains(item))
                {
                    result.Add(item);
                }
            }
            return result;
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

        public void Load(string path)
        {
            allItemList = new List<Item>();
            currentArrange = new List<Item>();
            
            List<String> images = ImageList.listDirectory(path);
            foreach (String pathStr in images) 
            {
                Item item = new Item(pathStr);
                allItemList.Add(item);
                AutoArrange(item);
            }
            callback.OnFinish();
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
        

        public void Update()
        {
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
                callback.OnStep(item);
            }
            
        }

        public void ManualArrange(Item item, int x, int y)
        {
            int position = calcer.FindSuitable(currentArrange, item, x, y);
            currentArrange.Insert(position, item);
        }

    }
}
