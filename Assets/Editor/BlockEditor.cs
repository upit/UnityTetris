// Author: Upit      Date: 08.09.2019
using UnityEditor;
using UnityEngine;
using XD.TETRIS.BLOCKS;

/// <summary> Хэлпер для редактирования фигур в инспекторе. </summary>
[CustomEditor(typeof(Block))]
public class BlockEditor : Editor
{
	private string strBlockWidth  = "";
    private int    blockWidth     = 0;
    private string strBlockHeight = "";
    private int    blockHeight    = 0;

    private bool[,] blockData;    // Двумерный массив данных о фигуре. 
    
    /// <summary> Override стандартного инспектора для ScriptableObject типа Block </summary>
    public override void OnInspectorGUI()
    {
        GUILayout.Space(25);
        Block block = (Block)target;

        // Если поля пустые - берем данные из сериализованного ScriptableObject.
        if (strBlockWidth == "" || strBlockHeight == "")
        {
	        Vector2Int size = block.blockData.GetSize();
            blockWidth = size.x;
	        blockHeight = size.y;
	        strBlockWidth = blockWidth.ToString();
	        strBlockHeight = blockHeight.ToString();
            
	        blockData = block.blockData.Get();
        }
       
        
        // Создание пустой фигуры заданой ширины и высоты. 
        GUILayout.BeginHorizontal();
        {
            GuiLabel("Ширина:");
            strBlockWidth = GUILayout.TextField(strBlockWidth, GUILayout.Width(20));
            GuiLabel("Высота:");
            strBlockHeight = GUILayout.TextField(strBlockHeight, GUILayout.Width(20));
            if (GUILayout.Button("Применить", GUILayout.Width(100)))
            {
                blockWidth = int.Parse(strBlockWidth);
                blockHeight = int.Parse(strBlockHeight);
                block.blockData=new BlockData(blockWidth, blockHeight);
                blockData = block.blockData.Get();
            }
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(25);
        
        // Сетка для редактирования блоков фигуры.
        for (int y = 0; y < blockHeight; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < blockWidth; x++)
            {
                int invertedY = blockHeight - y - 1;
                GUI.color = blockData[x, invertedY] ? Color.green : Color.white;
                if (GUILayout.Button("", GUILayout.Width(18)))
                {
                    blockData[x, invertedY] = !blockData[x, invertedY];
                }
            }
            GUILayout.EndHorizontal();
        }
        
        GUILayout.Space(25);
        GUI.color = Color.white;
        
        // Сохранение данных и сериализации фигуры.
        if (GUILayout.Button("Сохранить", GUILayout.Width(100)))
        {
            block.blockData.Set(blockData);
            EditorUtility.SetDirty(target);
        }
        
        // Выбор спрайта для блоков (кусков) фигуры.
        SerializedObject pieceSprite = new SerializedObject(target);
        EditorGUILayout.PropertyField(pieceSprite.FindProperty("pieceSprite"));
        pieceSprite.ApplyModifiedProperties();

    }

    /// <summary> Создание подписей шириной, кратной кол-ву букв. </summary>
    /// <param name="text">Текст надписи.</param>
    private static void GuiLabel(string text)
    {
        GUILayout.Label(text, GUILayout.Width(text.Length * 8));
    }
}