using System;
using System.Collections.Generic;
using System.Linq;

namespace Logic.Classes
{
    public class GaussianMixtureModel
    {
        private readonly int numGaussians;
        private readonly int vectorSize;

        private List<double> weights;
        private List<double[]> means;
        private List<double[]> covariances;

        private List<double[]> mfccData;
        private List<double[]> responsibilitiesList;

        public GaussianMixtureModel(int numGaussians, int vectorSize, List<double[]> trainingData)
        {
            this.numGaussians = numGaussians;
            this.vectorSize = vectorSize;
            mfccData = trainingData;
            InitializeParameters();
            Train();
        }

        private void InitializeParameters()
        {
            // Implement with KMeans
        }

        public void Train()
        {
            int maxIterations = 250;
            double convergenceThreshold = 1e-6; // Adjust as needed

            double previousLogLikelihood = double.NegativeInfinity;

            for (int i = 0; i < maxIterations; i++)
            {
                EStep();
                MStep();

                double currentLogLikelihood = ComputeLogLikelihood();

                // Check if the change in log-likelihood is below the threshold
                if (Math.Abs(currentLogLikelihood - previousLogLikelihood) < convergenceThreshold)
                {
                    Console.WriteLine($"Converged after {i + 1} iterations.");
                    break;
                }

                previousLogLikelihood = currentLogLikelihood;
            }

            int a = 3;
        }

        private void EStep()
        {
            responsibilitiesList = new List<double[]>();

            foreach (double[] vector in mfccData)
            {
                double[] responsibilities = new double[numGaussians];

                for (int i = 0; i < numGaussians; i++)
                {
                    double logDistributionDensity = CalculateGaussianLogDensity(vector, means[i], covariances[i]);
                    double logLikelihood = Math.Log(weights[i]) + logDistributionDensity;

                    responsibilities[i] = logLikelihood;
                }

                double maxLogResponsibility = responsibilities.Max();
                double sumExpAdjustedResponsibilities = responsibilities.Select(r => Math.Exp(r - maxLogResponsibility)).Sum();

                responsibilities = responsibilities.Select(r => Math.Exp(r - maxLogResponsibility) / sumExpAdjustedResponsibilities).ToArray();

                double minResponsibility = 1e-300;
                responsibilities = responsibilities.Select(r => Math.Max(r, minResponsibility)).ToArray();

                responsibilitiesList.Add(responsibilities);
            }
        }

        private double CalculateGaussianLogDensity(double[] x, double[] mean, double[] covariance)
        {
            double[] diff = new double[x.Length];
            for (int j = 0; j < x.Length; j++)
            {
                diff[j] = x[j] - mean[j];
            }

            double[] invCovariance = new double[covariance.Length];
            for (int j = 0; j < covariance.Length; j++)
            {
                invCovariance[j] = 1.0 / covariance[j];
            }

            double exponent = 0;
            for (int j = 0; j < x.Length; j++)
            {
                exponent += diff[j] * diff[j] * invCovariance[j];
            }

            double logDensity = -0.5 * exponent - 0.5 * x.Length * Math.Log(2 * Math.PI) - 0.5 * covariance.Select(c => Math.Log(c)).Sum();

            return logDensity;
        }


        private void MStep()
        {
            // Update weights
            for (int i = 0; i < numGaussians; i++)
            {
                double totalResponsibility = 0.0;
                for (int j = 0; j < mfccData.Count; j++)
                {
                    totalResponsibility += responsibilitiesList[j][i];
                }
                weights[i] = totalResponsibility / mfccData.Count;
            }

            // Update means
            for (int i = 0; i < numGaussians; i++)
            {
                double[] updatedMean = new double[vectorSize];
                double totalResponsibility = 0.0;

                for (int j = 0; j < mfccData.Count; j++)
                {
                    double responsibility = responsibilitiesList[j][i];
                    totalResponsibility += responsibility;

                    for (int k = 0; k < vectorSize; k++)
                    {
                        updatedMean[k] += responsibility * mfccData[j][k];
                    }
                }

                for (int k = 0; k < vectorSize; k++)
                {
                    means[i][k] = updatedMean[k] / totalResponsibility;
                }
            }

            // Update covariances
            for (int i = 0; i < numGaussians; i++)
            {
                double[] updatedCovariance = new double[vectorSize];
                double totalResponsibility = 0.0;

                for (int j = 0; j < mfccData.Count; j++)
                {
                    double responsibility = responsibilitiesList[j][i];
                    totalResponsibility += responsibility;

                    for (int k = 0; k < vectorSize; k++)
                    {
                        double diff = mfccData[j][k] - means[i][k];
                        updatedCovariance[k] += responsibility * (diff * diff);
                    }
                }

                for (int k = 0; k < vectorSize; k++)
                {
                    covariances[i][k] = updatedCovariance[k] / totalResponsibility;
                }
            }
        }

        private double ComputeLogLikelihood()
        {
            double logLikelihood = 0.0;

            foreach (double[] vector in mfccData)
            {
                double vectorLogLikelihood = 0.0;

                for (int i = 0; i < numGaussians; i++)
                {
                    double logDistributionDensity = CalculateGaussianLogDensity(vector, means[i], covariances[i]);
                    double weightedLogLikelihood = Math.Log(weights[i]) + logDistributionDensity;

                    vectorLogLikelihood = LogSumExp(vectorLogLikelihood, weightedLogLikelihood);
                }

                logLikelihood += vectorLogLikelihood;
            }

            return logLikelihood;
        }

        private double LogSumExp(double x, double y)
        {
            if (x == double.NegativeInfinity) return y;
            if (y == double.NegativeInfinity) return x;

            double minValue = Math.Min(x, y);
            return minValue + Math.Log(Math.Exp(x - minValue) + Math.Exp(y - minValue));
        }

        public int PredictClusterIndex(double[] vector)
        {
            double[] responsibilities = new double[numGaussians];

            for (int i = 0; i < numGaussians; i++)
            {
                double logDistributionDensity = CalculateGaussianLogDensity(vector, means[i], covariances[i]);
                double logLikelihood = Math.Log(weights[i]) + logDistributionDensity;

                responsibilities[i] = logLikelihood;
            }

            double maxLogResponsibility = responsibilities.Max();
            double sumExpAdjustedResponsibilities = responsibilities.Select(r => Math.Exp(r - maxLogResponsibility)).Sum();

            responsibilities = responsibilities.Select(r => Math.Exp(r - maxLogResponsibility) / sumExpAdjustedResponsibilities).ToArray();

            int predictedClusterIndex = Array.IndexOf(responsibilities, responsibilities.Max());

            return predictedClusterIndex;
        }

        public (List<double> weights, List<double[]> means, List<double[]> covariances) GetGaussianParameters()
        {
            return (weights, means, covariances);
        }
    }
}