using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phothoflow.location
{
    class Corner : IComparable<Corner>, IEquatable<Corner>
    {
        public float x;
        public float y;

        public bool valid;

        public Corner()
        {
            valid = true;
        }

        public Corner(float x_, float y_)
        {
            x = x_;
            y = y_;
            valid = true;
        }

        public Corner(Item item)
        {
            x = item.Left;
            y = item.Top;
        }

        public int CompareTo(Corner other)
        {
            return (int)((y * 9999 + x)-(other.y * 9999 + other.x));
        }

        public bool Equals(Corner other)
        {
            return y == other.y && x == other.x;
        }
    }
}
