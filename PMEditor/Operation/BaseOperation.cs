using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PMEditor.Operation
{
    public abstract class BaseOperation
    {
        public abstract void ReDo();

        public abstract void Undo();
    }
}
