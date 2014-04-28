using UnityEngine;
using System.Collections.Generic;
using System;

public class SmartGps : Gps
{
	// Struktura opisujaca jedna sciezke
	private class PathListElement
	{
		//Sciezka
		public List<Vector2> Path;
		//Czas potrzebny na przejechanie sciezki w podanym przedziale czasu
		public float[] TimeOf;
		//Liczba przejazdow po danej sciezce w podanym przedziale czasu
		public int[] NumberOfWays;

		//Liczba istniejacych przedzialow czasu
		public const int NumberOfTimes = 6;
		//Predkosc agenta (do ustalenia)
		public const int Speed = 10;

		//Konstruktor
		public PathListElement (List<Vector2> PathToAdd, Map MyMap)
		{
			TimeOf = new float[NumberOfTimes];
			NumberOfWays = new int[NumberOfTimes];
			Path = PathToAdd;

			int i;

			//Obliczenie teoretycznego czasu przejazdu po danej sciezce
			float length = 0;
			for (i=1; i<Path.Count; ++i)
			{
				length += MyMap.GetRoadLength(Path[i-1], Path[i]);
			}
			float time = length/Speed;

			//Inicjalizacja czasow przejazdu
			for (i=0; i < NumberOfTimes; ++i)
			{
				TimeOf[i] = time;
				NumberOfWays[i] = 1;
			}
		}
	}

	//Lista wszystkich wezlow na mapie
	private List<Vector2> AllCrossroads;
	//Lista wszystkich sensownych sciezek
	private List<PathListElement> AllPaths;
	//Lista tymczasowa (na brudno)
	private List<PathListElement> TemporaryPaths;

	//Funkcja wywolywana wewnatrz funkcji "LoadMap"
	private void RecursiveSearcher (List<Vector2> currentPath, Vector2 lastCrossing, Vector2 end)
	{
		currentPath.Add (lastCrossing);

		if (lastCrossing == end)
		{
			//Sciezka jest gotowa - trzeba ja dodac
			TemporaryPaths.Add(new PathListElement(currentPath, MyMap));
		}
		else
		{
			foreach (Vector2 c in MyMap.FindConnectedCrossroads(lastCrossing))
			{
				if (!currentPath.Contains(c))
				{
					List<Vector2> nextPath = new List<Vector2> ();
					nextPath.AddRange(currentPath);
					RecursiveSearcher(nextPath, c, end);
				}
			}
		}
	}

	//Ładowanie mapy
	override public void LoadMap (Map mapToLoad)
	{
		MyMap = mapToLoad;
		AllCrossroads = MyMap.GetAllCrossings();

		AllPaths = new List<PathListElement> ();

		int i, j;
		for (i=0; i<(AllCrossroads.Count-1); ++i)
		{
			for (j=i+1; j<AllCrossroads.Count; ++j)
			{
				//Znajdywanie wszystkich sciezek pomiedzy wezlami "i" i "j"
				TemporaryPaths = new List<PathListElement> ();
				List<Vector2> currentPath = new List<Vector2>();
				RecursiveSearcher(currentPath, AllCrossroads[i], AllCrossroads[j]);

				//TODO: Powywalanie bazsensownych sciezek (wielokrotnie dluzszych od najkrotszej)

				//Dorzucenie znalezionych sciezek do ogolnej listy
				AllPaths.AddRange(TemporaryPaths);
			}
		};
	}

	// Tu bedzie inteligentny algorytm
	override public List<Vector2> FindPath(Vector2 start, Vector2 end)
	{
		return null;
	}
}
