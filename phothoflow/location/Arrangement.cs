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


        public void SortBySize()
        {
            if (allItemList != null)
            {
                allItemList.Sort();
            }
        }

        public void checkRotate()
        {
            if (allItemList == null)
                return;
            foreach (Item item in allItemList) {
                if (item.Width > SettingManager.GetWidth() && item.Height < SettingManager.GetWidth())
                {
                    item.RotateImg();
                } else if (SettingManager.GetWidth() - item.Height > 0 && SettingManager.GetWidth() - item.Height < SettingManager.GetWidth() / 8) {
                    item.RotateImg();
                }
            }
            
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
                if (currentArrange.Contains(item)) continue;
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

        public bool ArrangeElement(Item item)
        {
            int position = calcer.FindSuitable(currentArrange, item);
            if (position != -1)
            {
                currentArrange.Insert(position, item);
                callback.OnArrangeStart();
                callback.OnArrangeFinish();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DrawBackItem(Item item)
        {
            callback.OnDrawBack(item);
            callback.OnArrangeStart();
            int pos = currentArrange.IndexOf(item);
            currentArrange.Remove(item);
            List<Item> invali = currentArrange.GetRange(pos, currentArrange.Count - pos);
            currentArrange.RemoveRange(pos, currentArrange.Count - pos);
            PartChange(invali);
            callback.OnArrangeFinish();
        }

        int OverRiding(Item adjust, float x, float y) {
            for (int i = 0; i < currentArrange.Count; i++ )
            {
                Item cur = currentArrange[i];
                if (cur == adjust) continue;
                if (cur.Contains(x, y))
                    return i;
            }
            return -1;
        }

        public void AdjustItem(Item adjust, float x, float y)
        {
            callback.OnArrangeStart();
            if (!(adjust.Width + x > SettingManager.GetWidth() || y < 0 || x < 0))
            {

                float originX = adjust.Left;
                float originY = adjust.Top;
                int place = currentArrange.IndexOf(adjust);

                int thinkbad = OverRiding(adjust, x, y);
                if (thinkbad != -1)
                {
                    Item bad = currentArrange[thinkbad];

                    adjust.Top = bad.Top;
                    adjust.Left = bad.Left;

                    currentArrange.Remove(adjust);
                    int badplace = currentArrange.IndexOf(bad);
                    currentArrange.Insert(badplace, adjust);

                    List<Item> abondon = new List<Item>();
                    for (int i = badplace - 1; i >= 0; i--)
                    {
                        Item maybeMove = currentArrange[i];
                        if (adjust.IsOverlap(maybeMove))
                        {
                            currentArrange.Remove(maybeMove);
                            abondon.Insert(0, maybeMove);
                        }
                    }
                    int stablePlace = currentArrange.IndexOf(adjust);
                    abondon.AddRange(currentArrange.GetRange(stablePlace + 1, currentArrange.Count - stablePlace - 1));
                    currentArrange.RemoveRange(stablePlace + 1, currentArrange.Count - stablePlace - 1);
                    PartChange(abondon);
                }
                else
                {
                    currentArrange.Remove(adjust);

                    int postion = calcer.FindSuitable(currentArrange, adjust, x, y);
                    if (postion != -1)
                    {
                        currentArrange.Insert(postion, adjust);
                    }
                    else
                    {
                        adjust.Top = originY;
                        adjust.Left = originX;
                        currentArrange.Insert(place, adjust);
                    }

                }
            }
            callback.OnArrangeFinish();

        }

        void PartChange(List<Item> rest)
        {
            foreach (Item item in rest)
            {
                int position = calcer.FindSuitable(currentArrange, item);
                if (position != -1)
                {
                    currentArrange.Insert(position, item);
                }
            }
        }

    }
}
