using UnityEngine;

/* reprezentuje swiatla na skrzyzowaniu */
public class TrafficLight : MonoBehaviour
{
    public GameObject redLight; //czerwone swiatlo
    public GameObject greenLight; //zielone swiatlo
    public GameObject wall; //sciana, dzialajaca jak szlaban

    public float wallShift; //obnizenie muru, gdy zapala sie zielone swiatlo
    private float wallYPos; //oryginalna pozycja y sciany

    private Crossroads sourceCrossroads; //skrzyzowanie, z ktorego prowadzi droga, na ktorej umieszczona jest sciana
    public Crossroads SourceCrossroads
    {
        set
        {
            sourceCrossroads = value;

            if(sourceCrossroads != null && owner != null)
                transform.LookAt(sourceCrossroads.RealPosition);
        }
        get
        {
            return sourceCrossroads;
        }
    }

    private Crossroads owner; //skrzyzowanie-wlasciciel danej sciany
    public Crossroads Owner
    {
        set
        {
            owner = value;
            transform.position = owner.RealPosition;

            if(sourceCrossroads != null && owner != null)
                transform.LookAt(sourceCrossroads.RealPosition); //obroc sciane w kierunku skrzyzowania zrodlowego
        }
        get
        {
            return sourceCrossroads;
        }
    }

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    void Awake()
    {
        wallYPos = wall.transform.position.y;
    }

    /* ***********************************************************************************
     *                                   FUNKCJE PUBLICZNE
     * *********************************************************************************** */

    /* blokuje przejazd */
    public void ActivateRedLight()
    {
        redLight.SetActive(true);
        greenLight.SetActive(false);
        wall.rigidbody.MovePosition(new Vector3(wall.transform.position.x, wallYPos, wall.transform.position.z));
    }

    /* odblokowuje przejazd */
    public void ActivateGreenLight()
    {
        redLight.SetActive(false);
        greenLight.SetActive(true);
        wall.rigidbody.MovePosition(new Vector3(wall.transform.position.x, wall.transform.position.y - wallShift, wall.transform.position.z));
    }
}
