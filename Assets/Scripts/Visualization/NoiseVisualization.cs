﻿using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


using static Noise;
public class NoiseVisualization : Visualization {

    static int noiseId = Shader.PropertyToID("_Noise");

    [SerializeField]
    Settings noiseSettings = Settings.Default;

    [SerializeField]
    SpaceTRS domain = new SpaceTRS {
        scale = 8f
    };

    static ScheduleDelegate[,] noiseJobs = {
        {
            Job<Lattice1D<Perlin>>.ScheduleParallel,
            Job<Lattice2D<Perlin>>.ScheduleParallel,
            Job<Lattice3D<Perlin>>.ScheduleParallel
        },
        {
            Job<Lattice1D<Turbulence<Perlin>>>.ScheduleParallel,
            Job<Lattice2D<Turbulence<Perlin>>>.ScheduleParallel,
            Job<Lattice3D<Turbulence<Perlin>>>.ScheduleParallel

        },
        {
            Job<Lattice1D<Value>>.ScheduleParallel,
            Job<Lattice2D<Value>>.ScheduleParallel,
            Job<Lattice3D<Value>>.ScheduleParallel
        },
        {
            Job<Lattice1D<Turbulence<Value>>>.ScheduleParallel,
            Job<Lattice2D<Turbulence<Value>>>.ScheduleParallel,
            Job<Lattice3D<Turbulence<Value>>>.ScheduleParallel
        }

    };

    public enum NoiseType { Perlin, PerlinTurbulence, Value , ValueTurbulence}

    [SerializeField]
    NoiseType type;

    [SerializeField, Range(1, 3)]
    int dimensions = 3;

    NativeArray<float4> noise;
    ComputeBuffer noiseBuffer;

    protected override void EnableVisualization(int dataLength, MaterialPropertyBlock propertyBlock) {
        noise = new NativeArray<float4>(dataLength, Allocator.Persistent);
        noiseBuffer = new ComputeBuffer(dataLength * 4, 4);
        propertyBlock.SetBuffer(noiseId, noiseBuffer);
    }

    protected override void DisableVisualization() {
        noise.Dispose();
        noiseBuffer.Release();
        noiseBuffer = null;
    }

    protected override void UpdateVisualization(
        NativeArray<float3x4> positions, int resolution, JobHandle handle) {


        noiseJobs[(int)type , dimensions - 1](
            positions,noise,noiseSettings,domain,resolution,handle
        ).Complete();
        noiseBuffer.SetData(noise.Reinterpret<float>(4 * 4));

    }
}
