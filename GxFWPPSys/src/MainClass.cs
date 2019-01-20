using System;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace GxFWPPSys.src
{
    public class MainClass
    {
        private MTBridge MtBridge;

        public MainClass()
        {
            MtBridge = new MTBridge();
        }


        public void Start()
        {
            MtBridge.StartHost();
        }
    }
}
