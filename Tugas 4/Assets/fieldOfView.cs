using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class fieldOfView : MonoBehaviour
{
    [SerializeField]
    private LayerMask layerMask;
    // layer mask digunakan untuk supaya raycast ngecek collide di layer apa
    // yang ada di layermask yang akan dicek, yang tidak ada maka akan diignore

    public Mesh mesh;
    private Vector3 origin;
    public float curAngle;
    public float fov;
    public float viewDistance;
    public int rayCtr;


    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        this.origin = Vector3.zero;
    }

    // Update is called once per frame
    void LateUpdate()
    {

        // banyak titik dalam mesh, makin banyak makin smooth

        // current angle player 
        float curAngle = this.curAngle;

        // angle increase
        float angleIncrease = fov / rayCtr;


        Vector3[] vertices = new Vector3[rayCtr + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] tri = new int[rayCtr * 3];

        vertices[0] = this.origin;

        int vertexIndex = 1;
        int triIndex = 0;
        for (int i = 0; i <= rayCtr; i++)
        {
            Vector3 vertex;
            // seperti collider tetapi raycast
            RaycastHit2D raycastHit2D = Physics2D.Raycast(this.origin, UtilsClass.GetVectorFromAngle(curAngle), viewDistance, layerMask); 
            if (raycastHit2D.collider == null)
            {
                // no hit
                vertex = this.origin + UtilsClass.GetVectorFromAngle(curAngle) * viewDistance;
            }
            else
            {
                // hit object
                Debug.Log(raycastHit2D.collider.gameObject.name);
                vertex = raycastHit2D.point;
                // jadi jika kena object maka titik render hanya di point object kena raycast 2d itu
            }

            vertices[vertexIndex] = vertex;

            if (i > 0) // karena vertex origin tidak punya prev vertex
            {
                tri[triIndex + 0] = 0; // origin
                tri[triIndex + 1] = vertexIndex - 1; // prev vertex
                tri[triIndex + 2] = vertexIndex; // cur vertex
                
                triIndex += 3;
            }

            vertexIndex++;
            curAngle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = tri;
    }

    public void setOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void setAimDirection(Vector3 aimDir)
    {
        this.curAngle = UtilsClass.GetAngleFromVectorFloat(aimDir) + fov / 2f;
    }


}
