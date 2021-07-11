using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Knn
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> l1 = DataParser.ReadCSV(@"C:\Users\rogo9\Downloads\iris.csv");
            List<double[]> l2 = DataParser.ParseData(l1);
            List<double[]> l3 = KNN.MinMax(l2);
            KNN.Normalize(l2, l3);
            Console.WriteLine("Acc :"+ KNN.Eval(l2, 3, 5));
        }
    }
    class KNN { 
        public static List<double[]> MinMax(List<double[]> set)
        {
            List<double[]> output = new List<double[]>();
            for (int i = 0;i < set[0].Length - 1; i++)
            {
                double[] colVal = new double[set.Count];
                for (int j = 0;j < set.Count; j++)
                {
                    colVal[j] += set[j][i];
                }
                output.Add(new double[] { colVal.Min(), colVal.Max() });
                Console.WriteLine("Min: " + colVal.Min() + " ,max: " + colVal.Max());
            }
            return output;
        }

        public static void Normalize(List<double[]> set, List<double[]> minmax)
        {
            foreach(var i in Enumerable.Range(0, set.Count))
            {
                foreach(var j in Enumerable.Range(0,set[0].Length - 1))
                {
                    set[i][j] = (set[i][j] - minmax[j][0]) / (minmax[j][1] - minmax[j][0]);
                }
            }
        }

        public static double L2(double[] l1,double[] l2)
        {
            double Sum = 0.0;
            foreach(var i in Enumerable.Range(0, l1.Length - 1))
            {
                Sum += Math.Pow(l1[i] - l2[i], 2);
            }
            return Math.Sqrt(Sum);
        }

        public static List<double> Neighbors(List<double[]> train, double[] test_row, int k)
        {
            var distance = new List<double>();
            foreach(var train_row in train)
            {
                var dist = L2(train_row, test_row);
                distance.Add(dist);
            }
            distance.Sort();
            var neighbors = new List<double>();
            foreach(var i in Enumerable.Range(0, k))
            {
                neighbors.Add(distance[i]);
            }
            return neighbors;
        }

        public static List<double[]> CVS(List<double[]> dataset,int n_folds)
        {
            List<double[]> dataset_split = new List<double[]>();
            List<double[]> dataset_copy = dataset.ToList();
            int fold_size = Convert.ToInt32(dataset.Count()/n_folds);
            Random rng = new Random();
            foreach (var _ in Enumerable.Range(0, n_folds))
            {
                List<double[]> fold = new List<double[]>();
                while(fold.Count < fold_size)
                {
                    int r = rng.Next(0, dataset_copy.Count);
                    fold.Add(dataset_copy[r]);
                    dataset_copy.RemoveAt(r);
                }
                dataset_split.AddRange(fold);
            }
            return dataset_split;
        }
        public static double Predict(List<double[]> n1,double[] n2,int k)
        {
            List<double> neighbors = Neighbors(n1, n2, k);
            var output_values = (from row in neighbors
                                 select neighbors.Last()).ToList();
            HashSet<double> tmp = new HashSet<double>(output_values);
            double prediction = tmp.Max();
            Console.WriteLine("Prediction : " + prediction);
            return prediction;
        }
        public static List<double[]> KNNClas(List<double[]> train, List<double[]> test,int k)
        {
            List<double[]> pred = new List<double[]>();
            foreach(var row in test)
            {
                double output = Predict(train, row, k);
                double[] outArr = new double[1];
                outArr.Append(output);
                pred.Add(outArr);
            }
            return pred;
        }
        public static double Eval(List<double[]> list,int k_folds,int k)
        {   
            List<double[]> copy = new List<double[]>(list);
            List<double[]> train_set = new List<double[]>(30);
            List<double[]> test_set = new List<double[]>();
            for(int i = 0;i < 30; i++)
            {
                train_set.Add(copy[i]);
                copy.Remove(copy[i]);
            }
            for(int i = 0;i < copy.Count; i++)
            {
                test_set.Add(copy[i]);
            }
            List<double[]> predicted = KNNClas(train_set, test_set, k);
            double scores = Metric(test_set, predicted);
            return scores;
        }
        public static double Metric(List<double[]> n1, List<double[]> n2)
        {
            int correct = 0;
            foreach(var i in Enumerable.Range(0, n1.Count))
            {
                Console.WriteLine(n1[i][4] + " " + n2[i][0]);
                if (n1[i][4] == n2[i][0])
                {
                    correct += 1;
                }
            }
            return correct;
        }
    }
    class DataParser
    {
        public static List<string> ReadCSV(string path)
        {
            if (File.Exists(path))
            {
                var nullifycomma = CultureInfo.InvariantCulture;
                List<string> cont = new List<string>();
                StreamReader file = new StreamReader(path);
                while (!file.EndOfStream)
                {
                    //read and split lines
                    string line = file.ReadLine();
                    cont.Add(line);
                }
                return cont;
            }
            else
            {
                return null;
            }
        }
        public static List<double[]> ParseData(List<string> input)
        {
            List<double[]> output = new List<double[]>();
            for(int i = 0;i < input.Count; i++)
            {
                double[] res = new double[5];
                string line = input[i].ToString();
                string[] split = line.Split(';').ToArray();
                int type = 0;
                if (split[4].Equals("Iris-setosa"))
                {
                    type = 0;
                }
                else if (split[4].Equals("Iris-versicolor"))
                {
                    type = 1;
                }
                else if (split[4].Equals("Iris-virginica"))
                {
                    type = 2;
                }
                res[0] = Double.Parse(split[0], CultureInfo.InvariantCulture);
                res[1] = Double.Parse(split[1], CultureInfo.InvariantCulture);
                res[2] = Double.Parse(split[2], CultureInfo.InvariantCulture);
                res[3] = Double.Parse(split[3], CultureInfo.InvariantCulture);
                res[4] = type;
                output.Add(res);
            }
            return output;
        }
        public static void Print(List<double[]> l1)
        {
            for (int i = 0; i < l1.Count; i++)
            {
                Console.WriteLine("SL: " + l1[i][0] + " ,SW: " + l1[i][1]+ " ,PL: "+ l1[i][2] + " ,PW: " + 
                    l1[i][3] + " ,type: " + l1[i][4]);
            }
        }
    }
}
