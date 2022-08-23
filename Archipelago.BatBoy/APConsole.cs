using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BepInEx;

namespace Archipelago.BatBoy;

public class APConsole : MonoBehaviour
{
    public Font font;
    void Start()
    {
        TextGenerationSettings settings = new TextGenerationSettings();
        settings.textAnchor = TextAnchor.LowerCenter;
        settings.color = Color.red;
        settings.generationExtents = new Vector2(500.0F, 200.0F);
        settings.pivot = Vector2.zero;
        settings.richText = true;
        settings.font = font;
        settings.fontSize = 32;
        settings.fontStyle = FontStyle.Normal;
        settings.verticalOverflow = VerticalWrapMode.Overflow;
        
        TextGenerator generator = new TextGenerator();
        generator.Populate("I am a string", settings);
        print("I generated: " + generator.vertexCount + " verts!");
    }

    public void ReceiveItem(string itemReceived)
    {
        print($"Received {itemReceived}!");
    }

    public void SendRedSeed(int levelIndex)
    {
    }
}