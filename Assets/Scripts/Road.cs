using UnityEngine;

/* reprezentuje droge */
public class Road : MonoBehaviour 
{
    private Crossroads start; //skrzyzowanie, w ktorym droga sie rozpoczyna
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

    private Crossroads end; //skrzyzowanie, w ktorym droga sie konczy
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

    /* pozycja logiczna. odzworowuje wspolrzedne: X i Z. bezposrednio powiazana z rzeczywista pozycja.
     * zmiana pozycji logicznej skutkuje zmiana pozycji rzeczywistej */
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
    /* prawdziwa pozycja na swiecie */
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

    public float Length { get; private set; } //dlugosc drogi

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    void Awake()
    {
        Length = 0;
    }

    /* ***********************************************************************************
     *                                FUNKCJE POMOCNICZE
     * *********************************************************************************** */

    private float CalculateLength()
    {
        return (start.LogicPosition - end.LogicPosition).magnitude;
    }

}
