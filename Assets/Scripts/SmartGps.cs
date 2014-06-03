﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SmartGps : Gps
{
    private class RoadInfo
    {
        public Vector2 a;
        public Vector2 b;
        public float [] time;
        public int[] howManyTimes;

        public bool IsThisPath(Vector2 start, Vector2 end)
        {
            bool result = false;

            if ((a == start) && (b == end))
            {
                result = true;
            }
            else if ((b == start) && (a == end))
            {
                result = true;
            }

            return result;
        }

        // Konstruktor
        public RoadInfo(Vector2 start, Vector2 end, Map roadMap)
        {
            a = start;
            b = end;

            List<float> timeTemp = new List<float> ();
            List<int> howManyTimesTemp = new List<int>();

            int i;
            for (i = 0; i < Lt; ++i)
            {
                timeTemp.Add(roadMap.AllRoads[(a+b)/2f].Length / InitialSpeed); // Predkosc do ustalenia
                howManyTimesTemp.Add(0);
            }

            time = timeTemp.ToArray();
            howManyTimes = howManyTimesTemp.ToArray();
        }

        public void RememberTime(float newTime, int term)
        {
            time[term] = (time[term] * howManyTimes[term] + newTime) / (howManyTimes[term] + 1);
            ++howManyTimes[term];
        }
    }

    //Liczba pamietanych sciezek dla kazdej konfiguracji
    private static int Ls = 5;
    //Prawdopodobienstwo mutacji w %
    private static int Mp = 10;

    // Predkosc teoretyczna (w przypadku braku korkow)
    private static int InitialSpeed = 10;
    // Ilosc okresow dnia
    private static int Lt = 6;

    //Maszynka do losowania
    private System.Random rand;

    //Lista wszystkich wezlow na mapie
    private List<Vector2> AllCrossroads;
    //private int numberOfCrossroads;

    //Lista wszystkich odcinkow
    private List<RoadInfo> AllRoads;

    //Lista wszystkich pamietanych obecnie sciezek
    private List<List<Vector2>> [] AllPaths;

    //Przelicza godzine na pore dnia
    private int HourToTerm(int hour)
    {
        if ((hour >= 5) && (hour <= 7)) // rano
            return 1;
        else if ((hour >= 8) && (hour <= 10)) // do poludnia
            return 2;
        else if ((hour >= 11) && (hour <= 13)) // poludnie
            return 3;
        else if ((hour >= 14) && (hour <= 17)) // po poludniu
            return 4;
        else if ((hour >= 18) && (hour <= 22)) // wieczor
            return 5;
        else // noc
            return 0;
    }

    //Zwraca czas przejazdu jednego odcinka
    private float GetRoadTime(Vector2 a, Vector2 b, int term)
    {
        float result = 0;
        foreach (RoadInfo ri in AllRoads)
        {
            if (ri.IsThisPath(a, b))
            {
                result = ri.time[term];
                break;
            }
        }
        return result;
    }

    //Sprawdza, czy sciezka laczy podane wezly
    //Zwraca: 0 - nie laczy, 1 - laczy, 2 - laczy, ale w odwrotnej kolejnosci
    private short CmpPaths(List<Vector2>ListToCompare, Vector2 start, Vector2 end)
    {
        short result = 0;

        if ((ListToCompare[0] == start) && (ListToCompare[ListToCompare.Count - 1] == end))
        {
            result = 1;
        }
        
        if ((ListToCompare[0] == end) && (ListToCompare[ListToCompare.Count - 1] == start))
        {
            result = 2;
        }

        return result;
    }

    //Funkcja krzyzujaca dwie sciezki
    //Zwraca null, jesli sciezki nie sa mozliwe do skrzyzowania
    public List<Vector2> Crossbreed(List<Vector2> parent1, List<Vector2> parent2)
    {
        List<Vector2> child = null;

        int indexP1 = 0; // Polozenie wezla krzyzujacego u pierwszego rodzica
        int indexP2 = 0; // Polozenie wezla krzyzujacego u pierwszego rodzica
        int bestLocation = int.MaxValue; 
        int thisLocation; // Wskaznik "srednosci" polozenia wezlow
        int i, j;

        //Szukanie wezla krzyzujacego
        for (i = 0; i < parent1.Count; ++i)
        {
            for (j = 0; j < parent2.Count; ++j)
            {
                if (parent1[i] == parent2[j])
                {
                    thisLocation = Math.Abs(parent1.Count + parent2.Count - (i + j + 2) * 2);
                    if (thisLocation<bestLocation)
                    {
                        indexP1 = i;
                        indexP2 = j;
                        bestLocation = thisLocation;
                    }
                }
            }
        }

        //Krzyzowanie (jesli mozliwe)
        if ((indexP1 != 0) || (indexP2 != 0))
        {
            child = new List<Vector2>();
            if (parent1[0] != parent2[parent2.Count - 1]) // poczatek pierwszego, koniec drugiego
            {
                child.AddRange(parent1.GetRange(0, indexP1));
                child.AddRange(parent2.GetRange(indexP2, parent2.Count - indexP2));
            }
            else if (parent2[0] != parent1[parent1.Count - 1]) // poczatek drugiego, koniec pierwszego
            {
                child.AddRange(parent2.GetRange(0, indexP2));
                child.AddRange(parent1.GetRange(indexP1, parent1.Count - indexP1));
            }
            else // oba poczatki
            {
                child.AddRange(parent1.GetRange(0, indexP1));
                for (i = indexP2; i >= 0; --i)
                {
                    child.Add(parent2[i]);
                }
            }

            //Wyciecie "petelek"
            for (i = 0; i < (child.Count - 1); ++i)
            {
                for (j = i + 1; j < child.Count; ++j)
                {
                    if (child[i] == child[j])
                    {
                        child.RemoveRange(i, j - i);
                    }
                }
            }
        }

        return child;
    }

    //Funkcja wybijajaca najslabsze osobniki sposrod sciezek laczacych dwa podane wezly
    private void Select(Vector2 a, Vector2 b, int term)
    {
        List<int> indexes = new List<int>();
        List<float> times = new List<float>();
        int i, j;
        int tempIndex;
        float tempTime;

        //Wyszukanie sciezek, ktorych dotyczy proces selekcji
        for (i = 0; i<AllPaths[term].Count; ++i)
        {
            if (CmpPaths(AllPaths[term][i], a, b) != 0)
            {
                indexes.Add(i);
                times.Add(GetPathTime(AllPaths[term][i], term));
            }
        }

        //Posortowanie sciezek rosnaco wg czasow (sortowanie przez proste wstawianie)
        for (i = 1; i < indexes.Count; ++i)
        {
            if (times[i] < times[i - 1]) // Jesli czas jest mniejszy od czasu poprzednika, trzeba przestawiac
            {
                tempIndex = indexes[i];
                tempTime = times[i];
                indexes.RemoveAt(i);
                times.RemoveAt(i);

                for (j = 0; times[j] < tempTime; ++j)
                { }; // Szukanie miejsca do wstawienia elementu

                indexes.Insert(j, tempIndex);
                times.Insert(j ,tempTime);
            }
        }

        //Wziecie indeksow sciezek do odstrzalu
        indexes = indexes.GetRange(Ls, indexes.Count - Ls);

        //Posortowanie indeksow malejaco
        for (i = 1; i < indexes.Count; ++i)
        {
            if (indexes[i] > indexes[i - 1]) // Jesli czas jest mniejszy od czasu poprzednika, trzeba przestawiac
            {
                tempIndex = indexes[i];
                indexes.RemoveAt(i);

                for (j = 0; indexes[j] > tempIndex; ++j)
                { }; // Szukanie miejsca do wstawienia elementu

                indexes.Insert(j, tempIndex);
            }
        }

        //Egzekucja
        for (i = 0; i < indexes.Count; ++i)
        {
            AllPaths[term].RemoveAt(indexes[i]);
        }

    }

    //Funkcja mutujaca
    private void Mutate(List<Vector2> pth)
    {
        int mutstart, mutend;
        mutstart = rand.Next(pth.Count - 1);
        mutend = rand.Next(mutstart+1, pth.Count-1);

        Vector2 start = pth[mutstart];
        Vector2 end = pth[mutend];

        pth.RemoveRange(mutstart, mutend - mutstart + 1);
        List<Vector2> newFragment = RandomPath(start, end, pth);
        pth.InsertRange(mutstart, newFragment);
    }

    private void Generation(int term)
    {
        int i, j;
        int stop = AllPaths[term].Count;
        List<Vector2> newPath;

        //Krzyzowanie
        for (i = 1; i < stop; ++i)
        {
            //Donienie nowej sciezki bedacej krzyzowka poprzednich
            newPath = Crossbreed(AllPaths[term][i], AllPaths[term][i - 1]);
            if (newPath != null)
            {
                AllPaths[term].Add(newPath);
            }
        }

        //Selekcja
        stop = AllCrossroads.Count;
        for (i = 0; i < stop-1; ++i)
        {
            for (j = i + 1; j < stop; ++j)
            {
                Select(AllCrossroads[i], AllCrossroads[j], term);
            }
        }

        //Mutacje
        j = 100 / Mp; // Co ktora sciezka zostanie zmutowana
        i = rand.Next(j);
        stop = AllCrossroads.Count;
        while (i < stop)
        {
            Mutate(AllPaths[term][i]);
            i += j;
        }

    }

    private void StartGeneration()
    {
        int i;
        for (i = 0; i < Lt; ++i)
        {
            Generation(i);
        }
    }

    //Zwraca czas przejazdu calej sciezki
    private float GetPathTime(List<Vector2> Path, int term)
    {
        float result = 0;
        int i;
        for (i = 1; i < Path.Count; ++i)
        {
            result += GetRoadTime(Path[i - 1], Path[i], term);
        }
        return result;
    }

    //Zdajduje losowa sciezke pomiedzy danymi wezlami
    private List<Vector2> RandomPath(Vector2 start, Vector2 end, List<Vector2> bannedCrossroads)
    {
        List<Crossroads> neighbours = new List<Crossroads>();
        foreach(var c in MyMap.AllCrossroads[start].ConnectedCrossroads)
            neighbours.Add(c.Value);

        List<Vector2> result = null;

        int randValue = rand.Next(neighbours.Count);
        int neighbourIndex = randValue;
        Vector2 randomNeighbour;

        List<Vector2> newBannedCrossroads = new List<Vector2>();
        newBannedCrossroads.Add(start);

        
        if (bannedCrossroads != null)
        {
            newBannedCrossroads.AddRange(bannedCrossroads);
        }

        while (true)
        {
            randomNeighbour = neighbours[neighbourIndex].LogicPosition;
            if (!newBannedCrossroads.Contains(randomNeighbour)) // Sprawdzamy, czy element juz nie nalezy do sciezki
            {
                if (randomNeighbour == end) // Sprawdzamy, czy nie dotarlismy do celu
                {
                    result = new List<Vector2>();
                    result.Add(start);
                    result.Add(randomNeighbour);
                    break;
                }
                else // Szukamy szczescia dalej
                {
                    result = RandomPath(randomNeighbour, end, newBannedCrossroads);

                    if (result != null) // Znaleziono sciezke do celu!
                    {
                        result.Insert(0, start);
                        break;
                    }
                }
            }

            // Przejscie do kolejnego sasiada - jesli jakis niesprawdzony jeszcze istnieje
            neighbourIndex = (neighbourIndex + 1) % neighbours.Count;
            if (neighbourIndex == randValue)
                break;
        }
    

        return result;
    }

    private void ReverseRoadSearcher(Vector2 from)
    {
        foreach (var nb in MyMap.AllCrossroads[from].ConnectedCrossroads)
        {
            //Sprawdzanie, czy taka droga jest juz na liscie
            bool exists = false;
            foreach (RoadInfo ri in AllRoads)
            {
                if (ri.IsThisPath(from, nb.Value.LogicPosition))
                {
                    exists = true;
                }
            }

            if (exists == false)
            {
                AllRoads.Add(new RoadInfo(from, nb.Value.LogicPosition, MyMap));
                ReverseRoadSearcher(nb.Value.LogicPosition);
            }
        }
    }

    private List<Vector2> FindBestPath(Vector2 start, Vector2 end, int term)
    {
        List<Vector2> best = null;
        float bestTime = float.MaxValue;
        float time;
        short cmpResult;
        int debug = 0;

        foreach (List<Vector2> pth in AllPaths[term])
        {
            cmpResult = CmpPaths(pth, start, end);
            if (cmpResult != 0)
            {
                ++debug;
                time = GetPathTime(pth, term);
                if (time < bestTime)
                {
                    bestTime = time;
                    if (cmpResult == 1)
                    {
                        best = pth;
                    }
                    else
                    {
                        //Odwrocenie sciezki
                        best = new List<Vector2>();
                        int i;
                        for (i = pth.Count; i > 0; --i)
                        {
                            best.Add(pth[i - 1]);
                        }
                    }
                }
            }
        }
        
        return best;
    }

    //Ladowanie mapy
    override public void LoadMap(Map mapToLoad)
    {
        // Komora maszyny losujacej jest pusta...
        rand = new System.Random(); 

        // Ladowanie skrzyzowan
        MyMap = mapToLoad;
        AllCrossroads = new List<Vector2>();
        foreach(var c in MyMap.AllCrossroads)
            AllCrossroads.Add(c.Value.LogicPosition);
            
        //numberOfCrossroads = AllCrossroads.Count;

        int i, j, k;

        // Ladowanie sciezek
        List<List<Vector2>> AllPathsTemp = new List<List<Vector2>>();

        for (i = 0; i < (AllCrossroads.Count - 1); ++i)
        {
            for (j = i + 1; j < AllCrossroads.Count; ++j)
            {
                //Znajdywanie kilku losowych sciezek pomiedzy wezlami "i" i "j"
                for (k = 0; k < Ls; k++)
                {
                    AllPathsTemp.Add(RandomPath(AllCrossroads[i], AllCrossroads[j], null));
                }
            }
        }

        List<List<List<Vector2>>> AllPathsStart = new List<List<List<Vector2>>>();
        for (i = 0; i < Lt; ++i)
        {
            // Kopia poczatkowej listy dla kazdego przedzialu czasu
            AllPathsStart.Add(new List<List<Vector2>>(AllPathsTemp));
        }
        AllPaths = AllPathsStart.ToArray();

        // Ladowanie odcinkow
        AllRoads = new List<RoadInfo>();
        ReverseRoadSearcher(AllCrossroads[0]);

        // Trzy pokolenia na start
        StartGeneration();
        StartGeneration();
        StartGeneration();
    }

    // Zapamietuje czas przejazdu po danym odcinku
    // start, end - poczatek i koniec odcinka
    // time - czas przejazdu odcinka
    // hour - godzina przejazdu (0-23)
    // endOfPath - czy to byl ostatni odcinek w sciezce (jesli tak, trzeba zrobic kolejne pokolenie)
    public void RememberTime(Vector2 start, Vector2 end, float time, int hour, bool endOfPath)
    {
        foreach (RoadInfo ri in AllRoads)
        {
            if (ri.IsThisPath(start, end))
            {
                ri.RememberTime(time, HourToTerm(hour));
                break;
            }
        }

        if (endOfPath)
        {
            Generation(HourToTerm(hour));
            Generation(HourToTerm(hour));
        }
    }

    // Tu bedzie inteligentny algorytm
    public List<Vector2> FindPath(Vector2 start, Vector2 end, int hour)
    {
        return FindBestPath(start, end, HourToTerm(hour));
    }

    /* ***********************************************************************************
     *                   FUNKCJE ZWIAZANE Z ZAPISEM I ODCZYTEM Z PLIKU
     * *********************************************************************************** */
    public void SaveToFile(FileStream Stream)
    {
        int i, j;

        //Zapamietanie listy wezlow
        Stream.Write(BitConverter.GetBytes(AllRoads.Count), 0, sizeof(int)); // Liczba wezlow
        foreach(RoadInfo r in AllRoads)
        {
            Stream.Write(BitConverter.GetBytes(r.a.x), 0, sizeof(float)); // Pierwsza wspolrzedna pierwszego punktu
            Stream.Write(BitConverter.GetBytes(r.a.y), 0, sizeof(float)); // Druga wspolrzedna pierwszego punktu
            Stream.Write(BitConverter.GetBytes(r.b.x), 0, sizeof(float)); // Pierwsza wspolrzedna drugiego punktu
            Stream.Write(BitConverter.GetBytes(r.b.y), 0, sizeof(float)); // Druga wspolrzedna drugiego punktu
            for(i = 0; i < Lt; ++i)
            {
                Stream.Write(BitConverter.GetBytes(r.time[i]), 0, sizeof(float));
                Stream.Write(BitConverter.GetBytes(r.howManyTimes[i]), 0, sizeof(int));
            }
        }

        //Zapamietanie listy sciezek
        for(i = 0; i < Lt; ++i)
        {
            Stream.Write(BitConverter.GetBytes(AllPaths[i].Count), 0, sizeof(int)); // Liczba sciezek
            foreach(List<Vector2> p in AllPaths[i])
            {
                Stream.Write(BitConverter.GetBytes(p.Count), 0, sizeof(int)); // Liczba wezlow
                for(j = 0; j < p.Count; ++j)
                {
                    Stream.Write(BitConverter.GetBytes(p[j].x), 0, sizeof(float)); // Pierwsza wspolrzedna punktu
                    Stream.Write(BitConverter.GetBytes(p[j].y), 0, sizeof(float)); // Druga wspolrzedna punktu
                }
            }
        }
    }

    public void LoadFromFile(FileStream Stream, Map NewMap)
    {
        int i, j, k;

        //Zmienne - bufory
        byte[] buf = new byte[10];
        float a, b, c, d;
        int number, number2;
        RoadInfo temp;
        List<List<Vector2>> paths;
        List<Vector2> tempPath;

        //Zapamietanie referencji do mapy
        MyMap = NewMap;

        //Odpowiednik konstruktora
        AllRoads = new List<RoadInfo>();
        AllPaths = new List<List<Vector2>>[Lt];

        rand = new System.Random();
        AllCrossroads = new List<Vector2>();
        foreach(var cross in MyMap.AllCrossroads)
            AllCrossroads.Add(cross.Value.LogicPosition);

        //Wczytanie listy wezlow
        Stream.Read(buf, 0, sizeof(int)); //Liczba wezlow
        number = BitConverter.ToInt32(buf, 0);

        for(i = 0; i < number; ++i)
        {
            Stream.Read(buf, 0, sizeof(float));
            a = BitConverter.ToSingle(buf, 0); // Pierwsza wspolrzedna pierwszego punktu
            Stream.Read(buf, 0, sizeof(float));
            b = BitConverter.ToSingle(buf, 0); // Druga wspolrzedna pierwszego punktu
            Stream.Read(buf, 0, sizeof(float));
            c = BitConverter.ToSingle(buf, 0); // Pierwsza wspolrzedna drugiego punktu
            Stream.Read(buf, 0, sizeof(float));
            d = BitConverter.ToSingle(buf, 0); // Druga wspolrzedna drugiego punktu

            temp = new RoadInfo(new Vector2(a, b), new Vector2(c, d), NewMap);
            for(j = 0; j < Lt; ++j)
            {
                Stream.Read(buf, 0, sizeof(float));
                temp.time[j] = BitConverter.ToSingle(buf, 0); // Czas przejazdu
                Stream.Read(buf, 0, sizeof(int));
                temp.howManyTimes[j] = BitConverter.ToInt32(buf, 0); // Liczba "czasow"
            }

            AllRoads.Add(temp);
        }

        //Odczytanie listy sciezek
        for(i = 0; i < Lt; ++i)
        {
            paths = new List<List<Vector2>>();

            Stream.Read(buf, 0, sizeof(int));
            number = BitConverter.ToInt32(buf, 0); // Liczba sciezek

            for(j = 0; j < number; ++j)
            {
                tempPath = new List<Vector2>();

                Stream.Read(buf, 0, sizeof(int));
                number2 = BitConverter.ToInt32(buf, 0); // Liczba wezlow

                // Sklejanie sciezki
                for(k = 0; k < number2; ++k)
                {
                    Stream.Read(buf, 0, sizeof(float));
                    a = BitConverter.ToSingle(buf, 0); // Pierwsza wspolrzedna pierwszego punktu
                    Stream.Read(buf, 0, sizeof(float));
                    b = BitConverter.ToSingle(buf, 0); // Druga wspolrzedna pierwszego punktu

                    tempPath.Add(new Vector2(a, b));
                }

                paths.Add(tempPath);
            }

            AllPaths[i] = paths;
        }
    }
}
