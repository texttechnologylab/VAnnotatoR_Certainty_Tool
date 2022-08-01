using System;
using System.Collections.Generic;

public enum RadialInputType { Keyboard }

[Serializable]
public class RadialSection
{
    public string Title = "";
    public string Description = "";
    public List<RadialSection> ChildSections = null;
    public object Value;
    /*
     * Set Value to InputType.Keyboard to 
     */
    public RadialSection(string title, string description, List<RadialSection> childSections, object value=null)
    {
        Title = title;
        Description = description;
        ChildSections = childSections;
        Value = value;
    }

    public override string ToString()
    {
        return Title;
    }

}
