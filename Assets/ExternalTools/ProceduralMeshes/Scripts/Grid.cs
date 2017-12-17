using UnityEngine;
using System.Collections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Grid : MonoBehaviour {

    public int xSize, ySize;

    private Vector3[] _vertices;

    private void Awake() {
        Generate();
    }

    private void Generate() {
        _vertices = new Vector3[(xSize + 1) * (ySize + 1)];
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.black;
        for (int i = 0; i < _vertices.Length; i++) {
            Gizmos.DrawSphere(_vertices[i], 0.1f);
        }
    }
}