using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<Key, Value> : IEnumerable<KeyValuePair<Key, Value>>
{
	[System.Serializable]
	public struct SmartDictionaryKeyPairValue
	{
		[SerializeField] public Key m_key;
		[SerializeField] public Value m_value;
	}

	[SerializeField] protected List<SmartDictionaryKeyPairValue> m_Dictionnary = new List<SmartDictionaryKeyPairValue>();

	// Constructor that takes a Dictionary as an argument
	public SerializableDictionary(Dictionary<Key, Value> dictionary)
	{
		foreach (var kvp in dictionary)
		{
			SmartDictionaryKeyPairValue pair = new SmartDictionaryKeyPairValue
			{
				m_key = kvp.Key,
				m_value = kvp.Value
			};
			
			m_Dictionnary.Add(pair);
		}
	}
	
	public Dictionary<Key, Value> ToDictionary()
	{
		Dictionary<Key, Value> toReturn = new Dictionary<Key, Value>();

		for (int i = 0; i < m_Dictionnary.Count; i++)
		{
			toReturn.Add(m_Dictionnary[i].m_key, m_Dictionnary[i].m_value);
		}

		return toReturn;
	}

	public bool ContainsKey(Key a_key)
	{
		for (int i = 0; i < m_Dictionnary.Count; i++)
		{
			if (m_Dictionnary[i].m_key.Equals(a_key))
			{
				return true;
			}
		}
		return false;
	}

	public bool ContainsValue(Value a_value)
	{
		for (int i = 0; i < m_Dictionnary.Count; i++)
		{
			if (m_Dictionnary[i].m_value.Equals(a_value))
			{
				return true;
			}
		}
		return false;
	}

	public Value this[Key a_Key]
	{
		get
		{
			for (int i = 0; i < m_Dictionnary.Count; i++)
			{
				if (m_Dictionnary[i].m_key.Equals(a_Key))
				{
					return m_Dictionnary[i].m_value;
				}
			}

			throw new KeyNotFoundException();
		}
	}

	public List<Value> Values()
	{
		List<Value> result = new List<Value>();
		foreach (SmartDictionaryKeyPairValue pair in m_Dictionnary)
		{
			result.Add(pair.m_value);
		}

		return result;
	}

	public void Add(Key a_key, Value a_value)
	{
		SmartDictionaryKeyPairValue pair = new SmartDictionaryKeyPairValue();
		pair.m_key = a_key;
		pair.m_value = a_value;
		m_Dictionnary.Add(pair);
	}

	public bool Remove(Key a_key)
	{
		for (int i = 0; i < m_Dictionnary.Count; i++)
		{
			if (m_Dictionnary[i].m_key.Equals(a_key))
			{
				m_Dictionnary.RemoveAt(i);
				return true;
			}
		}
		return false;
	}
	
	public SerializableDictionary() { }  // add if missing

	public IEnumerator<KeyValuePair<Key, Value>> GetEnumerator()
	{
		foreach (SmartDictionaryKeyPairValue pair in m_Dictionnary)
			yield return new KeyValuePair<Key, Value>(pair.m_key, pair.m_value);
	}

	// This explicit implementation is required — must match exactly
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}