using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using FANN.Net;

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

        //Predkosc agenta (do ustalenia)
        public const int Speed = 10;

        //Konstruktor
        public PathListElement(List<Vector2> PathToAdd, Map MyMap)
        {
            TimeOf = new float[NumberOfTimes];
            NumberOfWays = new int[NumberOfTimes];
            Path = PathToAdd;

            int i;

            //Obliczenie teoretycznego czasu przejazdu po danej sciezce
            float length = 0;
            for (i = 1; i < Path.Count; ++i)
            {
                length += MyMap.GetRoadLength(Path[i - 1], Path[i]);
            }
            float time = length / Speed;

            //Inicjalizacja czasow przejazdu
            for (i = 0; i < NumberOfTimes; ++i)
            {
                TimeOf[i] = time;
                NumberOfWays[i] = 1;
            }
        }
    }

    //Liczba istniejacych przedzialow czasu
    private static int NumberOfTimes = 6;

    //Lista wszystkich wezlow na mapie
    private List<Vector2> AllCrossroads;
    //Lista wszystkich sensownych sciezek
    private List<PathListElement> AllPaths;
    //Lista tymczasowa (na brudno)
    private List<PathListElement> TemporaryPaths;

    //Siec neuronowa
    private NeuralNet Fann;   

    // Nazwa pliku, w ktorym beda dane do "pierwszej lekcji" dla sieci neuronowej
    String LearningFile;

    //Zwraca szacowany czas przejazdu najszybsza sciezka pomiedzy danymi wezlami o okreslonej porze dni
    private float FindBestTime(Vector2 cross1, Vector2 cross2, int term)
    {
        float best = float.MaxValue;

        for (int i = 1; i < AllPaths.Count; ++i)
        {
            if ((((AllPaths[i].Path[0] == cross1) && (AllPaths[i].Path[AllPaths[i].Path.Count - 1] == cross2)) || ((AllPaths[i].Path[0] == cross2) && (AllPaths[i].Path[AllPaths[i].Path.Count - 1] == cross1))) &&
                (AllPaths[i].TimeOf[term] < best))
            {
                best = AllPaths[i].TimeOf[term];
            }
            
        }

        return best;
    }

    //Tworzy wpis dla pliku uczacego dodtyczacy polaczenia miedzy dwoma konkretnymi wezlami
    //cross1, cross2 - wezly do polaczenia
    //term - pora dnia
    public String PrepareRaportForCrossings(Vector2 cross1, Vector2 cross2, int term)
    {
        String raport;
        int i;

        //Blok danych wejsciowych
        raport = ((float)term/(float)NumberOfTimes).ToString() + ' ';

        for (i = 0; i < AllCrossroads.Count; ++i)
        {
            if ((AllCrossroads[i] == cross1) || (AllCrossroads[i] == cross2))
            {
                raport += "1 ";
            }
            else
            {
                raport += "0 ";
            }
        }

        raport += "\n\n";

        //Blok danych wyjsciowych
        float bestTime = FindBestTime(cross1, cross2, term);
        float temporary;
        int numberPaths = AllPaths.Count;

        for(i = 0; i<numberPaths; ++i)
        {
            if (((AllPaths[i].Path[0] == cross1) && (AllPaths[i].Path[AllPaths[i].Path.Count-1] == cross2)) || ((AllPaths[i].Path[0] == cross2) && (AllPaths[i].Path[AllPaths[i].Path.Count-1] == cross1)))
            {
                //Ta sciezka laczy te wezly, co potrzeba
                temporary = bestTime / AllPaths[i].TimeOf[term];
            }
            else
            {
                temporary = 0;
            }
            raport = raport + temporary.ToString() + ' ';
        }

        raport += "\n\n";

        return raport;
    }

    //Funkcja wywolywana wewnatrz funkcji "LoadMap"
    private void RecursiveSearcher(List<Vector2> currentPath, Vector2 lastCrossing, Vector2 end)
    {
        currentPath.Add(lastCrossing);

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
                    List<Vector2> nextPath = new List<Vector2>();
                    nextPath.AddRange(currentPath);
                    RecursiveSearcher(nextPath, c, end);
                }
            }
        }
    }

    //Ładowanie mapy
    override public void LoadMap(Map mapToLoad)
    {
        MyMap = mapToLoad;
        AllCrossroads = MyMap.GetAllCrossings();

        AllPaths = new List<PathListElement>();
        int i, j, k;

        // Znajdywanie wszystkich sciezek
        for (i = 0; i < (AllCrossroads.Count - 1); ++i)
        {
            for (j = i + 1; j < AllCrossroads.Count; ++j)
            {
                //Znajdywanie wszystkich sciezek pomiedzy wezlami "i" i "j"
                TemporaryPaths = new List<PathListElement>();
                List<Vector2> currentPath = new List<Vector2>();
                RecursiveSearcher(currentPath, AllCrossroads[i], AllCrossroads[j]);

                //TODO: Powywalanie bazsensownych sciezek (wielokrotnie dluzszych od najkrotszej), jesli czas wykonywania obliczen bedzieliczony w tygodniach

                //Dorzucenie znalezionych sciezek do ogolnej listy
                AllPaths.AddRange(TemporaryPaths);
            }
        };

        //Przygotowanie pliku uczacego
        LearningFile = "FirstLesson";
        FileStream learn = new FileStream(LearningFile, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter learnStream = new StreamWriter(learn);

        int n = AllCrossroads.Count; // Liczba skrzyzowan
        int p = AllPaths.Count; // Liczba sciezek
        learnStream.Write((n * (n - 1) * 3).ToString() + " " + (n + 1).ToString() + " " + p.ToString() + "\n\n");

        for (i = 0; i < (AllCrossroads.Count - 1); ++i)
        {
            for (j = i + 1; j < AllCrossroads.Count; ++j)
            {
                for (k = 0; k < NumberOfTimes; ++k)
                {
                    learnStream.Write(PrepareRaportForCrossings(AllCrossroads[i], AllCrossroads[j], k));
                }
                
            }
        };

        learnStream.Close();

        Fann = new NeuralNet();
        List<uint> Layers = new List<uint>();
        Layers.Add((uint)(n+1)); // Neurony wejsciowe
        Layers.Add((uint)((n+p)/2)); // Neurony ukryte
        Layers.Add((uint)p); // Neurony ukryte
        Fann.CreateStandardArray(Layers.ToArray());
        TrainingData training = new TrainingData();
        training.ReadTrainFromFile(LearningFile);
        Fann.SaveToFixed("Fann.data");

    }

    //FUNKCJA TYLKO DLA OBWODU STRERUJACAGO!
    public String PrintRaport()
    {
        String rap = "";

        foreach (PathListElement pth in AllPaths)
        {
            String p = "";
            for (int i = 0; i < pth.Path.Count; ++i)
            {
                p += "(";
                p += pth.Path[i].x.ToString();
                p += ", ";
                p += pth.Path[i].y.ToString();
                p += ")";
            }

            rap = rap + p + "\t" + pth.TimeOf[0].ToString() + "\n";
        }

        return rap;
    }

    // Tu bedzie inteligentny algorytm
    override public List<Vector2> FindPath(Vector2 start, Vector2 end, int term)
    {
        List<Vector2> Sciezka;
        int i;
        int maxIndex = 0;
        double max = 0.0;
        double[] outputData;

        List<double> inputData = new List<double>();
        inputData.Add((float)term / (float)NumberOfTimes);

        for (i = 0; i < AllCrossroads.Count; ++i)
        {
            if ((AllCrossroads[i] == start) || (AllCrossroads[i] == end))
            {
                inputData.Add(1.0);
            }
            else
            {
                inputData.Add(0.0);
            }
        }

        outputData = Fann.Run(inputData.ToArray());

        for (i = 0; i < AllPaths.Count; ++i)
        {
            System.Console.Out.Write("{0} ", outputData[i]);
            if (outputData[i] > max)
            {
                max = outputData[i];
                maxIndex = i;
            }
        }

        Sciezka = AllPaths[maxIndex].Path;
        
        //TODO: Odwrocic kolejnosc wezlow jesli potrzeba.

        return Sciezka;
    }
}
