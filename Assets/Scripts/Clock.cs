using UnityEngine;

/* globalny zegar */
public class Clock : MonoBehaviour
{
    /* ile sekund trwa doba w grze */
    public static readonly float Day = 360f;

    /* aktualny czas */
    public float DayTime {get; set;}

    /* aktualna godzina */
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

    void Awake() 
    {
        DayTime = 0;
    }

    void Update() 
    {
        DayTime = (DayTime + Time.deltaTime) % Day;
    }
}
