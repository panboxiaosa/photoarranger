using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phothoflow.location
{
    public interface LoadCallback
    {
        void OnLoadStart();

        void OnLoadStep(Item item);

        void OnLoadFinish();
    }
}
