using System.Collections.Generic;
using Text2Scene;

/// <summary>
/// Small helper class for the Text2SceneHandler
/// </summary>
public class SentenceObject
{
    public ClassifiedObject ClassifiedObject;
    public int Index;
    public bool IsPlaced;

    public SentenceObject(ClassifiedObject classifiedObject, int index)
    {
        ClassifiedObject = classifiedObject;
        Index = index;
        IsPlaced = false;
    }

    public override bool Equals(object obj)
    {
        return obj is SentenceObject @object &&
               EqualityComparer<ClassifiedObject>.Default.Equals(ClassifiedObject, @object.ClassifiedObject) &&
               Index == @object.Index &&
               IsPlaced == @object.IsPlaced;
    }

    public override int GetHashCode()
    {
        int hashCode = 904256680;
        hashCode = hashCode * -1521134295 + EqualityComparer<ClassifiedObject>.Default.GetHashCode(ClassifiedObject);
        hashCode = hashCode * -1521134295 + Index.GetHashCode();
        hashCode = hashCode * -1521134295 + IsPlaced.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(SentenceObject left, SentenceObject right)
    {
        return EqualityComparer<SentenceObject>.Default.Equals(left, right);
    }

    public static bool operator !=(SentenceObject left, SentenceObject right)
    {
        return !(left == right);
    }
}