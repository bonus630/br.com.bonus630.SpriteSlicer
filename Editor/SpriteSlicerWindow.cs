using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteSlicerWindow : EditorWindow
{
    private Texture2D sprite;
    private string sliceName = string.Empty;
    private string category = string.Empty;
    //private int slices = 1;
    private int width = 60;
    private int height = 60;
    private int offsetX = 780;
    private int offsetY = 834;
    private int paddingX = 14;
    private int paddingY = 0;
    private int rows = 1;
    private int cols = 1;

    [MenuItem("Window/Custom Sprite Slicer")]
    public static void OpenWindow()
    {

        SpriteSlicerWindow window = GetWindow<SpriteSlicerWindow>();
        window.titleContent = new GUIContent("Custom Sprite Slicer by Bonus630");
        window.minSize = new Vector2(140, 320);
    }
    private void OnGUI()
    {
        sprite = (Texture2D)EditorGUILayout.ObjectField("Sprite", sprite, typeof(Texture2D), false);
        sliceName = EditorGUILayout.TextField("Name", sliceName);
        category = EditorGUILayout.TextField("Category", category);
        //slices = EditorGUILayout.IntField("Slices", slices);
        rows = EditorGUILayout.IntField("Rows", rows);
        cols = EditorGUILayout.IntField("Columns", cols);
        width = EditorGUILayout.IntField("Width", width);
        height = EditorGUILayout.IntField("Height", height);
        offsetX = EditorGUILayout.IntField("Offset X", offsetX);
        offsetY = EditorGUILayout.IntField("Offset Y", offsetY);
        paddingX = EditorGUILayout.IntField("Padding X", paddingX);
        paddingY = EditorGUILayout.IntField("Padding Y", paddingY);
        if (GUILayout.Button("Generate"))
        {
            if (sprite != null)
                Slice();
            else
                Debug.Log("Select a Sprite Sheet!");

        }
        else
        {
            //EditorUtility.DisplayDialog("Erro", "Por favor, selecione um sprite!", "OK");
        }
    }
    private void PrepareRegion(string path)
    {
        //Unity use Y bottom - up
        int textureWidth = cols * width;
        int c = 1;
       
        Texture2D texture = new Texture2D(cols * rows * width, height);
        texture.filterMode = FilterMode.Point;
        int x = offsetX;
        int y = offsetY;
        for (int i = 0; i < cols * rows; i++)
        {
            Color[] pixels = sprite.GetPixels(x, y, width , height );
            texture.SetPixels(width * i, 0, width, height, pixels);
            x += width + paddingX;
            c++;
            if (c > cols)
            {
                c = 1;
                x = offsetX;
                y -= (height + paddingY);
            }
        }
        texture.Apply();
        byte[] buffer = texture.EncodeToPNG();
        File.WriteAllBytes(path, buffer);
        AssetDatabase.Refresh();

    }
    private void Slice()
    {
        //  EditorUtility.DisplayDialog(sliceName, "Por favor, selecione um sprite!", "OK");
        string path = AssetDatabase.GetAssetPath(sprite);
        string newPath = path.Substring(0,path.LastIndexOf('/') + 1);
        if (!string.IsNullOrEmpty(category))
        {
            newPath += category + '/';
            Directory.CreateDirectory(newPath);
        }
        newPath += sliceName + ".png";

        if(File.Exists(newPath))
        {
            Debug.Log("File already exists!");
            //EditorUtility.DisplayDialog("Warning!", "File already exists!", "OK");
            return;
            
        }
        PrepareRegion(newPath);
        TextureImporter importer = TextureImporter.GetAtPath(newPath) as TextureImporter;

        if (importer == null)
            return;

        importer.isReadable = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Point;
        importer.spriteImportMode = SpriteImportMode.Multiple;

        int x = 0;

        SpriteMetaData[] metaDatas = new SpriteMetaData[cols * rows];
        for (int i = 0; i < cols * rows; i++)
        {
            metaDatas[i] = new SpriteMetaData
            {
                name = string.Format("{0}_{1}", sliceName, i),
                rect = new Rect(x, 0, width, height),
                alignment = (int)SpriteAlignment.Center
            };
            x += width;
        }
        importer.spritesheet = metaDatas;
        AssetDatabase.ImportAsset(newPath, ImportAssetOptions.ForceUpdate);

       // EditorUtility.DisplayDialog("Slices", newPath, "OK");
    }
}
