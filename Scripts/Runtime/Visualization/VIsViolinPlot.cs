using System.Linq;
using System;
using UnityEngine;
using System.Collections.Generic;

public class VisViolinPlot : Vis
{
    public VisViolinPlot()
    {
        title = "VisViolin";
        dataMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/Marks/Sphere");
        tickMarkPrefab = (GameObject)Resources.Load("Prefabs/DataVisPrefabs/VisContainer/Tick");
    }

    public override GameObject CreateVis(GameObject container)
    {
        base.CreateVis(container);

        double[,] DensityCalculate = KernelDensityEstimation.KDE(dataSets[0].ElementAt(0).Value, 1, 100);
        double[] xValues = Enumerable.Range(0, DensityCalculate.GetLength(0)).Select(i => DensityCalculate[i, 0]).ToArray();
        double[] yValues = Enumerable.Range(0, DensityCalculate.GetLength(0)).Select(i => DensityCalculate[i, 1]).ToArray();
        double[] xValuesMirrored = new double[xValues.Length * 2];
        double[] yValuesMirrored = new double[yValues.Length * 2];

        double[] numbers = yValues.ToArray();



        for (int i = 0; i <= xValues.Length - 1; i++)
        {
            xValuesMirrored[i] = xValues[i];
            yValuesMirrored[i] = yValues[i];
        }
        for (int i = xValues.Length - 1; i >= 0; i--)
        {
            xValuesMirrored[2 * xValues.Length - i - 1] = xValues[i];
            yValuesMirrored[2 * xValues.Length - i - 1] = yValues[i] * -1.0f;
        }

        //## 01:  Create Axes and Grids
        // X Axis
        visContainer.CreateAxis(dataSets[0].ElementAt(0).Key, xValuesMirrored, Direction.X);
        visContainer.CreateGrid(Direction.X, Direction.Y);

        // Y Axis
        //visContainer.CreateAxis(dataSets[0].ElementAt(1).Key, binData.yValues, Direction.Y);
        visContainer.CreateAxis("Counter", yValuesMirrored, Direction.Y);

        //## 02: Set Remaining Vis Channels (Color,...)
        visContainer.SetChannel(VisChannel.XPos, xValuesMirrored);
        visContainer.SetChannel(VisChannel.YPos, yValuesMirrored);
        visContainer.SetChannel(VisChannel.Color, yValuesMirrored);

        double[] sizeList = new double[xValuesMirrored.Length];

        visContainer.SetChannel(VisChannel.XSize, sizeList);
        visContainer.SetChannel(VisChannel.YSize, sizeList);
        visContainer.SetChannel(VisChannel.ZSize, sizeList);
        //## 03: Draw all Data Points with the provided Channels 
        visContainer.CreateDataMarks(dataMarkPrefab);

        ConnectPoints(visContainer.dataMarkList);
        //## 04: Rescale Chart
        visContainerObject.transform.localScale = new Vector3(width, height, depth);

        CreateFillArea(visContainer.dataMarkList);

        MediansAndOtherThings(visContainer.dataMarkList);

        return visContainerObject;
    }

    private void ConnectPoints(List<DataMark> dataList)
    {
        GameObject lineObject = new GameObject("LineRenderer");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        ///# Get a list of closed graph consisting of KDE with the x-axis as the axis of symmetry
        List<Vector3> points = new List<Vector3>();
        ///## 01 Get the points already calculated from the KDE calculation
        float mirrorAxisPosition;
        foreach (var point in dataList)
        {
            var p = point.GetDataMarkChannel().position / 4.0f;
            Vector3 pointOrigin = new Vector3(p.x, p.y, p.z);
            points.Add(pointOrigin);
        }
        ///## 02 Get the other half point with the x-axis as the axis of symmetry
        // for (int i = points.Count - 1; i >= 0; i--)
        // {
        //     Vector3 pointMirrored = new Vector3(points[i].x, (points[i].y * (-1.0f)), points[i].z);
        //     Debug.Log(pointMirrored.y);
        //     points.Add(pointMirrored);
        // }
        ///### 02.1 Get the last point(same as the first point) to close the graph
        Vector3 pointlast = new Vector3(points[0].x, points[0].y, points[0].z);
        points.Add(pointlast);

        lineRenderer.positionCount = points.Count;
        lineRenderer.startWidth = 0.0005f;
        lineRenderer.endWidth = 0.0005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        lineRenderer.SetPositions(points.ToArray());
        //CreatePaintArea(points, Color.red);
    }


