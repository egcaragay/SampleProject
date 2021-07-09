using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MultiplayerListener : MonoBehaviour
{
    public static MultiplayerListener Instance;
    
    public GameObject toHide;
    private TcpSocketManager client;
    // Start is called before the first frame update

    public int id;
    public Player player;
    private Dictionary<byte, Action<DataStreamReader>> listener = new Dictionary<byte, Action<DataStreamReader>>();

    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private void Connected(DataStreamReader dataStreamReader)
    {
        var player = Instantiate(this.player);
        player.id = id;
        players.Add(id,player);
    }

    private void Joined(DataStreamReader dataStreamReader)
    {
        var id = dataStreamReader.ReadInt();
        var player = Instantiate(this.player);
        player.id = id;
        players.Add(id,player);
    }
    
    private void Run(DataStreamReader dataStreamReader)
    {
        var id = dataStreamReader.ReadInt();
        var x = dataStreamReader.ReadFloat();
        var y = dataStreamReader.ReadFloat();
        var z = dataStreamReader.ReadFloat();
        players[id].SetDestination(new Vector3(x,y,z));

    }

    public  void Join()
    {
        client = new TcpSocketManager("127.0.0.1",9999);
        StartCoroutine(client.initSocket(() =>
        {
            var writer = new DataStreamWriter();
            writer.WriteByte(0);
            id = Random.Range(0, int.MaxValue);
            Debug.Log(id);
            writer.WriteInt(id);
            Send(writer);
        }));
    }
    
    void Start()
    {
        Instance = this;
        listener.Add(0,Connected);
        listener.Add(1,Joined);
        listener.Add(2,Run);
    }

    public void Send(DataStreamWriter dataStreamWriter)
    {
        client?.Send(dataStreamWriter.buffer.ToArray());
    }
    
    // Update is called once per frame
    void Update()
    {
        if (client != null && !client.IsDisconnected)
        {
            toHide.SetActive(false);

            var receive = client.Receive();
            
            foreach (var bytes in receive)
            {
                DataStreamReader dsr = new DataStreamReader(bytes);
                var messageType = dsr.ReadByte();
                if (listener.ContainsKey(messageType))
                {
                    listener[messageType](dsr);
                }
            }
            
            return;
        }

        foreach (var kvp in players)
        {
            Destroy(kvp.Value.gameObject);
        }
        
        players.Clear();
        toHide.SetActive(true);
    }
}
