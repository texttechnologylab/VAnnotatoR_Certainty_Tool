using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class KeyboardLayouts {

    public enum Layout { German, Numerical };

    public static char[,][] GermanLayout = new char[,][]
    {
        {new char[] {'^','°', '\0'}, new char[] {'1','!', '\0'}, new char[] {'2','\"','²' }, new char[] {'3','§','³' }, new char[] {'4','$', '\0'}, new char[] {'5','%', '\0'}, new char[] {'6','&', '\0'}, new char[] {'7','/','{' }, new char[] {'8','(','[' }, new char[] {'9',')',']' }, new char[] {'0','=','}' }, new char[] {'ß','?','\\' }, new char[] {'´','`', '\0' } },
        {new char[] {'q','Q','@' }, new char[] {'w','W', '\0'}, new char[] {'e','E','€' }, new char[] {'r','R', '\0'}, new char[] {'t','T', '\0'}, new char[] {'z','Z', '\0'}, new char[] {'u','U', '\0'}, new char[] {'i','I', '\0'}, new char[] {'o','O', '\0'}, new char[] {'p','P', '\0'}, new char[] {'ü','Ü', '\0'}, new char[] {'+','*','~' }, null },
        {new char[] {'a','A', '\0'}, new char[] {'s','S', '\0'}, new char[] {'d','D', '\0'}, new char[] {'f','F', '\0'}, new char[] {'g','G', '\0'}, new char[] {'h','H', '\0'}, new char[] {'j','J', '\0'}, new char[] {'k','K', '\0'}, new char[] {'l','L', '\0'}, new char[] {'ö','Ö', '\0'}, new char[] {'ä','Ä', '\0'}, new char[] {'#','\'','\0' }, null },
        {new char[] {'<','>','|' }, new char[] {'y','Y', '\0'}, new char[] {'x','X', '\0'}, new char[] {'c','C', '\0'}, new char[] {'v','V', '\0'}, new char[] {'b','B', '\0'}, new char[] {'n','N', '\0'}, new char[] {'m','M', '\0'}, new char[] {',',';', '\0'}, new char[] {'.',':', '\0'}, new char[] {'-','_', '\0'}, null, null },
    };

    public static char[,][] NumericalLayout = new char[,][]
    {
        { new char[] {'7', '\0', '\0'}, new char[] {'8', '\0', '\0'}, new char[] {'9', '\0', '\0'} },
        { new char[] {'4', '\0', '\0'}, new char[] {'5', '\0', '\0'}, new char[] {'6', '\0', '\0'} },
        { new char[] {'3', '\0', '\0'}, new char[] {'2', '\0', '\0'}, new char[] {'1', '\0', '\0'} },
        { new char[] {'-', '\0', '\0'}, new char[] {'0', '\0', '\0'}, new char[] {'.', '\0', '\0'} }
    };

    public static Dictionary<Layout, char[,][]> Layouts = new Dictionary<Layout, char[,][]>()
    {
        { Layout.German, GermanLayout }, { Layout.Numerical, NumericalLayout }
    };
}
