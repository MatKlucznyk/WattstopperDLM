using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace WattstopperDLM
{
    internal class LevelChangeEventArgs : EventArgs
    {
        internal int Level;

        internal LevelChangeEventArgs(int level)
        {
            this.Level = level;
        }
    }
}