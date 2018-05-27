using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace phothoflow.manual
{
    class CommandManager
    {
        List<Command> undoList;
        List<Command> redoList;

        public void executeCommand(Command cmd)
        {
            cmd.execute();
            undoList.Add(cmd);
            redoList.Clear();
        }

        public void undo()
        {
            if (undoList.Count == 0)
            {
                return;
            }
            Command cmd = undoList.Last();
            cmd.undo();
            undoList.Remove(cmd);
            redoList.Add(cmd);
        }

        public void redo()
        {
            if (redoList.Count == 0)
            {
                return;
            }
            Command cmd = redoList.Last();
            cmd.execute();
            redoList.Remove(cmd);
            undoList.Add(cmd);
        }
    }
}
