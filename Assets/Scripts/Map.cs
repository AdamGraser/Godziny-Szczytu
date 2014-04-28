using UnityEngine;
using System.Collections.Generic;

/* Reprezentuje mape w postaci grafu (polaczonych ze soba punktow z okresleniem dlugosci pomiedzy nimi. */
public class Map : MonoBehaviour 
{
	/* reprezentuje skrzyzowanie */
    private class Road
	{
		private Vector2 start;
		private Vector2 end;
		private float length = 0;
		private GameObject model;

		/* punkt poczatkowy drogi */
		public Vector2 Start
		{
			set
			{
				start = new Vector2(value.x, value.y);
				CalculateLength();
			}
			get
			{
				return start;
			}
		}

		/* punkt koncowy drogi */
		public Vector2 End
		{
			set
			{
				end = new Vector2(value.x, value.y);
				CalculateLength();
			}
			get
			{
				return end;
			}
		}

		/* dlugosc drogi */
		public float Length
		{
			get
			{
				return length;
			}
		}

		/* fizyczny/graficzny model drogi */
		public GameObject Model
		{
			get
			{
				return model;
			}
		}

		/* oblicza dlugosc drogi. Jezeli start lub end sa null, wtedy dlugosc wynosi 0 */
		private void CalculateLength()
		{
			length = Mathf.Sqrt(Mathf.Pow(start.x - end.x, 2) + Mathf.Pow(start.y - end.y, 2));
		}

		/* konstruktor.
		 * model - fizyczny/graficzny model drogi */
		public Road(GameObject model)
		{
			this.model = model;
		}
	}

	/* ------------------------------------- */

    /* reprezentuje skrzyzowanie */
    private class Crossroads
    {
        private GameObject model;
		private Vector2 position;
		/* polaczenia z innymi skrzyzowaniami. kluczem jest pozycja (x, y) */
		private Dictionary<Vector2, Crossroads> connectedCrossroads;

        /* fizyczny/graficzny model */
        public GameObject Model
        {
            get
            {
                return model;
            }
        }

        /* pozycja skrzyzowania */
        public Vector2 Position
        {
            get
            {
				return position;
            }
        }

		/* polaczenia z innymi skrzyzowaniami */
		public Dictionary<Vector2, Crossroads> ConnectedCrossroads
		{
			get
			{
				return connectedCrossroads;
			}
		}

		/* konstruktor
		 * model - model fizyczny/graficzny skrzyzowania */
		public Crossroads(GameObject model)
		{
			this.model = model;
			position = new Vector2(model.transform.position.x, model.transform.position.z);
			connectedCrossroads = new Dictionary<Vector2, Crossroads>();
		}

		/* konstruktor
		 * model - model fizyczny/graficzny skrzyzowania 
		 * pos - pozycja skrzyzowaania */
		public Crossroads(GameObject model, Vector2 pos)
		{
			this.model = model;
			this.model.transform.Translate(new Vector3(pos.x, 0, pos.y));
			position = pos;
			connectedCrossroads = new Dictionary<Vector2, Crossroads>();
		}
       
    }

    /* ------------------------------------- */

    /* prefab skrzyzowania */
    public GameObject crossroadsPrefab;
    /* prefab drogi */
    public GameObject roadPrefab;

	/* zbior skrzyzowan. kluczem jest pozycja (x, y) */
	private Dictionary<Vector2, Crossroads> crossroads;
	/* zbior drog, ktore lacza skrzyzowania. kluczem jest pozycja srodka drogi (x, y) */
	private Dictionary<Vector2, Road> roads; 

	/* ekwiwalent konstruktora */
	public void Start () 
	{
		crossroads = new Dictionary<Vector2, Crossroads>();
		roads = new Dictionary<Vector2, Road>();
	}

