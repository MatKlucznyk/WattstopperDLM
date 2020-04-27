using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace WattstopperDLM
{
    public class RoomInterfaceAddedEventArgs : EventArgs
    {
        public string ProcID;

        public RoomInterfaceAddedEventArgs(string procId)
        {
            this.ProcID = procId;
        }
    }
}