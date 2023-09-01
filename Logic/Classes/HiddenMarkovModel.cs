using System;
using System.Collections.Generic;

namespace Logic.Classes
{
    public class HiddenMarkovModel
    {
        private readonly int numStates;
        private readonly int numObservations;
        private readonly double[] initialProbabilities;
        private readonly double[,] transitionProbabilities;
        private readonly double[,] emissionProbabilities;

        private double[,] alpha;
        private double[,] beta;
        private double[,] gamma;
        private double[,,] xi;

        private int numFrames;
        private List<double[]> mfccList;

        public HiddenMarkovModel(int numStates, int numObservations)
        {
            this.numStates = numStates;
            this.numObservations = numObservations;

            transitionProbabilities = InitializeMatrix(numStates, numStates);
            emissionProbabilities = InitializeMatrix(numStates, numObservations);
            initialProbabilities = InitializeArray(numStates);
        }

        #region Initialization

        private double[,] InitializeMatrix(int rows, int columns)
        {
            double[,] matrix = new double[rows, columns];
            Random random = new();

            for (int i = 0; i < rows; i++)
            {
                double[] logProbabilities = new double[columns];
                double maxLogProb = double.MinValue;

                for (int j = 0; j < columns; j++)
                {
                    double logValue = Math.Log(random.NextDouble());
                    logProbabilities[j] = logValue;
                    if (logValue > maxLogProb)
                    {
                        maxLogProb = logValue;
                    }
                }

                double sumExp = 0.0;
                for (int j = 0; j < columns; j++)
                {
                    double expLogProb = Math.Exp(logProbabilities[j] - maxLogProb);
                    matrix[i, j] = expLogProb;
                    sumExp += expLogProb;
                }

                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] /= sumExp;
                }
            }