	/* tworzy i dodaje do mapy skrzyzowanie.
	 * pos - pozycja skrzyzowania
	 * zwraca: true, jesli skrzyzowanie dodane; false - w przeciwnym wypadku */
	public bool AddCrossroads(Vector2 pos)
	{
		bool returnVal = false;
		Crossroads cross; //nowoutworzone skrzyzowanie

		//jesli skrzyzowanie na danej pozycji juz istnieje, to nie dodawaj nowego
		if(!crossroads.ContainsKey(pos))
		{
			cross = new Crossroads((GameObject)Instantiate(crossroadsPrefab), pos);
			crossroads.Add(pos, cross);

			//jezeli skrzyzowanie zostalo postawione na drodze, to ja podziel i polacz z tym skrzyzowaniem
			var crossArray = FindRoadContainingPoint(pos);
			if(crossArray != null)
			{
				var c1 = crossroads[crossArray[0]];
				var c2 = crossroads[crossArray[1]];

				RemoveRoad(c1.Position, c2.Position);
				AddRoad(c1.Position, pos);
				AddRoad(pos, c2.Position);
			}

			returnVal = true;
		}

		return returnVal;
	}

	/* dodaje droge laczaca dwa skrzyzowania
	 * cross1Pos, cross2Pos - skrzyzowania, ktore maja zostac polaczone
	 * zwraca: true, gdy droga zostanie dodana; false - w przeciwnym wypadku */
	public bool AddRoad(Vector2 cross1Pos, Vector2 cross2Pos)
	{
		bool returnVal = false;
		Road road; //nowoutworzona droga
		Vector2 roadPos; //punkt srodkowy drogi
		float roadAngle = 0; //kat obrotu drogi w radianach
		Crossroads cross1 = crossroads[cross1Pos];
		Crossroads cross2 = crossroads[cross2Pos]; 

		//jesli polaczenie istnieje, to nic nie rob. nei lacz tez skryzowania z samym soba
		//nie dodawal drogi, gdy moglaby przecinac inna droge
		if(cross1 != null && cross2 != null && cross1Pos != cross2Pos && !cross1.ConnectedCrossroads.ContainsKey(cross2Pos) &&
		   FindRoadCrossingRoad(cross1Pos, cross2Pos) == null)
		{
			road = new Road((GameObject)Instantiate(roadPrefab));

			//nadaj punkty koncowe
			road.Start = cross1Pos;
			road.End = cross2Pos;

			//nadaj odpowiednie polozenie drogi
			roadPos = new Vector2((cross1Pos.x + cross2Pos.x) / 2.0f, (cross1Pos.y + cross2Pos.y) / 2.0f);
			road.Model.transform.Translate(new Vector3(roadPos.x, 0.0f, roadPos.y));

			//nadaj odpowiednia dlugosc
			road.Model.transform.localScale = new Vector3(0.1f, 1.0f, road.Length * road.Model.transform.localScale.z);

			//obroc pod odpowiednim katem
			roadAngle = (cross1Pos.x - cross2Pos.x)/ (cross1Pos.y - cross2Pos.y);
			roadAngle = Mathf.Atan(roadAngle);
			road.Model.transform.Rotate(new Vector3(0, roadAngle * Mathf.Rad2Deg, 0));

			//dodaj droge do kolekcji
			roads.Add(roadPos, road);

            //polacz logicznie ze soba skrzyzowania
            cross1.ConnectedCrossroads.Add(cross2Pos, cross2);
            cross2.ConnectedCrossroads.Add(cross1Pos, cross1);

			returnVal = true;
		}

		return returnVal;
	}

