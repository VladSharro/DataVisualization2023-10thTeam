using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;

public class VisDensityPlot : Vis
{
    public VisDensityPlot()
    {
        title = "VisDensity";
        dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Sphere");
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    public override GameObject CreateVis(GameObject container)
    {
        base.CreateVis(container);

        double[,] DensityCalculate = KernelDensityEstimation.KDE(dataSets[0].ElementAt(0).Value, 1, 100);
        double[] xValues = Enumerable.Range(0, DensityCalculate.GetLength(0)).Select(i => DensityCalculate[i, 0]).ToArray();
        double[] yValues = Enumerable.Range(0, DensityCalculate.GetLength(0)).Select(i => DensityCalculate[i, 1]).ToArray();
        //for (int i = 0; i < yValues.Length; i++)
        //{
        //    Debug.Log(xValues[i]);
        //}
        // ConnectPoints(xValues, yValues);
        //## 01:  Create Axes and Grids
        //VisBarChart.xyzTicks = { 10, 10, 10 };
        // X Axis
        visContainer.CreateAxis(dataSets[0].ElementAt(0).Key, xValues, Direction.X);
        visContainer.CreateGrid(Direction.X, Direction.Y);

        // Y Axis
        //visContainer.CreateAxis(dataSets[0].ElementAt(1).Key, binData.yValues, Direction.Y);
        visContainer.CreateAxis("Counter", yValues, Direction.Y);

        //## 02: Set Remaining Vis Channels (Color,...)
        visContainer.SetChannel(VisChannel.XPos, xValues);
        visContainer.SetChannel(VisChannel.YPos, yValues);
        visContainer.SetChannel(VisChannel.Color, yValues);

        //## 03: Draw all Data Points with the provided Channels 
        visContainer.CreateDataMarks(dataMarkPrefab);

        //## 04: Rescale Chart
        visContainerObject.transform.localScale = new Vector3(width, height, depth);
        // Vector3[] points = new Vector3[];
        List<Vector3> points = new List<Vector3>();

        foreach (var point in visContainer.dataMarkList)
        {
            var p = point.GetDataMarkChannel().position / 4.0f;
            points.Add(p);
        }

        GameObject lineObject = new GameObject("LineRenderer");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        //      LineRenderer          
        lineRenderer.positionCount = points.Count;
        lineRenderer.startWidth = 0.002f;
        lineRenderer.endWidth = 0.002f;
        // lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.SetPositions(points.ToArray());

        return visContainerObject;
    }
    private void ConnectPoints(double[] xV, double[] yV)
    {
        //      LineRenderer        Ŀն   
        GameObject lineObject = new GameObject("LineRenderer");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        //      LineRenderer          
        lineRenderer.positionCount = xV.Length;
        lineRenderer.startWidth = 0.002f;
        lineRenderer.endWidth = 0.002f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        //                
        Vector3[] linePoints = new Vector3[xV.Length];
        for (int i = 0; i < xV.Length; i++)
        {
            linePoints[i] = new Vector3(((float)xV[i] * 0.00427f) + 0.029f, ((float)yV[i] * 4f) + 0.0275f, 0.0f);
        }
        //          ĵ     
        lineRenderer.SetPositions(linePoints);
    }
}