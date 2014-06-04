using UnityEngine;

/**<summary>Funkcja reprezentujaca globalny, 24-godzinny zegar</summary>*/
public class Clock : MonoBehaviour
{
    /**<summary>Ile sekund trwa doba w grze</summary>*/
    public static readonly float Day = 360f;
    /**<summary>Aktualny czas (w sekundach)</summary>*/
    public float DayTime {get; set;}
    /**<summary>Aktualna godzina</summary>*/
    public int Hour
    {
        get
        {
            float anHour = Day / 24f;

            return Mathf.RoundToInt(DayTime / anHour) % 24;
        }
    }

    /* ***********************************************************************************
     *                        FUNKCJE ODZIEDZICZONE PO MONOBEHAVIOUR 
     * *********************************************************************************** */

    /** <summary>Funkcja przygotowujaca zegar do dzialania</summary> */
    void Awake() 
    {
        DayTime = 0;
    }

    /** <summary>Funkcja wywolywana podczas kazdej klatki.</summary> */
    void Update() 
    {
        DayTime = (DayTime + Time.deltaTime) % Day;
    }
}
