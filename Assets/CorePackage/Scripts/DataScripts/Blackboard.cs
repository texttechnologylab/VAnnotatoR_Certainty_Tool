using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.tvOS;

public class Blackboard
{
    private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

    
    public bool Contains(string variableName)
    {
        return _dictionary.ContainsKey(variableName);
    }

    public bool Remove(string variableName)
    {
        return _dictionary.Remove(variableName);
    }

    public void Set<T>(string variableName, T newValue)
    {
        if (_dictionary.TryGetValue(variableName, out var value) && !(value is T))
            throw new InvalidOperationException($"Can't set variable '{variableName}' to '{value}': The variable in the blackboard has type '{value.GetType()}' which is different from the type of the new value'{typeof(T)}'!");
        
        _dictionary[variableName] = newValue;
    }

    public T Get<T>(string variableName)
    {
        var value = _dictionary[variableName];
        if(!(value is T castedValue))
            throw new InvalidOperationException($"Can't get variable '{variableName}' of type '{typeof(T)}: Variable in the blackboard is of type '{value.GetType()}'!");
        return castedValue;
    }

    public bool TryGet<T>(string variableName, out T value)
    {
        if (!_dictionary.ContainsKey(variableName))
        {
            value = default(T);
            return false;
        }

        value = Get<T>(variableName);
        return true;
    }

    public T GetOrDefault<T>(string variableName)
    {
        if (!TryGet<T>(variableName, out var value))
            value = default;
        return value;
    }
}
