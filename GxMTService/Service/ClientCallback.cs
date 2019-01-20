using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GxMTService
{
    public class ClientCallback : IClientCallback
    {
        public void Message(string Results)
        {
            Console.WriteLine(Results);
        }
    }
}
