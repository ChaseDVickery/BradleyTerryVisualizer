using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
// using Unity.Barracuda;

namespace Distributions {
    public class BradleyTerryDistribution
    {
        private List<(int,int)> prefs;
        private Matrix<double> trajs;
        private Matrix<double> trajLens;
        private Matrix<double> deltas;
        private Matrix<double> psis;
        private bool includeDeltas;

        private bool _dirtyCache = true;
        private Matrix<double> cachedDeltas;
        private Matrix<double> tempDeltas;

        private Matrix<double> _allDeltas;
        private Matrix<double> allDeltas {
            get {
                if (_dirtyCache) {
                    _dirtyCache = false;
                    _allDeltas = GetCalculatedDeltas();
                    if (deltas != null) {
                        if (allDeltas == null) {
                            _allDeltas = Matrix<double>.Build.DenseOfMatrix(deltas);
                        }
                        else {
                            _allDeltas = deltas.Stack(_allDeltas);
                        }
                    }
                    if (tempDeltas != null) {
                        if (_allDeltas == null) {
                            _allDeltas = Matrix<double>.Build.DenseOfMatrix(tempDeltas);
                        } else {
                            _allDeltas = _allDeltas.Stack(tempDeltas);
                        }
                    }
                }
                return _allDeltas;
            }
        }
        private int tempDeltaRowStart {
            get {
                int idx = 0;
                Matrix<double> trajDeltas = GetCalculatedDeltas();
                if (trajDeltas != null) {
                    idx += trajDeltas.RowCount;
                }
                if (deltas != null) {
                    idx += deltas.RowCount;
                }
                return idx;
            }
        }

        private int numFeatures = 1;

        public BradleyTerryDistribution(int nFeatures) {
            numFeatures = nFeatures;
            psis = Matrix<double>.Build.Dense(0,numFeatures);
            deltas = Matrix<double>.Build.Dense(0,numFeatures);
            prefs = new List<(int,int)>();
        }

        private Matrix<double> CreatePsis(Matrix<double> trajectories, Matrix<double> trajLens) {
            return Matrix<double>.Build.Random(5,2);
        }
        
        private void UpdateTrajectories(Matrix<double> trajectories, Matrix<double> trajLens) {
            this.trajs = trajectories;
            this.trajLens = trajLens;
            this.psis = this.CreatePsis(trajectories, trajLens);
            this._dirtyCache = true;
        }

        public void AddDeltas(Matrix<double> newDeltas) {
            if (deltas == null) { deltas = Matrix<double>.Build.DenseOfMatrix(newDeltas); return; }
            deltas = newDeltas.Stack(deltas);
            _dirtyCache = true;
        }

        public void SetDeltas(Matrix<double> newDeltas) {
            deltas = newDeltas;
            _dirtyCache = true;
        }

        public void OverwriteTempDeltas(double[,] temps) {
            if (tempDeltas == null) {
                SetTempDeltas(Matrix<double>.Build.DenseOfArray(temps));
                return;
            }

            if (temps.GetLength(0) != tempDeltas.RowCount) {
                Debug.LogError($"Given temp deltas have count {temps.GetLength(0)} while current temp deltas have count {tempDeltas.RowCount}");
                return;
            }
            // Copy values into the existing matrix
            for (int i = 0; i < temps.GetLength(0); i++) {
                for (int j = 0; j < temps.GetLength(1); j++) {
                    tempDeltas.At(i, j, temps[i,j]);
                    allDeltas.At(i+tempDeltaRowStart, j, temps[i,j]);
                    // tempDeltas.SetRow(i, temps[i]);
                }
            }
            // 
        }
        public void SetTempDeltas(Matrix<double> temps) {
            tempDeltas = Matrix<double>.Build.DenseOfMatrix(temps);
            _dirtyCache = true;
        }
        public void ClearTempDeltas() {
            tempDeltas = null;
            _dirtyCache = true;
        }

        public void ClearDeltas() {
            deltas = null;
            _dirtyCache = true;
        }

        public Matrix<double> LogProb(Matrix<double> particles) {
            Matrix<double> ll = ComputeLogLikelihood(particles);
            if (ll == null) { return null; }
            return ll;
        }
        public Matrix<double> Prob(Matrix<double> particles) {
            Matrix<double> ll = ComputeLogLikelihood(particles);
            if (ll == null) { return null; }
            return Matrix<double>.Exp(ll);
        }

        private Matrix<double> GetCalculatedDeltas() {
            // if (cachedDeltas == null || _dirtyCache) {
            //     return null;
            // }
            // _dirtyCache = false;
            return null;
        }

        public Matrix<double> GetPrefLikelihoodDeltas(Matrix<double> d, Matrix<double> w) {
            // d (deltas): [# preferences x # features]
            // w: [# particles x # features]
            // w*deltas.T: [# particles, # preferences]
            Matrix<double> result = 1f / (1.0f + Matrix<double>.Exp(w.TransposeAndMultiply(d)));
            return result;
        }

        // Computes the log-likelihood for each particle using the current distribution
        public Matrix<double> ComputeLogLikelihood(Matrix<double> particles) {
            // Combine preferences with current deltas
            // Matrix<double> allDeltas = GetCalculatedDeltas();
            // if (allDeltas == null) {
            //     allDeltas = Matrix<double>.Build.DenseOfMatrix(deltas);
            // }
            // else {
            //     allDeltas = deltas.Stack(allDeltas);
            // }
            // if (tempDeltas != null) {
            //     allDeltas = allDeltas.Stack(tempDeltas);
            // }
            if (allDeltas == null || particles == null) { return null; }
            // Perform calculation
            Matrix<double> jointLogprob = Matrix<double>.Build.DenseOfRowArrays(
                Matrix<double>.Log(GetPrefLikelihoodDeltas(allDeltas, particles))
                .RowSums()  // Vector
                .Storage    // Vector data
                .AsArray()  // as array of values
            );
            return jointLogprob;
        }



        
    }
}