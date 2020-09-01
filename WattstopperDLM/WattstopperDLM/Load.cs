using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace WattstopperDLM
{
    public class Load : IEquatable<Load>
    {
        private string _id;
        private string _procId;
        private int _level;
        private bool _isRegistered;

        public delegate void IsRegistered(ushort state);
        public delegate void LevelChange(ushort level);
        public IsRegistered onIsRegistered { get; set; }
        public LevelChange onLevelChange { get; set; }

        public string ID { get { return _id; } set { _id = value; } }
        public string ProcID { get { return _procId; } set { _procId = value; } }

        public Load()
        {
            Processor.RoomInterfaceAdded += new EventHandler<RoomInterfaceAddedEventArgs>(Processor_RoomInterfaceAdded);
        }

        void Processor_RoomInterfaceAdded(object sender, RoomInterfaceAddedEventArgs e)
        {
            if (e.ProcID == this._procId)
            {
                Initialize();
            }
        }

        public bool Equals(Load other)
        {
            return this._id == other._id && this._procId == other._procId;
        }

        internal void Initialize()
        {
            if (Processor.RegisterLoad(_procId, this))
            {
                Processor.RoomInterfaces[_procId].Loads[this].OnNewEvent += new EventHandler<LevelChangeEventArgs>(Load_OnNewEvent);

                _isRegistered = true;
            }
            else
            {
                _isRegistered = false;
            }

            if (onIsRegistered != null)
            {
                onIsRegistered(Convert.ToUInt16(_isRegistered));
            }
        }

        public void SetLevel(ushort level)
        {
            var newLevel = Convert.ToInt32(Math.Round(ScaleDown(Convert.ToDouble(level))));

            Processor.SendData(_procId, string.Format("LOAD {0} {1}", _id, newLevel));
        }

        void Load_OnNewEvent(object sender, LevelChangeEventArgs e)
        {
            _level = Convert.ToInt32(Math.Round(ScaleUp(Convert.ToDouble(e.Level))));

            if (onLevelChange != null)
            {
                onLevelChange(Convert.ToUInt16(_level));
            }
        }

        internal double ScaleUp(double level)
        {
            double scaleLevel = level;
            double levelScaled = (scaleLevel * (65535.0 / 100.0));
            return levelScaled;
        }

        internal double ScaleDown(double level)
        {
            double scaleLevel = level;
            double levelScaled = (scaleLevel / (65535.0 / 100.0));
            return levelScaled;
        }
    }
}