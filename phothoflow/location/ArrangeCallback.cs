using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace phothoflow.location
{
    public interface ArrangeCallback
    {
        void OnArrangeStart();

        void OnArrangeFinish();
    }
}
