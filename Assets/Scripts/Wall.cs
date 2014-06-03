using UnityEngine;

/* reprezentuje sciane (szlaban) na skrzyzowaniu */
public class Wall : MonoBehaviour 
{
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

    public float shift; //przesuniecie sciany wzgledem osi Y, gdy ta jest aktywowana lub deaktywowana
    private float yPos; //oryginalna pozycja wzgledem osi y

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    void Awake()
    {
        yPos = transform.position.y;
    }

    /* ***********************************************************************************
     *                                   FUNKCJE PUBLICZNE
     * *********************************************************************************** */

    /* aktywuje sciane */
    public void Activate()
    {
        Vector3 pos = transform.position; //pozycja docelowa
        pos.y = yPos;

        rigidbody.MovePosition(pos);
    }

    /* deaktywuje sciane */
    public void Deactivate()
    {
        Vector3 pos = transform.position; //pozycja docelowa
        pos.y -= shift;

        rigidbody.MovePosition(pos);
    }
}
