using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phothoflow.location
{
    interface StepCallback
    {
        void OnStart();

        void OnStep(Item item);

        void OnFinish();
    }
}
