using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic.Classes
{
    public static class KMeans
    {
        private static int numClusters;
        private static List<double[]> mfccData;
        private static List<double[]> centroids;

        private static bool IsTrained;

        public static void Train(List<double[]> trainedData)
        {
            numClusters = ProjectSettings.NumClusters;
            centroids = new List<double[]>(numClusters);

            mfccData = trainedData;

            InitializeCentroids();

            bool converged = false;
            int maxIterations = 100;
            int iteration = 0;

            while (!converged && iteration < maxIterations)
            {
                List<List<double[]>> clusters = AssignToClusters();

                List<double[]> newCentroids = RecalculateCentroids(clusters);

                converged = CentroidsConverged(newCentroids);

                centroids = newCentroids;

                iteration++;
            }

            IsTrained = true;
        }

        private static void InitializeCentroids()
        {
            centroids.Clear();

            int randomIndex = new Random().Next(mfccData.Count);
            centroids.Add(mfccData[randomIndex]);

            while (centroids.Count < numClusters)
            {
                double[] distances = mfccData
                    .Select(point => MinDistanceToCentroids(point, centroids))
                    .ToArray();

                double totalDistance = distances.Sum();
                double targetDistance = new Random().NextDouble() * totalDistance;

                double cumulativeDistance = 0;
                for (int i = 0; i < distances.Length; i++)
                {
                    cumulativeDistance += distances[i];
                    if (cumulativeDistance >= targetDistance)
                    {
                        centroids.Add(mfccData[i]);
                        break;
                    }
                }
            }
        }

        private static double MinDistanceToCentroids(double[] point, List<double[]> centroids)
        {
            double minDistance = double.MaxValue;
            foreach (var centroid in centroids)
            {
                double distance = EuclideanDistance(point, centroid);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
            return minDistance;
        }

        private static double EuclideanDistance(double[] a, double[] b)
        {
            double sumSquared = 0;
            for (int i = 0; i < a.Length; i++)
            {
                double diff = a[i] - b[i];
                sumSquared += diff * diff;
            }
            return Math.Sqrt(sumSquared);
        }

        private static List<List<double[]>> AssignToClusters()
        {
            List<List<double[]>> clusters = Enumerable.Range(0, numClusters)
                .Select(_ => new List<double[]>())
                .ToList();

            foreach (var point in mfccData)
            {
                int closestCentroidIndex = ClosestCentroidIndex(point, centroids);
                clusters[closestCentroidIndex].Add(point);
            }

            return clusters;
        }

        private static int ClosestCentroidIndex(double[] point, List<double[]> centroids)
        {
            int closestIndex = 0;
            double minDistance = double.MaxValue;
            for (int i = 0; i < centroids.Count; i++)
            {
                double distance = EuclideanDistance(point, centroids[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }
            return closestIndex;
        }

        private static List<double[]> RecalculateCentroids(List<List<double[]>> clusters)
        {
            List<double[]> newCentroids = new(numClusters);

            foreach (var cluster in clusters)
            {
                if (cluster.Count == 0)
                {
                    newCentroids.Add(centroids[clusters.IndexOf(cluster)]);
                }
                else
                {
                    double[] newCentroid = new double[mfccData[0].Length];
                    for (int i = 0; i < newCentroid.Length; i++)
                    {
                        newCentroid[i] = cluster.Select(point => point[i]).Average();
                    }
                    newCentroids.Add(newCentroid);
                }
            }

            return newCentroids;
        }

        private static bool CentroidsConverged(List<double[]> newCentroids)
        {
            for (int i = 0; i < centroids.Count; i++)
            {
                double[] oldCentroid = centroids[i];
                double[] newCentroid = newCentroids[i];

                if (!oldCentroid.SequenceEqual(newCentroid))
                {
                    return false;
                }
            }
            return true;
        }

        public static int PredictCluster(double[] inputVector)
        {
            if (!IsTrained)
            {
                throw new InvalidOperationException("Model hasn't been trained yet.");
            }

            int closestCentroidIndex = ClosestCentroidIndex(inputVector, centroids);
            return closestCentroidIndex;
        }
    }
}