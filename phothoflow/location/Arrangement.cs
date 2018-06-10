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
using System.Diagnostics;

namespace phothoflow.location
{
    class Arrangement
    {

        ArrangeCalcer calcer = new ArrangeCalcer();
        private List<Item> currentArrange;
        private List<Item> allItemList;

        ArrangeCallback callback;
        
        public Arrangement(ArrangeCallback arrangeCallback)
        {
            callback = arrangeCallback;
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

        public void Arrange()
        {
            if (allItemList == null)
            {
                return;
            }
            callback.OnArrangeStart();
            currentArrange = new List<Item>();
            foreach (Item item in allItemList)
            {
                int position = calcer.FindSuitable(currentArrange, item);
                if (position != -1)
                {
                    currentArrange.Insert(position, item);
                }
            }
            callback.OnArrangeFinish();
        }

        public void AddElement(Item item)
        {
            if (allItemList == null)
            {
                allItemList = new List<Item>();
            }
            allItemList.Add(item);
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
            callback.OnArrangeStart();
            foreach (Item item in rest)
            {
                int position = calcer.FindSuitable(already, item);
                if (position != -1)
                {
                    already.Insert(position, item);
                }
            }
            currentArrange = already;

            callback.OnArrangeFinish();
        }
    }
}