            return matrix;
        }

        private double[] InitializeArray(int length)
        {
            double[] array = new double[length];
            Random random = new();

            double[] logProbabilities = new double[length];
            double maxLogProb = double.MinValue;

            for (int i = 0; i < length; i++)
            {
                double logValue = Math.Log(random.NextDouble());
                logProbabilities[i] = logValue;
                if (logValue > maxLogProb)
                {
                    maxLogProb = logValue;
                }
            }

            double sumExp = 0.0;
            for (int i = 0; i < length; i++)
            {
                double expLogProb = Math.Exp(logProbabilities[i] - maxLogProb);
                array[i] = expLogProb;
                sumExp += expLogProb;
            }

            for (int i = 0; i < length; i++)
            {
                array[i] /= sumExp;
            }

            return array;
        }

        #endregion

        #region Baum-Welch Algorithm

        public void BaumWelchAlgorithm(List<double[]> mfccListD)
        {
            numFrames = mfccListD.Count;
            mfccList = mfccListD;

            InitializeAlphaBetaGammaXi();

            ComputeAlpha();
            ComputeBeta();
            ComputeGamma();
            ComputeXi();

            UpdateInitialProbabilities();
            UpdateTransitionProbabilities();
            UpdateEmissionProbabilities();
        }

        private void InitializeAlphaBetaGammaXi()
        {
            alpha = new double[numFrames, numStates];
            beta = new double[numFrames, numStates];
            gamma = new double[numFrames, numStates];
            xi = new double[numFrames, numStates, numStates];
        }

        private void ComputeAlpha()
        {
            for (int state = 0; state < numStates; state++)
            {
                alpha[0, state] = Math.Log(initialProbabilities[state]) + Math.Log(emissionProbabilities[state, GetObservationIndex(mfccList[0])]);
            }

            for (int frame = 1; frame < numFrames; frame++)
            {
                for (int state = 0; state < numStates; state++)
                {
                    double maxLogSum = double.MinValue;

                    for (int prevState = 0; prevState < numStates; prevState++)
                    {
                        double logSum = alpha[frame - 1, prevState] + Math.Log(transitionProbabilities[prevState, state]);
                        if (logSum > maxLogSum)
                        {
                            maxLogSum = logSum;
                        }
                    }

                    alpha[frame, state] = maxLogSum + Math.Log(emissionProbabilities[state, GetObservationIndex(mfccList[frame])]);
                }
            }
        }

        private void ComputeBeta()
        {
            for (int state = 0; state < numStates; state++)
            {
                beta[numFrames - 1, state] = 0.0;
            }

            for (int frame = numFrames - 2; frame >= 0; frame--)
            {
                for (int state = 0; state < numStates; state++)
                {
                    double logSum = double.MinValue;

                    for (int nextState = 0; nextState < numStates; nextState++)
                    {
                        double logTerm = Math.Log(transitionProbabilities[state, nextState]) +
                                         Math.Log(emissionProbabilities[nextState, GetObservationIndex(mfccList[frame + 1])]) +
                                         beta[frame + 1, nextState];

                        if (logTerm > logSum)
                        {
                            logSum = logTerm;
                        }
                    }

                    beta[frame, state] = logSum;
                }
            }
        }

        private void ComputeGamma()
        {
            for (int frame = 0; frame < numFrames; frame++)
            {
                double frameLogSum = double.MinValue;

                for (int state = 0; state < numStates; state++)
                {
                    gamma[frame, state] = alpha[frame, state] + beta[frame, state];

                    if (gamma[frame, state] > frameLogSum)
                    {
                        frameLogSum = gamma[frame, state];
                    }
                }

                for (int state = 0; state < numStates; state++)
                {
                    gamma[frame, state] -= frameLogSum;
                    gamma[frame, state] = Math.Exp(gamma[frame, state]);
                }
            }
        }

        private void ComputeXi()
        {
            for (int frame = 0; frame < numFrames - 1; frame++)
            {
                double frameLogSum = double.MinValue;

                for (int state = 0; state < numStates; state++)
                {
                    for (int nextState = 0; nextState < numStates; nextState++)
                    {
                        xi[frame, state, nextState] = alpha[frame, state] +
                            Math.Log(transitionProbabilities[state, nextState]) +
                            Math.Log(emissionProbabilities[nextState, GetObservationIndex(mfccList[frame + 1])]) +
                            beta[frame + 1, nextState];

                        if (xi[frame, state, nextState] > frameLogSum)
                        {
                            frameLogSum = xi[frame, state, nextState];
                        }
                    }
                }

                for (int state = 0; state < numStates; state++)
                {
                    for (int nextState = 0; nextState < numStates; nextState++)
                    {
                        xi[frame, state, nextState] -= frameLogSum;
                        xi[frame, state, nextState] = Math.Exp(xi[frame, state, nextState]);
                    }
                }
            }
        }

        private void UpdateInitialProbabilities()
        {
            double[] gammaSumLog = new double[numStates];
            double maxGammaSumLog = double.MinValue;

            for (int state = 0; state < numStates; state++)
            {
                gammaSumLog[state] = gamma[0, state];
                if (gammaSumLog[state] > maxGammaSumLog)
                {
                    maxGammaSumLog = gammaSumLog[state];
                }
            }

            double sumExp = 0.0;
            for (int state = 0; state < numStates; state++)
            {
                gammaSumLog[state] -= maxGammaSumLog;
                gammaSumLog[state] = Math.Exp(gammaSumLog[state]);
                sumExp += gammaSumLog[state];
            }

            for (int state = 0; state < numStates; state++)
            {
                initialProbabilities[state] = gammaSumLog[state] / sumExp;
            }
        }

        private void UpdateTransitionProbabilities()
        {
            for (int state = 0; state < numStates; state++)
            {
                double[] gammaSumLog = new double[numFrames - 1];
                double maxGammaSumLog = double.MinValue;

                for (int frame = 0; frame < numFrames - 1; frame++)
                {
                    gammaSumLog[frame] = gamma[frame, state];
                    if (gammaSumLog[frame] > maxGammaSumLog)
                    {
                        maxGammaSumLog = gammaSumLog[frame];
                    }
                }

                double sumExp = 0.0;
                for (int frame = 0; frame < numFrames - 1; frame++)
                {
                    gammaSumLog[frame] -= maxGammaSumLog;
                    gammaSumLog[frame] = Math.Exp(gammaSumLog[frame]);
                    sumExp += gammaSumLog[frame];
                }

                for (int nextState = 0; nextState < numStates; nextState++)
                {
                    double maxXiSumLog = double.MinValue;

                    for (int frame = 0; frame < numFrames - 1; frame++)
                    {
                        double currentXiLog = xi[frame, state, nextState];
                        if (currentXiLog > maxXiSumLog)
                        {
                            maxXiSumLog = currentXiLog;
                        }
                    }

                    double sumXiExp = 0.0;
                    for (int frame = 0; frame < numFrames - 1; frame++)
                    {
                        double currentXiLog = xi[frame, state, nextState];
                        currentXiLog -= maxXiSumLog;
                        double currentXiExp = Math.Exp(currentXiLog);
                        sumXiExp += currentXiExp;
                    }

                    transitionProbabilities[state, nextState] = sumXiExp / sumExp;
                }
            }
        }

        private void UpdateEmissionProbabilities()
        {
            const double smoothingFactor = 0.01;

            for (int state = 0; state < numStates; state++)
            {
                double[] gammaSumLog = new double[numFrames];
                double maxGammaSumLog = double.MinValue;

                for (int frame = 0; frame < numFrames; frame++)
                {
                    gammaSumLog[frame] = gamma[frame, state];
                    if (gammaSumLog[frame] > maxGammaSumLog)
                    {
                        maxGammaSumLog = gammaSumLog[frame];
                    }
                }

                double sumExp = 0.0;
                for (int frame = 0; frame < numFrames; frame++)
                {
                    gammaSumLog[frame] -= maxGammaSumLog;
                    gammaSumLog[frame] = Math.Exp(gammaSumLog[frame]);
                    sumExp += gammaSumLog[frame];
                }

                for (int observation = 0; observation < numObservations; observation++)
                {
                    double sumGammaObservationExp = 0.0;

                    for (int frame = 0; frame < numFrames; frame++)
                    {
                        if (GetObservationIndex(mfccList[frame]) == observation)
                        {
                            double gammaObservationLog = gamma[frame, state];
                            gammaObservationLog -= maxGammaSumLog;
                            double gammaObservationExp = Math.Exp(gammaObservationLog);
                            sumGammaObservationExp += gammaObservationExp;
                        }
                    }

                    emissionProbabilities[state, observation] = (sumGammaObservationExp + smoothingFactor) / (sumExp + smoothingFactor * numObservations);
                }
            }
        }

        #endregion

        #region Viterbi Algorithm

        public int[] ViterbiAlgorithm(List<double[]> mfccList)
        {
            numFrames = mfccList.Count;
            this.mfccList = mfccList;

            int[] recognizedStates = new int[numFrames];
            double[,] delta = new double[numFrames, numStates];
            int[,] phi = new int[numFrames, numStates];

            InitializeDeltaPhi(delta, phi);

            ComputeDelta(delta, phi);

            recognizedStates[numFrames - 1] = FindMaxState(delta);
            recognizedStates = BacktrackPath(phi, recognizedStates);

            return recognizedStates;
        }

        private void InitializeDeltaPhi(double[,] delta, int[,] phi)
        {
            for (int state = 0; state < numStates; state++)
            {
                delta[0, state] = Math.Log(initialProbabilities[state]) + Math.Log(emissionProbabilities[state, GetObservationIndex(mfccList[0])]);
                phi[0, state] = 0;
            }
        }

        private void ComputeDelta(double[,] delta, int[,] phi)
        {
            for (int frame = 1; frame < numFrames; frame++)
            {
                for (int state = 0; state < numStates; state++)
                {
                    double maxDelta = double.MinValue;
                    int maxState = 0;

                    for (int prevState = 0; prevState < numStates; prevState++)
                    {
                        double currentDelta = delta[frame - 1, prevState] + Math.Log(transitionProbabilities[prevState, state]);
                        if (currentDelta > maxDelta)
                        {
                            maxDelta = currentDelta;
                            maxState = prevState;
                        }
                    }

                    delta[frame, state] = maxDelta + Math.Log(emissionProbabilities[state, GetObservationIndex(mfccList[frame])]);
                    phi[frame, state] = maxState;
                }
            }
        }

        private int FindMaxState(double[,] delta)
        {
            double maxDeltaFinal = double.MinValue;
            int maxStateFinal = 0;

            for (int state = 0; state < numStates; state++)
            {
                if (delta[numFrames - 1, state] > maxDeltaFinal)
                {
                    maxDeltaFinal = delta[numFrames - 1, state];
                    maxStateFinal = state;
                }
            }

            return maxStateFinal;
        }

        private int[] BacktrackPath(int[,] phi, int[] recognizedStates)
        {
            for (int frame = numFrames - 2; frame >= 0; frame--)
            {
                recognizedStates[frame] = phi[frame + 1, recognizedStates[frame + 1]];
            }
            return recognizedStates;
        }

        #endregion

        #region Kmeans

        private int GetObservationIndex(double[] mfcc)
        {
            return KMeans.PredictCluster(mfcc);
        }

        #endregion

        public double GetRecognitionScore(List<double[]> mfccFeatures)
        {
            int[] recognizedStates = ViterbiAlgorithm(mfccFeatures);
            double score = 0.0;

            for (int frame = 0; frame < mfccFeatures.Count; frame++)
            {
                int recognizedState = recognizedStates[frame];
                score += Math.Log(emissionProbabilities[recognizedState, GetObservationIndex(mfccFeatures[frame])]);
            }

            return score;
        }
    }
}