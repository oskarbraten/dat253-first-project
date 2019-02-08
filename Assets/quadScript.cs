using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

public class quadScript : MonoBehaviour
{
    private meshScript mScript;
    private static int xdim = 100;
    private static int ydim = 100;
    private float iso = 0.0f;
    private float slice = 0.0f;

    void Start()
    {
        print("void Start was called! Heck yeah!");
        update();
    }

    void Update()
    {
    }

    void update()
    {

        var texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false); // garbage collector will tackle that it is new'ed 

        for (int y = 0; y < ydim; y++)
        {
            for (int x = 0; x < xdim; x++)
            {
                float v = pixelValue(new Vector3(x, y, this.slice));
                texture.SetPixel(x, y, new UnityEngine.Color(v, v, v));
            }
        }

        texture.filterMode = FilterMode.Point;  // nearest neigbor interpolation is used. (alternative is FilterMode.Bilinear)
        texture.Apply();  // Apply all SetPixel calls
        GetComponent<Renderer>().material.mainTexture = texture;

        var circle = marchingSquares(texture, true);

        mScript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();
        mScript.createMeshGeometry(circle.Item1, circle.Item2);
    }

    Tuple<List<Vector3>, List<int>> marchingSquares(Texture2D texture, bool interpolated)
    {

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        int index = 0;

        for (int y = 0; y < (ydim); y += 1)
        {
            for (int x = 0; x < (xdim); x += 1)
            {

                float iso = 1.0f - this.iso;

                float bl = texture.GetPixel(x, y).grayscale;
                float br = texture.GetPixel(x + 1, y).grayscale;
                float tl = texture.GetPixel(x, y + 1).grayscale;
                float tr = texture.GetPixel(x + 1, y + 1).grayscale;

                float worldX = (x / 100.0f) - 0.5f;
                float worldY = (y / 100.0f) - 0.5f;
                float step = (1.0f / 100.0f);

                float deltaL = 0.005f;
                float deltaR = 0.005f;
                float deltaT = 0.005f;
                float deltaB = 0.005f;

                if (interpolated)
                {
                    if (tl < bl)
                    {
                        deltaL = 1.0f - ((iso - tl) / (bl - tl));
                    }
                    else
                    {
                        deltaL = ((iso - bl) / (tl - bl));
                    }
                    deltaL /= 100.0f;
                    if (tr < br)
                    {
                        deltaR = 1.0f - ((iso - tr) / (br - tr));
                    }
                    else
                    {
                        deltaR = ((iso - br) / (tr - br));
                    }
                    deltaR /= 100.0f;
                    if (tl < tr)
                    {
                        deltaT = 1.0f - ((iso - tr) / (tl - tr));
                    }
                    else
                    {
                        deltaT = ((iso - tl) / (tr - tl));
                    }
                    deltaT /= 100.0f;

                    if (bl < br)
                    {
                        deltaB = 1.0f - ((iso - br) / (bl - br));
                    }
                    else
                    {
                        deltaB = ((iso - bl) / (br - bl));
                    }
                    deltaB /= 100.0f;
                }

                Vector3 l = new Vector3(worldX, worldY + deltaL, 0.0f);
                Vector3 r = new Vector3(worldX + step, worldY + deltaR, 0.0f);
                Vector3 t = new Vector3(worldX + deltaT, worldY + step, 0.0f);
                Vector3 b = new Vector3(worldX + deltaB, worldY, 0.0f);

                bool p1 = tl >= iso;
                bool p2 = tr >= iso;
                bool p3 = br >= iso;
                bool p4 = bl >= iso;

                string pattern = (p1 ? "1" : "0") + (p2 ? "1" : "0") + (p3 ? "1" : "0") + (p4 ? "1" : "0");

                switch (pattern)
                {
                    case "1110":
                    case "0001":
                        vertices.Add(l);
                        vertices.Add(b);
                        indices.Add(index + 0);
                        indices.Add(index + 1);
                        break;
                    case "1101":
                    case "0010":
                        vertices.Add(b);
                        vertices.Add(r);
                        indices.Add(index + 0);
                        indices.Add(index + 1);
                        break;
                    case "1011":
                    case "0100":
                        vertices.Add(r);
                        vertices.Add(t);
                        indices.Add(index + 0);
                        indices.Add(index + 1);
                        break;
                    case "0111":
                    case "1000":
                        vertices.Add(l);
                        vertices.Add(t);
                        indices.Add(index + 0);
                        indices.Add(index + 1);
                        break;
                    case "1100":
                    case "0011":
                        vertices.Add(l);
                        vertices.Add(r);
                        indices.Add(index + 0);
                        indices.Add(index + 1);
                        break;
                    case "1001":
                    case "0110":
                        vertices.Add(t);
                        vertices.Add(b);
                        indices.Add(index + 0);
                        indices.Add(index + 1);
                        break;
                    case "0000":
                    case "1111":
                    // do nothing.
                    default:
                        continue;
                }

                // increment the index counter if we added some new vertices.
                index += 2;

            }
        }

        return (vertices, indices);
    }

    private static Vector3 origo = new Vector3(50.0f, 50.0f, 50.0f);
    float pixelValue(Vector3 point)
    {

        float magnitude = (origo - point).magnitude;
        float intensity = magnitude / 50.0f;

        return intensity;
    }

    public void slicePosSliderChange(float val)
    {
        this.slice = val;
        update();
    }

    public void sliceIsoSliderChange(float val)
    {
        this.iso = val;
        update();
    }

    public void button1Pushed()
    {
        print("button1Pushed");
    }

    public void button2Pushed()
    {
        print("button2Pushed");
    }

}
