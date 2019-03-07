
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class CreateMaterialsForTextures : EditorWindow
{
    public Shader shader;

    private string NormalExtension;
    private string AlbedoExtension;
    private string MetallicExtension;
    private string CustomMaterialLocation;
   
    private bool CustomMaterialLocationBool;
    private bool CustomTextureOptions;
    private bool CrunchCompress;

    private string Instructions = "Input extension of your maps the extension must be seperated from the name of the texture by _" +
            ", then select all the textures you want to become materials. Set your paramerters in material export location and whatever map options you want" +
            " then click start.";

    private string[] TextureSize = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
    private string[] ShaderName = new string[] { "Standard" };

    private int TileX;
    private int TileY;
    private int MaxTextureSize;

    private int CurrentTextureSizeIndex = 5;
    private int CrunchCompressionAmount = 50;

    private int ShaderNameIndex = 0;
    [SerializeField] private string[] ShaderParameter;
    [SerializeField] private string[] ImageParameter;

    [MenuItem("Tools/Create Materials From Textures")]
   
    public static void ShowWindow()
    {
        GetWindow<CreateMaterialsForTextures>(false, "Create Materials From Textures", true);
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Extensions", EditorStyles.boldLabel);
        ShaderNameIndex = EditorGUILayout.Popup(ShaderNameIndex, ShaderName);

        switch (ShaderNameIndex)
        {
            case 0:
                AlbedoExtension = EditorGUILayout.TextField("Extension of Albedo map", AlbedoExtension);
                NormalExtension = EditorGUILayout.TextField("Extension of Normal map", NormalExtension);
                MetallicExtension = EditorGUILayout.TextField("Extension of Metallic map", MetallicExtension);
                break;

        }
        

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Material Settings",EditorStyles.boldLabel);
        
        CustomMaterialLocationBool = EditorGUILayout.Toggle("Material Export Location", CustomMaterialLocationBool);

        if (CustomMaterialLocationBool)
        {
            CustomMaterialLocation = EditorGUILayout.TextField("Custom location to place", CustomMaterialLocation);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Texture Settings",EditorStyles.boldLabel);
        CustomTextureOptions = EditorGUILayout.Toggle("Change Map Options", CustomTextureOptions);


        if (CustomTextureOptions)
        {
            CurrentTextureSizeIndex = EditorGUILayout.Popup(CurrentTextureSizeIndex, TextureSize);
            CrunchCompress = EditorGUILayout.Toggle("Crunch Texture Maps", CrunchCompress);

            EditorGUILayout.BeginHorizontal("box");

            TileX = EditorGUILayout.IntField("X Tile", TileX);
            TileY = EditorGUILayout.IntField("Y Tile", TileY);
            //GUILayout.Button("I'm to the right");

            EditorGUILayout.EndHorizontal();
            if (CrunchCompress)
            {
                CrunchCompressionAmount = EditorGUILayout.IntSlider("Compressor Quality",CrunchCompressionAmount, 5, 100);
            }

        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Start"))
        {
            CreateMaterialsFromTextures();
        }

        EditorGUILayout.LabelField("Comments");
        EditorStyles.textArea.wordWrap = true;
        //EditorStyles.textArea.wordWrappedLabel;
        EditorGUILayout.TextArea(Instructions, EditorStyles.textArea);
        //EditorGUILayout.EndScrollView();
        
    }

    void OnEnable()
    {
        shader = Shader.Find("Standard");
    }

    void CreateMaterialsFromTextures()
    {
        
        try
        {
            AssetDatabase.StartAssetEditing();
            var textures = Selection.GetFiltered(typeof(Texture), SelectionMode.Assets).Cast<Texture>();
            foreach (var tex in textures)
            {

                string Path = AssetDatabase.GetAssetPath(tex);
                if (!Path.Contains("_"))
                {
                    continue;
                }


                TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(Path);
                Debug.Log(Path);
                List<string> SplitResult = Path.Split('_').ToList();
                
                List<string> SplitResultFileType = SplitResult[SplitResult.Count - 1].Split('.').ToList();
                
                List<string> SplitResultWithoutExtension = SplitResult;
                    SplitResultWithoutExtension.RemoveAt(SplitResult.Count - 1);
                

                string CombinedResultMinusExtension = string.Join("", SplitResultWithoutExtension);
                Debug.Log(CombinedResultMinusExtension);
                string currentPath = CombinedResultMinusExtension + ".mat";
                Debug.Log(currentPath);

                

                var mat = new Material(shader);
                
                if (AssetDatabase.LoadAssetAtPath(currentPath, typeof(Material)) == null)
                {
                    mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, currentPath);
                    Debug.Log("creating asset");
                }
                else
                {
                    mat = AssetDatabase.LoadAssetAtPath(currentPath, typeof(Material)) as Material;
                    Debug.Log("loading asset");
                }

                switch (ShaderNameIndex)
                {
                    case 0:
                        if (SplitResultFileType[0] == AlbedoExtension)
                        {
                            Debug.Log("setting albedo map");
                            mat.mainTexture = tex;
                        }

                        if (SplitResultFileType[0] == NormalExtension)
                        {
                            Debug.Log("setting normal map");
                            mat.EnableKeyword("_Normal");

                            mat.SetTexture("_BumpMap", tex);
                            if (CustomTextureOptions)
                            {
                                importer.textureType = TextureImporterType.NormalMap;
                                EditorUtility.SetDirty(importer);
                                importer.SaveAndReimport();
                            }
                        }

                        if (SplitResultFileType[0] == MetallicExtension)
                        {
                            Debug.Log("setting normal map");
                            mat.EnableKeyword("_MetallicGlossMap");

                            mat.SetTexture("_MetallicGlossMap", tex);
                        }
                        break;

                   
                }

                
                if (CustomTextureOptions)
                {
                    importer.maxTextureSize = int.Parse(TextureSize[CurrentTextureSizeIndex]);

                    if (CrunchCompress)
                    {
                        importer.crunchedCompression = true;
                        importer.compressionQuality = CrunchCompressionAmount;
                    }
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                }
                
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
    }
}