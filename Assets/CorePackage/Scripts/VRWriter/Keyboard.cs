using System.Collections.Generic;
using UnityEngine;

public class Keyboard  {

    public KeyboardLayouts.Layout Layout;
    public Key[] Keys;
	
    public Keyboard(KeyboardLayouts.Layout layout)
    {
        Layout = layout;
        GenerateKeys();
    }

    private void GenerateKeys()
    {
        char[,][] charset = KeyboardLayouts.Layouts[Layout];
        Keys = new Key[charset.Length];
        int i = 0;
        float xPos;
        float yPos = charset.GetLength(0) / 20f;
        for (int y=0; y<charset.GetLength(0); y++)
        {
            xPos = charset.GetLength(1) / 20f;
            for (int x = 0; x < charset.GetLength(1);  x++)
            {
                if (charset[y, x] == null) continue;
                Keys[i++] = new Key(charset[y, x][0], charset[y, x][1], charset[y, x][2], xPos, yPos);
                xPos -= 0.1f;

            }
            yPos -= 0.1f;
        }                
    }
}