	/* usuwa skrzyzowanie o podanej pozycji. niszczy tez wszelkie drogi polaczone z tym skrzyzowaniem oraz polaczenia logiczne
	 * zwraca: true, jesli skrzyzowanie zostanie usuniete; false - w przeciwnym wypadku */
	public bool RemoveCrossroads(Vector2 pos)
	{
		bool returnVal = false;
		Crossroads cross; //skrzyzowanie do usuniecia
		List<Vector2> roadsToDelete = new List<Vector2>();

		if(crossroads.ContainsKey(pos))
		{
			cross = crossroads[pos];

			//usun polaczenia (logiczne oraz drogi)
			foreach(var c in cross.ConnectedCrossroads)
				roadsToDelete.Add(c.Value.Position);
			foreach(var r in roadsToDelete)
				RemoveRoad(pos, r);

			//usun samo skrzyzowanie
			GameObject.Destroy(cross.Model);
			returnVal = crossroads.Remove(cross.Position);
		}

		return returnVal;
	}

	/* usuwa droge laczaca skrzyzowania o podanych pozycjach. usuwa tez polaczenie logiczne miedzy skrzyzowaniami*/
	public bool RemoveRoad(Vector2 cross1Pos, Vector2 cross2Pos)
	{
		bool returnVal = false;
		Crossroads cross1 = crossroads[cross1Pos];
		Crossroads cross2 = crossroads[cross2Pos];
		Vector2 roadPos;
		Road road; //droga do usuniecia

		if(cross1 != null && cross2 != null && cross1 != cross2)
		{
			roadPos = (cross1Pos + cross2Pos) / 2.0f;
			road = roads[roadPos];

			if(road != null)
			{
				GameObject.Destroy(road.Model);
				returnVal = roads.Remove(roadPos);
				
				//usun polaczeni logiczne miedzy skrzyzowaniami
				cross1.ConnectedCrossroads.Remove(cross2Pos);
				cross2.ConnectedCrossroads.Remove(cross1Pos);
			}
		}

		return returnVal;
	}

	/* zwraca zwraca namiary 2 srzyzowan polaczonych droga, ktora zawiera podany punkt.
	 * zwraca null, gdy nie istnieje taka droga */
	public Vector2[] FindRoadContainingPoint(Vector2 point)
	{
		foreach(var r in roads)
		{
			if(Math.IsPointBelongingToSegment(r.Value.Start, r.Value.End, point))
				return new Vector2[2] {r.Value.Start, r.Value.End};
		}

		return null;
	}

	/* znajduje i zwraca namiary skrzyzowan polaczonych przez droge przecinajaca inna droge (bez skrzyzowania)
	 * start, end - punkty koncowe sprawdzanej drogi
	 * zwraca null, jesli taka droga nie istnieje */
	public Vector2[] FindRoadCrossingRoad(Vector2 start, Vector2 end)
	{
		//sprawdzanie kazdej srogi
		foreach(var r in roads)
		{
			//sprawdz, czy ktorys z punktow naszej drogi nie przynalezy do drogi r
			/*if(Math.IsPointBelongingToSegment(start, end, r.Value.Start) || 
			   Math.IsPointBelongingToSegment(start, end, r.Value.End)) || 
			   Math.IsPointBelongingToSegment(r.Value.Start, r.Value.End, start) ||
			   Math.IsPointBelongingToSegment(r.Value.Start, r.Value.End, end))
			{
				return new Vector2[] {r.Value.End, r.Value.Start};
			}
			//sprawdz, czy odcinki sie przecinaja (zrodlo: http://www.algorytm.org/geometria-obliczeniowa/przecinanie-sie-odcinkow.html )
			else */if(Math.Det(start, end, r.Value.Start) * Math.Det(start, end, r.Value.End) < 0 &&
			        Math.Det(r.Value.Start, r.Value.End, start) * Math.Det(r.Value.Start, r.Value.End, end) < 0)
			{
				return new Vector2[] {r.Value.End, r.Value.Start};
			}
		}

		return null;
	}

	/* usuwa 'sametne' skrzyzowania, ktore nie sa polaczone z zadnymi innymi */
	public void RemoveLonelyCrossroads()
	{
		List<Vector2> crossToRemove = new List<Vector2>();

		foreach(var c in crossroads)
		{
			if(c.Value.ConnectedCrossroads.Count == 0)
				crossToRemove.Add(c.Key);
		}
		foreach(var c in crossToRemove)
			RemoveCrossroads(c);
	}

