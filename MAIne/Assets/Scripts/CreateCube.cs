using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCube : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //CreateCubeMesh();
        //CreateQuadMesh();
        CreateTwoQuadMesh();
    }

    void CreateCubeMesh()
    {
        float size = 1f;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        //Front
        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(0, size, 0));
        vertices.Add(new Vector3(size, size, 0));
        vertices.Add(new Vector3(size, 0, 0));
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);
        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(3);

        //Left
        vertices.Add(new Vector3(0, 0, size));
        vertices.Add(new Vector3(0, size, size));
        vertices.Add(new Vector3(0, size, 0));
        vertices.Add(new Vector3(0, 0, 0));
        triangles.Add(4);
        triangles.Add(5);
        triangles.Add(6);
        triangles.Add(4);
        triangles.Add(6);
        triangles.Add(7);

        //Back
        vertices.Add(new Vector3(size, 0, size));
        vertices.Add(new Vector3(size, size, size));
        vertices.Add(new Vector3(0, size, size));
        vertices.Add(new Vector3(0, 0, size));
        triangles.Add(8);
        triangles.Add(9);
        triangles.Add(10);
        triangles.Add(8);
        triangles.Add(10);
        triangles.Add(11);

        //Right
        vertices.Add(new Vector3(size, 0, 0));
        vertices.Add(new Vector3(size, size, 0));
        vertices.Add(new Vector3(size, size, size));
        vertices.Add(new Vector3(size, 0, size));
        triangles.Add(12);
        triangles.Add(13);
        triangles.Add(14);
        triangles.Add(12);
        triangles.Add(14);
        triangles.Add(15);

        //Top
        vertices.Add(new Vector3(0, size, 0));
        vertices.Add(new Vector3(0, size, size));
        vertices.Add(new Vector3(size, size, size));
        vertices.Add(new Vector3(size, size, 0));
        triangles.Add(16);
        triangles.Add(17);
        triangles.Add(18);
        triangles.Add(16);
        triangles.Add(18);
        triangles.Add(19);

        //Bottom
        vertices.Add(new Vector3(0, 0, size));
        vertices.Add(new Vector3(0, 0, 0));
        vertices.Add(new Vector3(size, 0, 0));
        vertices.Add(new Vector3(size, 0, size));
        triangles.Add(20);
        triangles.Add(21);
        triangles.Add(22);
        triangles.Add(20);
        triangles.Add(22);
        triangles.Add(23);

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        //mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void CreateQuadMesh()
    {
        float size = 0.5f;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        //Front
        vertices.Add(new Vector3(-size, 0, 0));
        vertices.Add(new Vector3(-size, size*2, 0));
        vertices.Add(new Vector3(size, size*2, 0));
        vertices.Add(new Vector3(size, 0, 0));
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);
        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(3);
        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0, 1));
        uv.Add(new Vector2(1, 1));
        uv.Add(new Vector2(1, 0));

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();

        //mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void CreateTwoQuadMesh()
    {
        float size = 0.5f;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        //Front
        vertices.Add(new Vector3(-size, 0, 0));
        vertices.Add(new Vector3(-size, size * 2, 0));
        vertices.Add(new Vector3(size, size * 2, 0));
        vertices.Add(new Vector3(size, 0, 0));
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(2);
        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(3);
        triangles.Add(2);
        triangles.Add(1);
        triangles.Add(0);
        triangles.Add(3);
        triangles.Add(2);
        triangles.Add(0);
        uv.Add(new Vector2(0, 0));
        uv.Add(new Vector2(0, 1));
        uv.Add(new Vector2(1, 1));
        uv.Add(new Vector2(1, 0));

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        //mesh.RecalculateNormals();

        //mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
