using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AgentD : MonoBehaviour 
{
	//localny punkt docelowy
	protected Vector2 destination;
	
	//miejsca pracy i zamieszkania
	public Vector2 HomePlace {get; set;}
	public Vector2 WorkPlace {get; set;}
	//czas wyjazdu i powrotu z pracy (w sekundach)
	public float WorkStart {get; set;}
	public float WorkEnd {get; set;}
	
	//maksymalna odleglosc od punktu docelowego, przy ktorej uznaje sie, ze punkt zostal osiagniety
	public float range;
	
	//predkosc samochodu
	protected float speed;
	//hamulec (wartosc 1 umozliwia normalna jazde; > 1 hamuje)
	protected float brake;
	//GPS
	protected Gps gps;
	//sciezka wyznaczona przez gps
	protected List<Vector2> path;
	
	//czy agent jest w trakcie skrecania
	protected bool isTurning;
	//kat, o jaki ma sie obrocic
	protected float turningAngle;
	//kat, o jaki na razie obrocono agenta
	protected float turningAngleDone;
	//krok, o jaki ma sie wykonac obrot
	public float turningAngleStep;
	
	
	void Start() 
	{
		destination = new Vector2(0, 0);
		HomePlace = new Vector2(0, 0);
		WorkPlace = new Vector2(0 ,0);
		WorkEnd = 0;
		WorkStart = 0;
		brake = 1;
		speed = 1;
		gps = new Gps();
		gps.LoadMap((Map)GameObject.FindWithTag("Map").GetComponent("Map"));
		path = new List<Vector2>();
		isTurning = false;
		turningAngle = 0;
		turningAngleDone = 0;
		
		/*path.Add(new Vector2(0, 0));
		path.Add(new Vector2(0, 4));
		path.Add(new Vector2(-4, 8));
		path.Add(new Vector2(-4, 4));
		path.Add(new Vector2(-4, 0));
		path.Add(new Vector2(-2, -2));
		path.Add(new Vector2(0, 0));
		path.Add(new Vector2(0, 4));
		path.Add(new Vector2(4, 8));
		path.Add(new Vector2(8, 8));
		path.Add(new Vector2(8, -8));
		path.Add(new Vector2(4, -8));
		path.Add(new Vector2(4, -2));
		path.Add(new Vector2(4, 0));
		path.Add(new Vector2(0, 4));
		path.Add(new Vector2(0, 0));*/

		gps.FindPath(Vector2.zero, new Vector2(8, 8));
		
		NextDestination();
		transform.LookAt(destination);
	}
	
	void FixedUpdate()
	{
		float distance = 0;
		float angle = 0;
		Vector3 movement;
		
		//oblicz predkosc agenta
		speed /= brake;
		distance = speed;
		
		//jezeli dojechano do skrzyzowania, to zacznij skrecac
		if(IsDestinationReached())
		{
			//jezeli to nie bylo ostatnie skrzyzowanie, to jedz do nastepnego
			if(path.Count != 0)
			{
				//jak bardzo trzeba skrecic
				isTurning = true;
				NextDestination();
				turningAngleDone = 0;
				rigidbody.velocity = Vector3.zero;
				turningAngle = TurningAngle(destination) - transform.eulerAngles.y;
				if(turningAngle < 0)
					turningAngle += 360;
				
				Debug.Log("turning Angle: " + turningAngle);
				Debug.Log("agent position: " + transform.position);
				Debug.Log("destination:  " + destination);
			}
			
			//w przeciwnym razie stan w miejscu
			else
				Brake();
		}
		
		//skrec
		if(isTurning)
		{
			if(Turn(turningAngleStep, ref turningAngleDone, turningAngle))
				isTurning = false;
			
		}
		//przemiesc sie zgodnie z kierunkiem, w ktorym jestes zorientowany
		else
		{
			angle = rigidbody.rotation.eulerAngles.y;
			movement = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0, Mathf.Cos(Mathf.Deg2Rad * angle));
			rigidbody.velocity = movement * distance;
		}
	}
	
	void Update()
	{
		Debug.LogWarning(Time.deltaTime);
	}
	
	/* ustawia skaldowa destination na kolejny punkt docelowy.
	 * zwraca true, jesli taki punkt istnieje; w przeciwnym przepadku zwraca false */
	protected bool NextDestination()
	{
		bool returnVal = path.Count == 0 ? false : true;
		
		if(returnVal)
		{
			destination = path[0];
			path.Remove(destination);
		}
		
		return returnVal;
	}
	
	/* czy punkt docelowy znajduje sie w zasiegu agenta */
	protected bool IsDestinationReached()
	{
		Vector2 pos = new Vector2(transform.position.x, transform.position.z);
		
		if(Vector2.Distance(pos, destination) <= range)
			return true;
		else
			return false;
	}
	
	/* zwraca kat, o jaki musi sie obrocic agent, 
	 * by byc skierowanym w kieruku docelowego skrzyzowania. */
	private float TurningAngle(Vector2 dest)
	{
		float angle = 0;
		Quaternion rotation = transform.rotation;
		
		transform.LookAt(new Vector3(dest.x, transform.position.y, dest.y));
		angle = transform.rotation.eulerAngles.y;
		transform.rotation = rotation;
		
		return angle;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag.Equals("Agent"))
		{
			Brake();
		}
		/*else if(other.tag.Equals("Crossroads"))
		{Debug.Log("DUP");
			if(NextDestination())
				transform.LookAt(new Vector3(destination.x, transform.position.y, destination.y));
			/* jezeli to byl ostatni punkt *
			else
			{
				brake = 1.3f;
			}
		}*/
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.tag.Equals("Agent"))
		{
			Go();
		}
	}
	
	/* zatrzymuje agenta */
	protected void Brake()
	{
		brake = 1.3f;
	}
	
	/* wprawia agenta w ruch */
	protected void Go()
	{
		brake = 1;
		speed = 1;
	}
	
	/* obraca agenta o podany kat. 
	 * funkcja dba, by agent nie obroci sie bardziej niz jest to podane w maxRotationAngle
	 * step - krok (kat), o jaki nalezy obrocic agenta
	 * rotationAngleDone - calkowity obrot agenta, jaki do tej pory wykonano (updateowany przez te funkcje)
	 * maxRotationAngle - maksymalny kat obrotu, jaki mozna wykonac na agencie
	 * zwraca: true, jesli osiagnieto wartosc maxRotationEngle; false - w przeciwnym wypadku */
    protected bool Turn(float step, ref float rotationAngleDone, float maxRotationAngle)
    {
        Vector3 position = transform.position;
        bool returnVal = false;
        
        if(maxRotationAngle != 0)
        {
            rotationAngleDone += step;
            
            maxRotationAngle = (maxRotationAngle - 360f) * -1f;
            
            if(rotationAngleDone > maxRotationAngle)
            {
                transform.RotateAround(position, new Vector3(0, 1, 0), rotationAngleDone - step - maxRotationAngle);
                rotationAngleDone = maxRotationAngle;
                returnVal = true;
            }
            else
                transform.RotateAround(position, new Vector3(0, 1, 0), -step);
        }
        else
            returnVal = true;
        
        return returnVal;
	}
}
