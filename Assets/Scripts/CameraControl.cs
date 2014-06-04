using UnityEngine;

/**<summary>Klasa oblsugujaca kontrole kamery</summary>*/
public class CameraControl : MonoBehaviour
{
    /**<summary>Predkosc przemieszczanai sie kamery</summary>*/
    public float velocity;
    /**<summary>Predkosc katowa (rad/s) kamery</summary>*/
    public float angularVelocity; //predkosc katowa kamery (w radianach na sekunde)

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /** <summary>Funkcja wywolywana podczas kazdej klatki.</summary> */
    void Update ()
    {
        /* przesun kamere */
        if(Input.GetKey("w"))
            rigidbody.AddForce(Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * velocity, 0, Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * velocity);
        else if(Input.GetKey("s"))
            rigidbody.AddForce(-Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * velocity, 0, -Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * velocity);
        else if(Input.GetKey("a"))
            rigidbody.AddForce(-Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * velocity, 0, Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * velocity);
        else if(Input.GetKey("d"))
            rigidbody.AddForce(Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * velocity, 0, -Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * velocity);
        /* obroc kamere */
        else if(Input.GetKey("up"))
            rigidbody.AddTorque(-Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * angularVelocity, 0, Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * angularVelocity);
        else if(Input.GetKey("down"))
            rigidbody.AddTorque(Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * angularVelocity, 0, -Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * angularVelocity);
        else if(Input.GetKey("left"))
            rigidbody.AddTorque(0, -angularVelocity, 0);
        else if(Input.GetKey("right"))
            rigidbody.AddTorque(0, angularVelocity, 0);
    }
}
