using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeformableObjects: MonoBehaviour
{
	//Public
	public float minImpulse = 2;
	public float malleability = 0.05f;
	public float radius = 0.1f;
	public float maxImpulse = 50;
	public bool	staticDeformer = false;
	public bool onlyDeform;

	//Private
	private Mesh m;
	private MeshCollider mc;
	private Vector3[] verts;
	private List<int> indexList;
	private List<int>[] trisWithVertex;
	private int[] triangles;
	private int[] origtriangles;
	private bool[] trianglesDisabled;



	private void Start()
	{
		m = GetComponent<MeshFilter>().mesh;
		mc = GetComponent<MeshCollider>();
		origtriangles = m.triangles;
		triangles = new int[origtriangles.Length];
		origtriangles.CopyTo(triangles, 0);
		verts = m.vertices;
		//Debug.Log(m.triangles.Length);
		//Debug.Log(m.vertices.Length);
		trianglesDisabled = new bool[origtriangles.Length];
		trisWithVertex = new List<int>[verts.Length];
		//Set up the list
		for (int i = 0; i < verts.Length; ++i)
		{
			trisWithVertex[i] = origtriangles.IndexOf(i);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		Debug.Log("collision");
		//Get point, impulse mag, and normal
		Vector3 pt = transform.InverseTransformPoint(collision.GetContact(0).point);
		Vector3 nrm = transform.InverseTransformDirection(collision.GetContact(0).normal);
		float imp = collision.impulse.magnitude;
		if (imp < minImpulse && !staticDeformer)
			return;

		//Deform vertices
		
      
		
		float scale; //Declare outside of tight loop
		for (int i = 0; i < verts.Length; i++)
		{
			//Get deformation scale based on distanceA
			scale =(float) Mathf.Clamp(radius - (pt - verts[i]).magnitude, 0, radius);
			if (imp > maxImpulse) imp = maxImpulse;

			//Run this part if the deformer object is moved manualy (speed = 0)
			if (staticDeformer && scale > 0.08f)
			{

				Vector3 deformation = nrm * scale * malleability;
				verts[i] += deformation;
				//Mark the triangles to be deleted
				if (!onlyDeform)
				{
					Debug.Log(onlyDeform);
					for (int j = 0; j < trisWithVertex[i].Count; ++j)
					{

						int value = trisWithVertex[i][j];
						//get values reminder because the triangles array is always multiple of 3 
						int remainder = value % 3;
						trianglesDisabled[value - remainder] = true;
						trianglesDisabled[value - remainder + 1] = true;
						trianglesDisabled[value - remainder + 2] = true;
					}
				}

			}

			//Run this part if the deformer object has speed. 
			else if(scale >= 0.08f)
			{
				//Deform by impulse multiplied by scale and strength parameter	
				Vector3 deformation = nrm * scale * malleability * imp;
				verts[i] += deformation;

				if (!onlyDeform)
				{
					for (int j = 0; j < trisWithVertex[i].Count; ++j)
					{
						//Mark the triangles to be deleted
						int value = trisWithVertex[i][j];
						int remainder = value % 3;
						//get values reminder because the triangles array is always multiple of 3 
						trianglesDisabled[value - remainder] = true;
						trianglesDisabled[value - remainder + 1] = true;
						trianglesDisabled[value - remainder + 2] = true;
					}
				}

			}
			
			else
			{
				Vector3 deformation = nrm * scale * malleability * imp;
				verts[i] += deformation;
			}
			
			
			
		}
		

		//Apply changes to collider and mesh
		triangles = origtriangles;
		triangles = triangles.RemoveAllSpecifiedIndicesFromArray(trianglesDisabled).ToArray();
		m.SetTriangles(triangles, 0);
		m.vertices = verts;
		mc.sharedMesh = m;

		//Recalculate mesh stuff
		///Currently gets unity to recalc normals. Could be optimized and improved by doing it ourselves.
		m.RecalculateNormals();
		m.RecalculateBounds();
	}

	private bool CheckList(int val)
    {
        if (indexList.Contains(val))
        {
			Debug.Log("true");
			return true;
        }
        else
        {
            indexList.Add(val);
			Debug.Log("false");
			return false;
        }
    }
}