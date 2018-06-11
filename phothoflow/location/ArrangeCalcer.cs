using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using phothoflow.setting;

namespace phothoflow.location
{
    class ArrangeCalcer
    {

        bool NotRecommend(List<Item> already, Corner c)
        {
            foreach (Item item in already)
            {
                if (c.x == item.Left && c.y == item.Top)
                    return true;
                if (c.x == item.Left + item.Width && c.y == item.Top + item.Height)
                    return true;
            }
            return false;
        }

        List<Corner> GetAllPosible(List<Item> origin)
        {
            HashSet<float> xs = new HashSet<float>();
            HashSet<float> ys = new HashSet<float>();
            foreach (Item one in origin)
            {
                xs.Add(one.Left);
                xs.Add(one.Left + one.Width);
                ys.Add(one.Top);
                ys.Add(one.Top + one.Height);
            }

            List<Corner> all = new List<Corner>();
            foreach (float y in ys)
            {
                foreach (float x in xs)
                {
                    Corner created = new Corner(x, y);
                    if (!all.Contains(created) && !NotRecommend(origin, created))
                        all.Add(created);

                }
            }

            all.Sort();

            return all;
        }

        bool Fit(Item comer, Corner attach, List<Item> already)
        {

            comer.Left = attach.x;
            comer.Top = attach.y;

            float totalWidth = SettingManager.GetWidth();
            if (attach.x + comer.Width > totalWidth)
            {
                return false;
            }

            foreach (Item comp in already)
            {
                if (comer.IsOverlap(comp))
                    return false;
            }
            return true;
        }

        int GetOrder(Item inc, List<Item> already)
        {
            Corner wander = new Corner(inc);
            for (int i = 0; i < already.Count; i++)
            {
                Corner target = new Corner(already[i]);
                if (wander.CompareTo(target) < 0)
                {
                    return i;
                }
            }
            return already.Count;
        }

        public int FindSuitable(List<Item> origin, Item inc)
        {
            float width = SettingManager.GetWidth();
            if (origin.Count == 0)
            {
                if (inc.Width <= width)
                {
                    inc.Top = 0;
                    inc.Left = 0;
                    return 0;
                }
            }
            else
            {
                List<Corner> possible = GetAllPosible(origin);
                foreach (Corner taste in possible)
                {
                    if (Fit(inc, taste, origin))
                    {
                        inc.Top = taste.y;
                        inc.Left = taste.x;
                        return GetOrder(inc, origin);
                    }
                }
            }
            return -1;
        }

        public int FindSuitable(List<Item> origin, Item inc, float x, float y)
        {
            float width = SettingManager.GetWidth();
            if (origin.Count == 0)
            {
                if (inc.Width <= width)
                {
                    inc.Top = 0;
                    inc.Left = 0;
                    return 0;
                }
            }
            else
            {
                List<Corner> possible = GetAllPosible(origin);

                foreach (Corner taste in possible)
                {
                    if (taste.x <= x && taste.y <= y && Fit(inc, taste, origin))
                    {
                        inc.Top = taste.y;
                        inc.Left = taste.x;
                        return GetOrder(inc, origin);
                    }
                }
            }
            return -1;
        }

    }
}
