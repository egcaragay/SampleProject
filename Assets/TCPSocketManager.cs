using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Threading;
using  System.Net.NetworkInformation;


/**
 * This class wraps a C# UdpClient, creates two threads for send & receive
 * and provides methods for sending, receiving data and closing threads.
 */

public class TcpSocketManager {

    private readonly object _sendQueueLock = new object();
    private readonly Queue<byte[]> _sendQueue = new Queue<byte[]>();

    private readonly object _receiveQueueLock = new object();
    private readonly Queue<byte[]> _receiveQueue = new Queue<byte[]>();

    private Thread _receiveThread;
    private Thread _sendThread;
    private Thread _ping;

    public TcpClient _tcpClient;
    
    private volatile bool _shouldRun = true;
    
    private readonly string _serverIp;
    private readonly int _serverPort;

    private NetworkStream stream;

    public bool IsDisconnected = false;

    private long pingSend;
    private long pingReceived;
    
    public int GetPing()
    { 
        return  (int)(pingReceived/20000f);
    }
    
    public TcpSocketManager(string _serverIp,int _serverPort) {

        this._serverIp = _serverIp;
        this._serverPort = _serverPort;
        IsDisconnected = false;
        
        _sendThread?.Abort();
        _receiveThread?.Abort();
        _ping?.Abort();
    }
    

    /**
     * Resets SocketManager state to default and starts Send & Receive threads
     */
    public IEnumerator initSocket(Action complete) {

        // check whether send & receive threads are alive, if so close them first
        if ((_sendThread != null && _sendThread.IsAlive) || (_receiveThread != null && _receiveThread.IsAlive) || (_ping != null && _ping.IsAlive)) {
            Disconnect();
            while ((_sendThread != null && _sendThread.IsAlive) || (_receiveThread != null && _receiveThread.IsAlive) || (_ping != null && _ping.IsAlive)) {
                yield return null;
                // wait until tcp threads closed
            }
        }

        // reset SocketManager state
        _sendQueue.Clear();
        _receiveQueue.Clear();
        try
        {
            _tcpClient = new TcpClient();
            _tcpClient.NoDelay = true;

            _tcpClient.Connect(_serverIp, _serverPort);

            stream = _tcpClient.GetStream();
            
            _shouldRun = true;

            // start Send & receive threads
            _receiveThread = new Thread(
                new ThreadStart(ReceiveThread));
            _receiveThread.IsBackground = true;
            _receiveThread.Start();

            _sendThread = new Thread(
                new ThreadStart(SendThread));
            _sendThread.IsBackground = true;
            _sendThread.Start();

            _ping = new Thread(new ThreadStart(Ping));
            _ping.IsBackground = true;
            _ping.Start();



            complete();

        }
        catch(Exception e)
        {
            Disconnect();
        }
        
    }


    /**
     * Adds an array of bytes to Queue for sending to server
     */
    public void Send(byte[] data) {
        
        lock (_sendQueueLock)
        {
            _sendQueue.Enqueue(data);
        }
        
    }

    /**
     * Reads received byte arrays from queue and return them as a list
     */
    public IList<byte[]> Receive() {

        IList<byte[]> res = new List<byte[]>();
        lock (_receiveQueueLock) {
            while (_receiveQueue.Count > 0)
            {
                var item = _receiveQueue.Dequeue();
                res.Add(item);
            }
        }
        return res;
    }

    private void Ping()
    {
        var pingByte = new byte[] { 254 };
        while (_shouldRun)
        {
            Thread.Sleep(3000);
            pingSend = DateTime.Now.Ticks;
            Send(pingByte);
        }

    }

    private void SendThread() {
        while (_shouldRun) {
            byte[] item = null;
            do {
                item = null;
                lock (_sendQueueLock) {
                    if (_sendQueue.Count > 0)
                        item = _sendQueue.Dequeue();
                }

                if (item != null) {
                    try
                    {
                        if (stream.CanWrite)
                        {
                            // Write byte array to socketConnection stream.                 
                            stream.Write(item, 0, item.Length);
                        }
                    }
                    catch (Exception socketException)
                    {
                        Disconnect();
                    }
                }
            }
            while (item != null); // loop until there are items to collect
        }
    }
    
    // i putted UdpClient creation in a seperate thread because im not sure if Bind() method is non-blocking
    // and if Bind() is Blocking, it could block Unity's thread
    private void ReceiveThread() {

        byte[] bytes = new byte[_tcpClient.ReceiveBufferSize];
        byte[] previousBytes = new byte[0];
        while (_shouldRun)
        {
            int length = 0;
            try
            {
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    var data = new byte[length+previousBytes.Length];
                    Array.Copy(previousBytes, data, previousBytes.Length);
                    Array.Copy(bytes, 0, data, previousBytes.Length, length);
                    _receiveQueue.Enqueue(data);
                }
                Disconnect();
            }

            catch (Exception socketException)
            {
                Disconnect();
                break;
            }
        }
    }
    

    public void Disconnect()
    {
        _shouldRun = false;
        _tcpClient?.Close();
        _tcpClient = null;
        IsDisconnected = true;
    }
}
