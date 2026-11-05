using System.Collections.Generic;
using UnityEngine;
using Suimono.Core; 
using Buoyancy.Struct;
using Buoyancy.Math;
using Buoyancy.MeshWorker;

public class BuoyancyComponent : MonoBehaviour
{
    [Header("Mesh Settings")]
    [Tooltip("Arraste o modelo 3D (malha) deste objeto para aqui")]
    public Mesh hullMesh;

    public delegate void ForceAction(Triangle triangle, Rigidbody rb);
    public ForceAction underwaterForces;
    public ForceAction abovewaterForces;

    private Rigidbody rb;
    private SuimonoModule moduloSuimono; // Criamos a variável para guardar o módulo
    
    void Start()
    {
        // O código procura e guarda o SUIMONO_Module que está na sua hierarquia
        moduloSuimono = FindObjectOfType<SuimonoModule>();
        
        WaterMath.GetWaterHeight += GetSuimonoWaterHeight;
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (hullMesh == null) return;

        var triangles = TriangleParser.parse(hullMesh, transform);
        var hullTriangles = HullMath.CutHullAtWaterline(triangles);

        if (underwaterForces != null)
        {
            ApplyUnderwaterForces(hullTriangles.underwater);
        }
        if (abovewaterForces != null)
        {
            ApplyAbovewaterForces(hullTriangles.abovewater);
        }
        
        WaterMath.caсheHeightMap.Clear();
    }

    private void ApplyUnderwaterForces(List<Triangle> underwater)
    {
        foreach (Triangle triangle in underwater) underwaterForces(triangle, rb);
    }

    private void ApplyAbovewaterForces(List<Triangle> abovewater)
    {
        foreach (Triangle triangle in abovewater) abovewaterForces(triangle, rb);
    }

    // --- Leitura das ondas do SUIMONO ---
    private float GetSuimonoWaterHeight(Vector3 position)
    {
        if (moduloSuimono != null) 
        {
            // Pede ao SUIMONO a altura exata da superfície (incluindo as ondas)
            return moduloSuimono.SuimonoGetHeight(position, "surfaceLevel");
        }
        return 0f; 
    }
}
