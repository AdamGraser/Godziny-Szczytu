using UnityEngine;
using System.Collections;

/* globalny zegar */
public class Clock : MonoBehaviour
{
	/* ile sekund trwa doba w grze */
	public static readonly float Day = 720f;

	/* aktualny czas */
	public float DayTime {get; set;}
	
	void Awake() 
	{
		DayTime = 0;
	}

	void Update () 
	{
		DayTime = (DayTime + Time.deltaTime) % Day;
	}
}
