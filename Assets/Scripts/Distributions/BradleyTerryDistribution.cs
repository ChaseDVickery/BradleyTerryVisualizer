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

        public void SetTempDeltas(Matrix<double> temps) {
            tempDeltas = Matrix<double>.Build.DenseOfMatrix(temps);
        }
        public void ClearTempDeltas() {
            tempDeltas = null;
        }

        public void ClearDeltas() {
            deltas = null;
            _dirtyCache = true;
        }

        public Matrix<double> LogProb(Matrix<double> particles) {
            return ComputeLogLikelihood(particles);
        }
        public Matrix<double> Prob(Matrix<double> particles) {
            return Matrix<double>.Exp(ComputeLogLikelihood(particles));
        }

        private Matrix<double> GetCalculatedDeltas() {
            // if (cachedDeltas == null || _dirtyCache) {
            //     return null;
            // }
            _dirtyCache = false;
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
            Matrix<double> allDeltas = GetCalculatedDeltas();
            if (allDeltas == null) { allDeltas = Matrix<double>.Build.DenseOfMatrix(deltas); }
            else {
                allDeltas = deltas.Stack(allDeltas);
            }
            if (tempDeltas != null) {
                allDeltas = allDeltas.Stack(tempDeltas);
            }
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