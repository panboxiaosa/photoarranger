using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phothoflow.location
{
    public interface StepCallback
    {
        void OnStart();

        void OnStep(Item item);

        void OnFinish();
    }
}
