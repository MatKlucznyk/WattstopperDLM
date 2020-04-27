using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace WattstopperDLM
{
    internal class LevelChangeEvent
    {
        private event EventHandler<LevelChangeEventArgs> onNewEvent = delegate { };

        public event EventHandler<LevelChangeEventArgs> OnNewEvent
        {
            add
            {
                if (!onNewEvent.GetInvocationList().Contains(value))
                {
                    onNewEvent += value;
                }
            }
            remove
            {
                if (onNewEvent.GetInvocationList().Contains(value))
                {
                    onNewEvent -= value;
                }
            }
        }

        internal void SendData(LevelChangeEventArgs e)
        {
            onNewEvent(null, e);
        }
    }
}