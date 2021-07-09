using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player : MonoBehaviour
{
    public int id;

    public Vector3 destination;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, 3.5f * Time.deltaTime);
        if (Input.GetMouseButtonDown(0) && MultiplayerListener.Instance.id == id) {
            RaycastHit hit;
                
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100)) {
                DataStreamWriter dswd = new DataStreamWriter();
                dswd.WriteByte(2);
                dswd.WriteFloat(hit.point.x);
                dswd.WriteFloat(hit.point.y);
                dswd.WriteFloat(hit.point.z);
                MultiplayerListener.Instance.Send(dswd);
            }
        }

    }

    public void SetDestination(Vector3 destination)
    {
        this.destination = destination;
    }
}
