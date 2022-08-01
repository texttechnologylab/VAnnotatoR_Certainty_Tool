using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextMeshProMarker : MonoBehaviour
{
    
    public struct MarkerInfo
    {
        public int Start { get; private set; }
        public int Length { get; private set; }

        public MarkerInfo(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public bool IsCharInRange(int charIndex)
        {
            return charIndex >= Start && charIndex < Start + Length;
        }
    }

    private TextMeshPro TextContent;
    private Color32[] colors;
    private Color32 color;
    private int mIndex;
    private int vIndex;
    public void Init(TextMeshPro textContent)
    {
        TextContent = textContent;
    }

    
    public void MarkText(List<MarkerInfo> markers, Color32 unmarked, Color32 marked, int textStart=0)
    {
        for (int c = 0; c < TextContent.textInfo.characterCount; c++)
        {
            color = unmarked;
            
            for (int i = 0; i < markers.Count; i++)
                if (markers[i].IsCharInRange(textStart + c))
                    color = marked;
                


            mIndex = TextContent.textInfo.characterInfo[c].materialReferenceIndex;
            vIndex = TextContent.textInfo.characterInfo[c].vertexIndex;
            colors = TextContent.textInfo.meshInfo[mIndex].colors32;
            if (colors.Length < vIndex + 3) continue;
            colors[vIndex + 0] = color;
            colors[vIndex + 1] = color;
            colors[vIndex + 2] = color;
            colors[vIndex + 3] = color;
            TextContent.textInfo.meshInfo[mIndex].colors32 = colors;
        }
        
        TextContent.UpdateVertexData();
        TextContent.renderMode = TextRenderFlags.DontRender;

    }

}
