using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phothoflow.manual
{
    public interface Command
    {
        void execute();
        void undo();
    }
}
