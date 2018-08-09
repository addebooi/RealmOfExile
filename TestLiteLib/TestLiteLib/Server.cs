using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using CommunicationContract;
using UnityEngine;

namespace TestLiteLib
{
    class Server : INetEventListener, ICommuncationContract
    {
        private const string _connectionKey = "ConnectionString";
        private const int _port = 9050;
        private const int _maxConnections = 32;
        private EventBasedNetListener _netListener;
        private NetManager _serverManager;
        private Game _game;
        private NetHandler _netHandler;
        public static List<string> _temporaryMessages;

        private const bool EnableStatistics = true;
        private const bool EnableTemporaryMessages = false;
        private Stopwatch watch;
        private static Stopwatch functionalityWatch;
        private float _deltaTime;
        private float FramesThisSecond;
        private float SecondCounter;
        private float totalTime;
        private float _averageDeltaTimeThisSecond;

        private static List<Tuple<double, string>> times;
        private static Dictionary<string, double> times_v2;
        private bool reprintTimes_v2;

        

        public Server()
        {
            _netListener = new EventBasedNetListener();
            _serverManager = new NetManager(_netListener, _maxConnections, _connectionKey);
            _netHandler = new NetHandler();
            _game = new Game(_netHandler);
            _temporaryMessages = new List<string>();
            watch = new Stopwatch();
            functionalityWatch = new Stopwatch();
            times = new List<Tuple<double, string>>(); 
            times_v2 = new Dictionary<string, double>();
            SecondCounter = 0;
            FramesThisSecond = 0;
            totalTime = 0;
            _averageDeltaTimeThisSecond = 15;
            _netHandler.tempServer = this;
            RegisterListener();
        }

        public void Start()
        {
            _serverManager.Start(_port);
            watch.Start();
            ServerLoop();
        }

        public void ServerLoop()
        {
            while (!Console.KeyAvailable)
            {
                Statistics();
                _serverManager.PollEvents();
                _game.Update(getDeltaTime());
                Thread.Sleep(15);
            }

            _serverManager.Stop();
        }

        private float getDeltaTime()
        {;
            _deltaTime =  (float) (watch.Elapsed.TotalMilliseconds / 1000f);
            totalTime += _deltaTime;
            SecondCounter += _deltaTime;
            FramesThisSecond++;
           
            watch.Restart();

            if (SecondCounter > 1)
            {
                _averageDeltaTimeThisSecond = SecondCounter / FramesThisSecond;

                if(EnableStatistics)
                    WriteTimesFunctionality(FramesThisSecond);
                FramesThisSecond = 0;
                SecondCounter = 0;
            }

            return _deltaTime;
        }



        public void OnPeerConnected(NetPeer peer)
        {
            _game.OnPlayerConnected(peer);
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _game.OnPlayerDisconnected(peer);
        }

        public void OnNetworkError(NetEndPoint endPoint, int socketErrorCode)
        {

        }

        public void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            CommunecationContract.CreateEvent(this, peer, reader);
            
        }

        public void OnNetworkReceiveUnconnected(NetEndPoint remoteEndPoint, NetDataReader reader, UnconnectedMessageType messageType)
        {

        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {

        }

        private void RegisterListener()
        {
            _netListener.PeerConnectedEvent += OnPeerConnected;
            _netListener.PeerDisconnectedEvent += OnPeerDisconnected;
            _netListener.NetworkReceiveEvent += OnNetworkReceive;
            _netListener.NetworkLatencyUpdateEvent += OnNetworkLatencyUpdate;
            _netListener.NetworkErrorEvent += OnNetworkError;
            _netListener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;
        }

        public void OnClickToPosition(ClickToPosition clickToPositionObject)
        {
            _game.UpdateClicktoPosition(clickToPositionObject);
        }

        public void OnSpawnData(SpawnData spawnDataObject)
        {

        }

        public void OnUpdatePosition(UpdatePositionData updatePositionData)
        {
            
        }

        public void OnUpdatePositionInterpolate(UpdatePositionInterpolateData updatePositionInterpolateData)
        {
            
        }

        public void OnDeleteObject(DeleteObjectData deleteObjectData)
        {
            
        }

        public void OnSpawnPlayer(SpawnPlayerData spawnPlayerData)
        {
            
        }

        public void OnSpawnFireballData(SpawnFireballData spawnFireBallData)
        {
            throw new NotImplementedException();
        }

        public void OnClientSpawnFireballData(ClientSpawnFireballData clientSpawnFireballData)
        {
            _game.OnFireballSpawn(clientSpawnFireballData);
        }

        public void OnVariableUpdate(UpdateVariableData updateVariableData)
        {

        }

        public void OnClientCastAbility(ClientCastAbilityData clientCastAbilityData)
        {
            _game.OnClientCastAbility(clientCastAbilityData);
        }

        public static void StopFunctionalityTime()
        {
            if (!EnableStatistics) return;
            var timeElapsed = functionalityWatch.Elapsed.TotalMilliseconds * 1000;
            functionalityWatch.Reset();
            functionalityWatch.Stop();
            if (!times_v2.ContainsKey(LastFunctionality))
                times_v2.Add(LastFunctionality, timeElapsed);
            else
                times_v2[LastFunctionality] += timeElapsed;
//            if (times.Any())
//            {
//                times[times.Count-1] = new Tuple<double, string>(timeElapsed, times.Last().Item2);
//;
//            }
            
        }

        private static string LastFunctionality = "";
        public static void StartFunctionalityTime(string functionality)
        {
            if (!EnableStatistics) return;
            Server.functionalityWatch.Start();
            LastFunctionality = functionality;

            //times.Add(new Tuple<double, string>(0, functionality));
        }

        public static void WriteTimesFunctionality(float totalFrames)
        {
            int cPos = Console.CursorTop;

            for (int i = 0; i < times_v2.Count; i++)
            {
                Console.SetCursorPosition(0, cPos + i);
                Console.Write("                                          ");
            }

            Console.SetCursorPosition(0, cPos);
            foreach (var c in times_v2)
            {
                Console.WriteLine($"{c.Key} : {(c.Value/ totalFrames).ToString("F")}");


            }
            times_v2.Clear();
        }

        private void Statistics()
        {
            if (!EnableStatistics) return;
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("Connected Players: {0}\n" +
                                  "#########################\n" +
                                  "Bytes Received:    {1}\n" +
                                  "Bytes Sent:        {2}\n" +
                                  "#########################\n" +
                                  "Packets Received:  {3}\n" +
                                  "Packets Sent:      {4}\n" +
                                  "Packet Loss:       {5}\n" +
                                  "#########################\n" +
                                  "Up Time (s):       {6}\n" +
                                  "LoopTime (ms):     {7}\n",
                _serverManager.PeersCount,
                _serverManager.Statistics.BytesReceived,
                _serverManager.Statistics.BytesSent,
                _serverManager.Statistics.PacketsReceived,
                _serverManager.Statistics.PacketsSent,
                _serverManager.Statistics.PacketLoss,
                totalTime.ToString("##.###"),
                (_averageDeltaTimeThisSecond*1000).ToString("##.###"));

            if(EnableTemporaryMessages)
            foreach (var msg in _temporaryMessages)
            {
                Console.WriteLine(msg+"\n");
            }
            
        }
    }
}
