using UnityEngine;
using System.Collections;

public class CubeRun : MonoBehaviour {


    private Vector3 NewPostion { get; set; }

    // Use this for initialization
    void Start () {
	
        if(!NetClient.IsConnect)
        {
            NetClient.Connect();
        }

        NetClient.CallBack.SetPostionEvent += CallBack_SetAngleEvent;

	}

    private void CallBack_SetAngleEvent(float x,float y,float z)
    {
        NewPostion=new Vector3(x, y, z);
    }

    // Update is called once per frame
    void Update()
    {
        var server = NetClient.GetServer();

        if (server != null)
            server.UpdateCurrentRotation(transform.rotation.x, transform.rotation.y, transform.rotation.z);

        transform.Rotate(Vector3.up, 60 * Time.deltaTime);
        transform.position = NewPostion;
    }
}
