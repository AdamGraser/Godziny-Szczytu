using UnityEngine;

/**<summary>Klasa reprezentujaca sygnalizacje swietlna na skrzyzowaniu</summary>*/
public class TrafficLight : MonoBehaviour
{
    /**<summary>Czerwone swiatlo</summary>*/
    public GameObject redLight;
    /**<summary>Zielone swiatlo</summary>*/
    public GameObject greenLight;
    /**<summary>Sciana-szlaban</summary>*/
    public GameObject wall;

    /**<summary>Wartosc obnizenia sciany, gdy zapala sie zielone swiatlo</summary>*/
    public float wallShift;
    /**<summary>Poczatkowa pozycja sciany</summary>*/
    private float wallYPos; 

    private Crossroads sourceCrossroads;
    /**<summary>Skrzyzowanie, z ktorego prowadzi droga (na ktorej umieszczona jest sciana)</summary>*/
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

   
    private Crossroads owner;
    /**<summary>Skrzyzowanie-wlasciciel tej sygnalizacji</summary>*/
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

    /** <summary>Funkcja przygotowujaca sygnalizacje do zabawy. Wywolywana na poczatku istnienia obiektu</summary> */
    void Awake()
    {
        wallYPos = wall.transform.position.y;
    }

    /* ***********************************************************************************
     *                                   FUNKCJE PUBLICZNE
     * *********************************************************************************** */

    /**<summary>Blokuje przejazd</summary>*/
    public void ActivateRedLight()
    {
        redLight.SetActive(true);
        greenLight.SetActive(false);
        wall.rigidbody.MovePosition(new Vector3(wall.transform.position.x, wallYPos, wall.transform.position.z));
    }

    /**<summary>Odblokowuje przejazd</summary>*/
    public void ActivateGreenLight()
    {
        redLight.SetActive(false);
        greenLight.SetActive(true);
        wall.rigidbody.MovePosition(new Vector3(wall.transform.position.x, wall.transform.position.y - wallShift, wall.transform.position.z));
    }
}
