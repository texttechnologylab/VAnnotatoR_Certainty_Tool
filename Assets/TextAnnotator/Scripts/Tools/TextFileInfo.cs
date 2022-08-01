using System;

public class TextFileInfo {
    
    public string Language { get; private set; }
    public string CategoryID { get; private set; }
    public DateTime LastChanged { get; private set; }

    public TextFileInfo(string language, string categoryID, DateTime lastChanged)
    {
        Language = language;
        CategoryID = categoryID;
        LastChanged = lastChanged;
    }

    
}
