using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GxMTService
{
    public interface IClientCallback
    {
        //Your callback method
        void Message(string name);
    }
}
