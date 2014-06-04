using System;
using System.Collections.Generic;
using UnityEngine;

/**<summary>Reprezentuje nawigacje GPS</summary>*/
public class Gps
{
    /**<summary>Mapa</summary>*/
    protected Map MyMap;

    /**<summary>Akcesor do mapy</summary>
     <param name="mapToLoad">Mapa do zaladowania</param>*/
    public virtual void LoadMap(Map mapToLoad)
    {
        MyMap = mapToLoad;
    }

    /**<summary>Wyszukuje najkrotsza sciezke uzywajac algorytmu Dijkstry</summary>
     <param name="start">Punkt poczatkowy</param>
     <param name="end">Punkt koncowy</param>*/
    public virtual List<Vector2> FindPath(Vector2 start, Vector2 end)
    {
        // Przygotowanie odpowiednich tablic		
        List<Vector2> crossings = GetAllCrossroadsPositions();
        int numberOfCrossings = crossings.Count;
        int max = int.MaxValue;
        
        Vector2[] crossingsTable = new Vector2[numberOfCrossings]; // Indeksy wezlow
        float[] distance = new float[numberOfCrossings]; // Odleglosc do wezla
        int[] previousCrossing = new int[numberOfCrossings]; // Indeks poprzednika
        bool[] flags = new bool[numberOfCrossings];
        
        int i;
        for (i = 0; i < numberOfCrossings; ++i)
        {
            crossingsTable[i] = crossings[i];
            
            if ((crossings[i] == start))
            {
                distance[i] = 0;
            }
            else
            {
                distance[i] = max; // nieskonczonosc
            }
            
            previousCrossing[i] = i; // niezdefiniowany (sam dla siebie)
            flags[i] = false;
        }
        
        int j, current, neighbour;
        float currentDistance;
        
        // Wlasciwy algorytm //
        
        for (i = 0; i < numberOfCrossings; ++i) // Proces trzeba powtorzyc tyle razy, ile jest wezlow
        {
            // Szukanie nieprzejrzanego węzła położonego jak najbliżej poczatku scieżki
            current = 0;
            currentDistance = max;
            for (j = 0; j < numberOfCrossings; ++j)
            {
                if ((flags[j] == false) && (distance[j] <= currentDistance))
                {
                    current = j;
                    currentDistance = distance[j];
                }
            }
            
            //Przejrzenie polaczen do sasiadow
            foreach (var next in MyMap.AllCrossroads[crossingsTable[current]].ConnectedCrossroads)
            {
                //Pobranie indeksu sąsiada
                neighbour = 0;
                for (j = 0; j < numberOfCrossings; ++j)
                {
                    if ((crossingsTable[j] == next.Value.LogicPosition))
                    {
                        neighbour = j;
                        break;
                    }
                }
                
                //Wlasciwa czesc algorytmu

                float newDistance = currentDistance + MyMap.AllRoads[(crossingsTable[current] + next.Value.LogicPosition) / 2f].Length;
                if (distance[neighbour] > newDistance)
                {
                    previousCrossing[neighbour] = current;
                    distance[neighbour] = newDistance;
                }
            }
            
            flags[current] = true;
            
        }
        
        // Skladanie sciezki
        
        List<Vector2> path = new List<Vector2>();
        current = 0;
        for (i = 0; i < numberOfCrossings; ++i) // Szukanie indeksu wezla koncowego
        {
            flags[i] = false; // Flaga przyda sie podczas skladania sciezki
            if ((crossingsTable[i] == end))
                current = i;
        }
        
        path.Add(crossingsTable[current]);
        
        try
        {
            while ((crossingsTable[current] != start))
            {
                if (flags[current]) // Blokada przed zapetleniem
                {
                    throw new Exception();
                };
                
                flags[current] = true;
                
                current = previousCrossing[current];
                path.Insert(0, crossingsTable[current]);
            }
        }
        catch (Exception)
        {
            Debug.LogWarning("Brak polaczenia!");
        }
        return path;
    }

    /**<summary>Kontruktor</summary>*/
    public Gps()
    {
    }

    /**<summary>Zwraca liste pozycji wszystkcih skrzyzowan</summary>
     * <returns>Zwraca liste pozycji wszystkcih skrzyzowan</returns>*/
    private List<Vector2> GetAllCrossroadsPositions()
    {
        List<Vector2> theList = new List<Vector2>();

        foreach(var c in MyMap.AllCrossroads)
            theList.Add(c.Key);

        return theList;
    }
}
