using UnityEngine;

/**<summary>Reprezentuje droge</summary>*/
public class Road : MonoBehaviour 
{
    private Crossroads start;
    /**<summary>Skrzyzowanie rozpoczynajace te droge</summary>*/
    public Crossroads Start
    {
        set
        {
            start = value;

            if(start != null && end != null)
                Length = CalculateLength();
        }

        get
        {
            return start;
        }
    }

    private Crossroads end;
    /**<summary>Skrzyzowanie bedace koncem tej drogi</summary>*/
    public Crossroads End
    {
        set
        {
            end = value;
            
            if(start != null && end != null)
                Length = CalculateLength();
        }

        get
        {
            return end;
        }
    }

    /**<summary>Pozycja logiczna bedaca punktem srodkowym drogi. Odzworowuje wspolrzedne: X i Z. Bezposrednio powiazana z rzeczywista pozycja.
     * Zmiana pozycji logicznej skutkuje zmiana pozycji rzeczywistej</summary> */
    public Vector2 LogicPosition
    {
        get
        {
            return new Vector2(transform.position.x, transform.position.z);
        }

        set
        {
            transform.position = new Vector3(value.x, transform.position.y, value.y);
        }
    }
    /**<summary>Rzeczywista pozycja na swiecie</summary>*/
    public Vector3 RealPosition
    {
        set
        {
            transform.position = value;
        }

        get
        {
            return transform.position;
        }
    }

    /**<summary>Dlugosc drogi</summary>*/
    public float Length { get; private set; }

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /** <summary>Funkcja przygotowujaca drogi do zabawy. Wywolywana na poczatku istnienia obiektu.</summary> */
    void Awake()
    {
        Length = 0;
    }

    /* ***********************************************************************************
     *                                FUNKCJE POMOCNICZE
     * *********************************************************************************** */

    /**<summary>Oblicza dlugosc drogi</summary>*/
    private float CalculateLength()
    {
        return (start.LogicPosition - end.LogicPosition).magnitude;
    }

}
