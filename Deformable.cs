using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
public class Deformable : MonoBehaviour
{
	[System.Serializable]
	public enum Deterioration
	{
		Deformar,
		DeformarYBorrar
	}

	[System.Serializable]
	public enum deformerObject
	{
		Static,
		Dinamic
	}

	//Public

	[Header("Configuración del deformable")]
	

	[Space(10)]
	[Tooltip("Impulso mínimo necesario para deformar el objeto.")]
	public float minImpulse = 2;

	[Space(10)]
	[Range(0, 1)]
	[Tooltip("Capacidad de deformación.")]
	public float malleability = 0.05f;

	[Space(10)]
	[Tooltip("Impulso máximo que se quiere aplicar a la deformación.")]
	public float maxImpulse = 50;

	[Space(10)]
	[Range(0, 1)]
	[Tooltip("Área de impacto de la colisión.")]
	public float impactZone = 0.1f;

	[Space(10)]
	[Tooltip("Determina si el objeto solo se va a deformar o deformar y borrar.")]
	[SerializeField] private Deterioration deterioType;

	[Space(10)]
	[Tooltip("Determina si el objeto deformador tiene velocidad o es estático/movido de manera artificial.")]
	[SerializeField] private deformerObject deformerType;




	//Private
	private Mesh m;
	private MeshCollider mc;
	private Vector3[] verts;
	private Dictionary<int, List<int>> trisWithVertex;
	private int[] triangles;
	private int[] origtriangles;
	private HashSet<int> trianglesDisabled;



	private void Start()
	{
		m = GetComponent<MeshFilter>().mesh;
		mc = GetComponent<MeshCollider>();
		origtriangles = m.triangles;
		triangles = new int[origtriangles.Length];
		origtriangles.CopyTo(triangles, 0);
		verts = m.vertices;
		trianglesDisabled = new HashSet<int>();
		trisWithVertex = new Dictionary<int, List<int>>();

		//Preparar la lista
		for (int i = 0; i < verts.Length; i++)
		{
			trisWithVertex[i] = new List<int>();
		}

		for (int i = 0; i < origtriangles.Length; i++)
		{
			int vertexIndex = origtriangles[i];
			trisWithVertex[vertexIndex].Add(i);
		}
		
	}



	

    private void OnCollisionEnter(Collision collision)
	{

		Debug.Log("collision");
		//Conseguir el punto, impulso y normales de la colision
		Vector3 pt = transform.InverseTransformPoint(collision.contacts[0].point);
		
		Vector3 nrm = transform.InverseTransformDirection(collision.contacts[0].normal);
		float imp = collision.impulse.magnitude;

		if (imp < minImpulse && deformerType == deformerObject.Dinamic)
			return;

		
      
		
		float scale; //Declarar fuera del loop
		for (int i = 0; i < verts.Length; i++)
		{
			//Conseguir la escala de deformación en base a la distancia
			float distance = (pt - verts[i]).magnitude;
			scale = Mathf.Clamp01(1 - distance / impactZone);
			if (imp > maxImpulse) imp = maxImpulse;

			//Deformar si el objeto deformador está estático
			if (deformerType == deformerObject.Static && scale > 0.08f)
			{

				Vector3 deformation = nrm * scale * malleability;
				verts[i] += deformation;
                if (deterioType == Deterioration.DeformarYBorrar) { 
					//Marcar los triangulos que hay que borrar
					for (int j = 0; j < trisWithVertex[i].Count; ++j)
					{

						int value = trisWithVertex[i][j];
						 
						int remainder = value % 3;
						trianglesDisabled.Add(value - remainder);
						trianglesDisabled.Add(value - remainder + 1);
						trianglesDisabled.Add(value - remainder + 2);

						

					}
				}

			}

			//Run this part if the deformer object has speed. 
			else if(scale >= 0.08f)
			{
				//Deform by impulse multiplied by scale and strength parameter	
				Vector3 deformation = nrm * scale * malleability * imp;
				verts[i] += deformation;
				
				if (deterioType == Deterioration.DeformarYBorrar)
				{
					// Marcar los triángulos a eliminar y deformar los vértices conectados
					for (int j = 0; j < trisWithVertex[i].Count; ++j)
					{

						int value = trisWithVertex[i][j];
						
						int remainder = value % 3;
						trianglesDisabled.Add(value - remainder);
						trianglesDisabled.Add(value - remainder + 1);
						trianglesDisabled.Add(value - remainder + 2);

						
						int vertex1 = origtriangles[value - remainder];
						int vertex2 = origtriangles[value - remainder + 1];
						int vertex3 = origtriangles[value - remainder + 2];

						
						verts[vertex1] += deformation;
						verts[vertex2] += deformation;
						verts[vertex3] += deformation;
					}
				}


			}
			else
			{
				//Deformar sin impulso significativo
				Vector3 deformation = nrm * scale * malleability * imp;
				verts[i] += deformation;
				
			}
			
			
			
		}


		// Aplicar cambios al collider y mesh
		triangles = origtriangles;
		int[] newTriangles = origtriangles.Where((_, index) => !trianglesDisabled.Contains(index)).ToArray();
		m.SetTriangles(newTriangles, 0);
		m.vertices = verts;
		mc.sharedMesh = m;


		// Recalcular propiedades del mesh
		// Actualmente, hacemos que Unity recalcule las normales. Podría optimizarse e implementarse manualmente.
		m.RecalculateNormals();
		m.RecalculateBounds(); 
	}

}