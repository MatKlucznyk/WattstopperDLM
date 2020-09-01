using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes

namespace WattstopperDLM
{
    public static class Processor
    {
        internal static Dictionary<string, RoomInterface> RoomInterfaces = new Dictionary<string, RoomInterface>();

        internal static event EventHandler<RoomInterfaceAddedEventArgs> RoomInterfaceAdded;

        internal static void AddRoomInterface(RoomInterface roomInterface)
        {
            try
            {
                lock (RoomInterfaces)
                {
                    if (!RoomInterfaces.ContainsKey(roomInterface.ID))
                    {
                        RoomInterfaces.Add(roomInterface.ID, roomInterface);

                        OnRoomInterfaceAdded(new RoomInterfaceAddedEventArgs(roomInterface.ID));
                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        private static void OnRoomInterfaceAdded(RoomInterfaceAddedEventArgs e)
        {
            EventHandler<RoomInterfaceAddedEventArgs> handler = RoomInterfaceAdded;

            if (handler != null)
            {
                handler(null, e);
            }
        }

        internal static bool RegisterLoad(string procId, Load load)
        {
            try
            {
                if (RoomInterfaces[procId] != null)
                {
                    return RoomInterfaces[procId].RegisterLoad(load);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        internal static void SendData(string procId, string data)
        {
            try
            {
                if (RoomInterfaces[procId] != null)
                {
                    RoomInterfaces[procId].SendData(data);
                }
            }
            catch (Exception e)
            {
            }
        }
    }
}