	/* czy ulozenie skrzyzowan wzgledem siebie jest dozwolone? (czy postawione sa w jednym z 8 mozliwych kierunkow?) */
	public bool IsCrossroadPlacedOnPossiblePlace(Vector2 cross1, Vector2 cross2)
	{
		float a = (cross1.y - cross2.y) / (cross1.x - cross2.x); //wspolczynnik a porstej miedzy skrzyzowaniami

		if(float.IsInfinity(a) || a == 0 || a == -1 || a == 1)
			return true;

		return false;
	}

	/* zwraca pozycje pierwszego lepszego skrzyzowania (lub null, jesli nie ma skrzyzowan) */
	public Vector2 GetAnyCrossroads()
	{
		Vector2 pos = new Vector2(0, 0);

		//zaladam, ze juz jakies skrzyzowanie istnieje.
		//jesli nie, to niech Bog ma cie w swojej opiece
		foreach(var c in crossroads)
			pos = c.Key;

		return pos;
	}

	/* zwraca pozycje skrzyzowan polaczonych droga ze skrzyzowaniem o pozycji cross. 
	 * jesli nie istnieje podane skrzyzowanie, zwracana jest wartosc null */
	public List<Vector2> FindConnectedCrossroads(Vector2 crossPos)
	{
		List<Vector2> crosses = new List<Vector2>();
		Crossroads cross = crossroads[crossPos];

		if(cross != null)
			foreach(var c in cross.ConnectedCrossroads)
				crosses.Add(c.Key);
		else
			crosses = null;

		return crosses;
	}

	/* zwraca dlugosc polaczenia (drogi) miedzy skrzyzowaniami o pozycjach cross1 i cross2 
	 * zwraca -1, jesli polaczenie miedzy skrzyzowaniami nie istnieje */
	public float GetRoadLength(Vector2 cross1, Vector2 cross2)
	{
		Road road = roads[(cross1 + cross2) / 2];

		if(road != null)
			return road.Length;
		else
			return -1;
	}

	/* Funkcja rekurencyjna wykorzystywana przez funkcje GetAllCrossings (by MWr) */
	private void AddNeighboursToList (List<Vector2> CurrentList, Vector2 LastCrossing)
	{
		foreach (Vector2 c in FindConnectedCrossroads(LastCrossing))
		{
			if (!CurrentList.Contains(c))
			{
				CurrentList.Add(c);
				AddNeighboursToList(CurrentList, c);
			}
		}
	}

	/* Funkcja zwracajaca liste wszystkich wezlow jako liste obiektow typu Vector2 (by MWr) */
	public List<Vector2> GetAllCrossings()
	{
		List<Vector2> crosses = new List<Vector2>();
		Vector2 last = GetAnyCrossroads();
		crosses.Add(last);
		AddNeighboursToList(crosses, last);
		return crosses;
	}

	/* lokalna klasa serwujaca pare uzytecznych metod do obliczen matematycznych */
	private class Math
	{
		/* oblicza wspolczynnik macierzy 3x3, utworzonej z danych pochodzacych z 3 punktow */
		public static float Det(Vector2 a, Vector2 b, Vector2 c)
		{
			return a.x*b.y + b.x*c.y + c.x*a.y - c.x*b.y - a.x*c.y - b.x*a.y;
		}

		/* sprawdza, czy punkt C przynalezy do odcinka |AB| */
		public static bool IsPointBelongingToSegment(Vector2 a, Vector2 b, Vector2 c)
		{
			float det = Det(a, b, c);

			if(det != 0)
				return false;
			else if(Mathf.Min(a.x, b.x) <= c.x && Mathf.Max(a.x, b.x) >= c.x && 
			        Mathf.Min(a.y, b.y) <= c.y && Mathf.Max(a.y, b.y) >= c.y)
			{
				return true;
			}

			return false;
		}
	}
}
