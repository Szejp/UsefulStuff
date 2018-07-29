using System.Collections;
using UnityEngine;

public class SimpleGrid : MonoBehaviour {

	[SerializeField]
	private string meshName = "SimpleGrid";
	[SerializeField]
	private Vector2 dimensions;
	[SerializeField]
	private float delta = 1;
	private Mesh mesh;
	private Vector3[] vertices;
	int[] triangles;

	[ContextMenu("GenerateGrid")]
	public void GenerateGrid() {
		mesh = GetComponent<MeshFilter>().mesh;
		mesh.name = meshName;

		int gridSize = (int)(dimensions.x + 1) * (int)(dimensions.y + 1);
		vertices = new Vector3[gridSize];
		triangles = new int[6 * (int)((dimensions.x) * (dimensions.y))];

		// assign vertices
		for (int i = 0; i < gridSize; i++) {
			vertices[i] = new Vector3(i % (dimensions.x + 1) * delta, (int)(i / (dimensions.y + 1)) * delta, 0);
		}

		// assign triangles
		for (int j = 0, index = 0, trianglesIndex = 0; j < dimensions.y; j++, index++) {
			for (int i = 0; i < dimensions.x; i++, index++, trianglesIndex += 6) {
				triangles[trianglesIndex] = index;
				triangles[trianglesIndex + 1] = triangles[trianglesIndex + 3] = index + 1;
				triangles[trianglesIndex + 2] = triangles[trianglesIndex + 5] = index + (int)dimensions.x + 1;
				triangles[trianglesIndex + 4] = index + (int)dimensions.x + 2;
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
	}

	private void Start() {
		GenerateGrid();
	}

	private void OnDrawGizmos() {
		if (vertices == null) return;
		foreach (var v in vertices) {
			Gizmos.DrawSphere(v + transform.position, .2f);
		}
	}
}