    private void CreateFillArea(List<DataMark> dataList)
    {
        // Get the positions of points from DataMark and create a Vector3 array
        Vector3[] points = dataList.Select(dataMark => dataMark.GetDataMarkChannel().position / 4.0f).ToArray();




        // New Game Object
        GameObject fillAreaObject = new GameObject("FillArea");
        fillAreaObject.transform.SetParent(visContainerObject.transform); // Делаем его дочерним объектом контейнера

        // MeshFilter und MeshRenderer
        MeshFilter meshFilter = fillAreaObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = fillAreaObject.AddComponent<MeshRenderer>();

        // New Mesh
        Mesh mesh = new Mesh();

        // Устанавливаем вершины сетки
        mesh.vertices = points;

        // A lot of Treugs
        int[] triangles = new int[(points.Length - 1) * 3];
        int index = 0;
        for (int i = 0; i < points.Length - 1; i++)
        {
            triangles[index++] = i;
            triangles[index++] = i + 1;
            triangles[index++] = points.Length - i - 1;
        }

        // Treugs place
        mesh.triangles = triangles;

        // Вычисляем нормали для освещения
        //mesh.RecalculateNormals();

        // Присваиваем созданную сетку MeshFilter
        meshFilter.mesh = mesh;

        // Устанавливаем материал для заполнения области
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = Color.blue; // Задайте желаемый цвет

        // Опционально можно добавить другие компоненты, такие как Collider, чтобы область была интерактивной
        // fillAreaObject.AddComponent<MeshCollider>();


    }

    private void MediansAndOtherThings(List<DataMark> dataList)
    {
        // Сортируем данные по оси X

        List<Vector3> points = new List<Vector3>();
        List<DataMark> sortedData = dataList.OrderBy(dataMark => dataMark.GetDataMarkChannel().position.x).ToList();
        List<DataMark> sortedDatay = dataList.OrderBy(dataMark => dataMark.GetDataMarkChannel().position.y).ToList();

        //int index = sortedDatay.FindIndex(dataMark => (int)dataMark.GetDataMarkChannel().position.y == 0);

        //List<int> indexes = sortedDatay.FindAllIndex(dataMark => dataMark.GetDataMarkChannel().position.y == 0);

        int index = sortedData.FindIndex(dataMark => (int)dataMark.GetDataMarkChannel().position.x == 0);



        // Вычисляем индексы для 25% и 75% данных
        int index25 = Mathf.RoundToInt(sortedData.Count * 0.25f);
        int index75 = Mathf.RoundToInt(sortedData.Count * 0.75f);

        float startWidth = 1f;
        float endWidth = 1f;

        float minY = sortedData[index].GetDataMarkChannel().position.x * 1.25f;
        Debug.Log("Look   " + index);

        // Получаем значения 25%, медианы и 75% для оси X
        float x25 = sortedData[index25].GetDataMarkChannel().position.x / 4.0f;
        float xMedian = sortedData[sortedData.Count / 2].GetDataMarkChannel().position.x / 4.0f;
        float x75 = sortedData[index75].GetDataMarkChannel().position.x / 4.0f;

        // Получаем значения медианы для оси Y
        float yMedian = sortedData[sortedData.Count / 2].GetDataMarkChannel().position.y / 4.0f;

        // Создаем свечу
        GameObject candleObject = new GameObject("Candle");
        candleObject.transform.SetParent(visContainerObject.transform);

        // Создаем LineRenderer для свечи
        LineRenderer lineRenderer = candleObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Устанавливаем точки свечи
        lineRenderer.SetPosition(0, new Vector3(x25, minY, 0f));
        lineRenderer.SetPosition(1, new Vector3(x75, minY, 0f));

        CreateVerticalLine(x25);
        CreateVerticalLine(xMedian);
        CreateVerticalLine(x75);


        // Опционально можно установить цвет свечи
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }



    private void CreateVerticalLine(float xPosition)
    {

        List<Vector3> points = new List<Vector3>();


        // Создаем объект черты
        GameObject lineObject = new GameObject("VerticalLine");
        lineObject.transform.SetParent(visContainerObject.transform);

        // Создаем LineRenderer для черты
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Получаем высоту графика
        float graphHeight = height;

        // Устанавливаем точки черты с уменьшенной длиной и перемещаем в середину графика
        float lineLength = 0.2f; // Устанавливаем желаемую длину черты
        lineRenderer.SetPosition(0, new Vector3(xPosition, 0.118f, 0f));
        lineRenderer.SetPosition(1, new Vector3(xPosition, 0.13f, 0f));

        // Устанавливаем цвет черты
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
    }



}