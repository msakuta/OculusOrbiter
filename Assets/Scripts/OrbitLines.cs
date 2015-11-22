using UnityEngine;
using System;
using System.Collections;

public class OrbitLines : MonoBehaviour {
	private const int vertexCount = 256;

	// Use this for initialization
	void Start () {
		LineRenderer lr = GetComponent<LineRenderer>();
		lr.SetVertexCount(vertexCount);
		for(int i = 0; i < vertexCount; i++)
		{
			double theta = i * Math.PI * 2 / (vertexCount - 1);
			lr.SetPosition(i, new Vector3((float)Math.Cos(theta), (float)Math.Sin(theta), 0));
		}
	}

}
