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

    void Start() {
        print("void Start was called! Heck yeah!");
        update();
    }

    void Update() {
    }

    void update() {

        var texture = new Texture2D(xdim, ydim, TextureFormat.RGB24, false); // garbage collector will tackle that it is new'ed 

        for (int y = 0; y < ydim; y++) {
            for (int x = 0; x < xdim; x++) {
                float v = pixelValue(new Vector3(x, y, this.slice));
                texture.SetPixel(x, y, new UnityEngine.Color(v, v, v));
            }
        }

        texture.filterMode = FilterMode.Point;  // nearest neigbor interpolation is used. (alternative is FilterMode.Bilinear)
        texture.Apply();  // Apply all SetPixel calls
        GetComponent<Renderer>().material.mainTexture = texture;

        mScript = GameObject.Find("GameObjectMesh").GetComponent<meshScript>();

        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        int index = 0;

        for (int y = 0; y < (ydim); y += 1) {
            for (int x = 0; x < (xdim); x += 1) {

                float iso = 1.0f - this.iso;

                float bl = texture.GetPixel(x, y).grayscale;
                float br = texture.GetPixel(x + 1, y).grayscale;
                float tl = texture.GetPixel(x, y + 1).grayscale;
                float tr = texture.GetPixel(x + 1, y + 1).grayscale;

                float worldX = (x / 100.0f) - 0.5f;
                float worldY = (y / 100.0f) - 0.5f;
                float delta = (0.5f / 100.0f);

                Vector3 l = new Vector3(worldX, worldY + delta, 0.0f);
                Vector3 r = new Vector3(worldX + delta * 2, worldY + delta, 0.0f);
                Vector3 t = new Vector3(worldX + delta, worldY + delta * 2, 0.0f);
                Vector3 b = new Vector3(worldX + delta, worldY, 0.0f);

                bool p1 = tl >= iso;
                bool p2 = tr >= iso;
                bool p3 = br >= iso;
                bool p4 = bl >= iso;

                string pattern = (p1 ? "1" : "0") + (p2 ? "1" : "0") + (p3 ? "1" : "0") + (p4 ? "1" : "0");
                
                switch (pattern) {
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

        print(vertices.Count() + "");
        print(indices.Count() + "");

        mScript.createMeshGeometry(vertices, indices);
    }

    private static Vector3 origo = new Vector3(50.0f, 50.0f, 50.0f);
    float pixelValue(Vector3 point) {
        
        float magnitude = (origo - point).magnitude;
        float intensity = magnitude / 50.0f;

        return intensity;
    }

    public void slicePosSliderChange(float val) {
        this.slice = val;
        update();
    }

    public void sliceIsoSliderChange(float val) {
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
