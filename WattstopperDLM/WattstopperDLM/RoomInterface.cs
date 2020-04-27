using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace WattstopperDLM
{
  public class RoomInterface
  {
    internal Dictionary<Load, LevelChangeEvent> Loads = new Dictionary<Load, LevelChangeEvent>();

    public delegate void IsCommunicating(ushort state);
    public delegate void SystemVersion(SimplSharpString version);
    public delegate void SendingData(SimplSharpString data);
    public delegate void SceneChange(ushort sceneId);
    public delegate void OccupancyChange(ushort occupancy);
    public IsCommunicating onIsCommunicating { get; set; }
    public SystemVersion onVersion { get; set; }
    public SendingData onSendingData { get; set; }
    public SceneChange onSceneChange { get; set; }
    public OccupancyChange onOccupancyChange { get; set; }

    public string ID { get { return _id; } set { _id = value; } }
    public ushort Debug { set { _debug = Convert.ToBoolean(value); } }

    private string _id;
    private bool _debug;
    private bool _isCommunicating;
    public bool _isOccupied;
    private int _currentScene;
    private CTimer _waitForConnection;
    private CTimer _receivedDataDequeue;
    private CrestronQueue<string> _receivedData;

    internal bool RegisterLoad(Load load)
    {
      try
      {
        lock (Loads)
        {
          if (!Loads.ContainsKey(load))
          {
            Loads.Add(load, new LevelChangeEvent());

            if (Processor.RoomInterfaces.ContainsKey(_id))
            {
                load.Initialize();
            }

            if (_debug)
            {
              CrestronConsole.PrintLine("Registered Load ID {0}", load.ID);
            }

            return true;
          }
          else
          {
            CrestronConsole.PrintLine("Already registered Load ID {0}", load.ID);

            return false;
          }
        }
      }
      catch (Exception e)
      {
        if (_debug)
        {
          ErrorLog.Exception("Exception occured in WattstopperDLM.Processor.RegisterLoad", e);
        }
        return false;
      }
    }

    internal void SendData(string data)
    {
      if (onSendingData != null)
      {
        onSendingData(data);
      }
    }

    public void Initialize()
    {
        Processor.AddRoomInterface(this);
        _receivedData = new CrestronQueue<string>();
        _receivedDataDequeue = new CTimer(DataReceivedDequeue, null, 0, 10);
        _waitForConnection = new CTimer(WaitForConnection, null, 0, 5000);
    }

    public void RecallScene(ushort scene)
    {
      SendData(string.Format("SCENE {0}", scene));
    }

    public void DataReceived(string data)
    {
      try
      {
        _receivedData.Enqueue(data);
      }
      catch (Exception e)
      {
        if (_debug)
        {
          ErrorLog.Exception("Exception occured in WattstopperDLM.Processor.DataReceived", e);
        }
      }
    }

    private void WaitForConnection(object o)
    {
      SendData("VERSION ");
    }

    private void DataReceivedDequeue(object o)
    {
      try
      {
        if (!_receivedData.IsEmpty)
        {
          ParseDataReceived(_receivedData.Dequeue());
        }
      }
      catch (Exception e)
      {
        if (_debug)
        {
          ErrorLog.Exception("Exception occured in WattstopperDLM.Processor.DataReceivedDequeue", e);
        }
      }
    }

    private void ParseDataReceived(string data)
    {
      try
      {
        if (_waitForConnection != null && !_isCommunicating)
        {
          _waitForConnection.Stop();
          _waitForConnection.Dispose();
          _waitForConnection = null;

          _isCommunicating = true;

          if (onIsCommunicating != null)
          {
            onIsCommunicating(Convert.ToUInt16(_isCommunicating));
          }

          if (data.Contains("VERSION"))
          {
            var response = data.Split(':');
            var ver = response[1].Split(' ');

            if (onVersion != null)
            {
              onVersion(ver[ver.Length - 1]);
            }
          }

          SendData("STATUS LOAD");
          SendData("STATUS SCENE");
          SendData("STATUS OCCUPANCY");
        }

        if (_isCommunicating)
        {
          var response = data.Split(':');

          if (response[0] == "S")
          {
            if (response[1] == "ONLINE")
            {
              SendData("STATUS LOAD");
              SendData("STATUS SCENE");
            }
            else if (response[1].Contains("SCENE"))
            {
              var scene = response[1].Split(' ');

              _currentScene = Convert.ToInt16(scene[1]);

              if (onSceneChange != null)
              {
                onSceneChange(Convert.ToUInt16(_currentScene));
              }
            }
            else if (response[1].Contains("LOAD"))
            {
              var newLoad = response[1].Split(' ');
              var newLoadId = newLoad[1];
              var newLoadLevel = Convert.ToInt16(newLoad[2]);

              foreach (var load in Loads)
              {
                if (load.Key.ID == newLoadId)
                {
                  load.Value.SendData(new LevelChangeEventArgs(newLoadLevel));
                }
              }
            }
            else if (response[1].Contains("OCCUPANCY"))
            {
              var occupancy = response[1].Split(' ');

              if (occupancy[1] == "OCCUPIED")
              {
                _isOccupied = true;
              }
              else
              {
                _isOccupied = false;
              }

              if (onOccupancyChange != null)
              {
                onOccupancyChange(Convert.ToUInt16(_isOccupied));
              }
            }
          }
        }
      }
      catch (Exception e)
      {
        if (_debug)
        {
          ErrorLog.Exception("Exception occured in WattstopperDLM.Processor.DataReceived", e);
        }
      }
    }
  }
}