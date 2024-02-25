using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// using Unity.Barracuda;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;

// #r "nuget: libtorch-cpu-win-x64, 2.1.0.1"

// https://github.com/dotnet/TorchSharp/issues/169
// https://github.com/dotnet/TorchSharp/wiki/Tensor-String-Formatting
// https://github.com/dotnet/TorchSharp/discussions/874?sort=new
// https://www.goodai.com/neural-networks-in-unity-using-native-libraries/
public class BradleyTerryTester : MonoBehaviour
{


    private BradleyTerryDistribution distr;
    private Matrix<double> testDeltas;

    void Awake() {
        // System.Runtime.InteropServices.NativeLibrary.Load("D:/UnityProjects/BradleyTerryVisualizer/libtorch-win-shared-with-deps-2.2.1+cpu/");
        // NativeLibrary.Load("D:/UnityProjects/BradleyTerryVisualizer/libtorch-win-shared-with-deps-2.2.1+cpu/");
    }

    // Start is called before the first frame update
    void Start()
    {
        distr = new BradleyTerryDistribution(2);
        // [# batch, # particles, # features]
        // https://github.com/dotnet/TorchSharp/issues/269
        // torch.InitializeDeviceType(TorchSharp.DeviceType.CPU);
        // testDeltas = torch.tensor(new float[,,] {
        //     {{0.5f, 0.5f}, {-0.5f, -0.5f}, {-0.5f, 0.5f}, {0.5f, -0.5f}}
        // });
        
        testDeltas = Matrix<double>.Build.DenseOfArray(new double[,]{
            {0.5f, 0.5f},
            {0.5f, -0.5f},
            {-0.5f, 0.5f},
            {-0.5f, -0.5f},
        });
    }

    public void DoTest() {
        distr.AddDeltas(testDeltas);
        Matrix<double> testWeights = Matrix<double>.Build.DenseOfArray(new double[,]{
            {0.5f, -1.0f}
        });

        Debug.Log(Matrix<double>.Exp(distr.LogProb(testWeights)));
    }
}



// NotSupportedException: This application or script uses TorchSharp but doesn't contain a reference to libtorch-cpu-win-x64, Version=2.1.0.1.

// Consider referencing one of the combination packages TorchSharp-cpu, TorchSharp-cuda-linux, TorchSharp-cuda-windows or call System.Runtime.InteropServices.NativeLibrary.Load(path-to-LibTorchSharp.dll) explicitly for a Python install of pytorch. See https://github.com/dotnet/TorchSharp/issues/169.".

// For CUDA, you may need to call 'TorchSharp.torch.InitializeDeviceType(TorchSharp.DeviceType.CUDA)' before any use of TorchSharp CUDA packages from scripts or notebooks.

// Trace from LoadNativeBackend:

// TorchSharp: LoadNativeBackend: Initialising native backend, useCudaBackend = False

// Step 1 - First try regular load of native libtorch binaries.

//     Trying to load native component torch_cpu relative to D:\UnityProjects\BradleyTerryVisualizer\Assets\Packages\TorchSharp.0.101.6\lib\netstandard2.0\TorchSharp.dll
//     Failed to load native component torch_cpu relative to D:\UnityProjects\BradleyTerryVisualizer\Assets\Packages\TorchSharp.0.101.6\lib\netstandard2.0\TorchSharp.dll
//     Trying to load native component LibTorchSharp relative to D:\UnityProjects\BradleyTerryVisualizer\Assets\Packages\TorchSharp.0.101.6\lib\netstandard2.0\TorchSharp.dll
//     Failed to load native component LibTorchSharp relative to D:\UnityProjects\BradleyTerryVisualizer\Assets\Packages\TorchSharp.0.101.6\lib\netstandard2.0\TorchSharp.dll
//     Result from regular native load of LibTorchSharp is False

// Step 3 - Alternative load from consolidated directory of native binaries from nuget packages

//     torchsharpLoc = D:\UnityProjects\BradleyTerryVisualizer\Assets\Packages\TorchSharp.0.101.6\lib\netstandard2.0
//     packagesDir = D:\UnityProjects\BradleyTerryVisualizer\Assets
//     torchsharpHome = D:\UnityProjects\BradleyTerryVisualizer\Assets\Packages\TorchSharp.0.101.6
//     Giving up, TorchSharp.dll does not appear to have been loaded from package directories

// TorchSharp.torch.LoadNativeBackend (System.Boolean useCudaBackend, System.Text.StringBuilder& trace) (at <c7662ae9d8164077955d32885495c7cd>:0)
// TorchSharp.torch.InitializeDeviceType (TorchSharp.DeviceType deviceType) (at <c7662ae9d8164077955d32885495c7cd>:0)
// TorchSharp.torch.InitializeDevice (TorchSharp.torch+Device device) (at <c7662ae9d8164077955d32885495c7cd>:0)
// TorchSharp.torch..cctor () (at <c7662ae9d8164077955d32885495c7cd>:0)
// Rethrow as TypeInitializationException: The type initializer for 'TorchSharp.torch' threw an exception.
// BradleyTerryTester.Start () (at Assets/Scripts/BradleyTerryTester.cs:20)