// ENGINE SCRIPT: AVOID PUTTING GAME SPECIFIC CODE IN HERE
/**
 * NOTE: SCRIPT WAS PORTED FROM FLASH
 *       THE MAIN PROBLEM HOWEVER IS THAT IN FLASH WE CAN GET AWAY WITHOUT TYPES (IN DICTIONARIES)
 *       IN C# WE CAN'T! THEREFORE CLASS DICENTRY HAS BEEN INTRODUCED WHICH CONTAINS THE TYPES THAT CAN BE CONTAINED
 *       A HUGE DISADVANTAGE FOR CODERS IS THAT THEY HAVE TO KNOW WHAT IS IN A DICENTRY (well they should know anyway else code will crash even in actionscript)
 *       FOR EXAMPLE YOU GET CODE LIKE: pShared["Cars"].d["Panini"].d["Grip"].f
 *
 * At a later date I saw this type: Hashtable  this *might* resolve the somewhat required silly .d .i .s properties  (doubt it though)
 * Would also be handy to implement a .v type for a 3D vector  (or .v2/.v3 for 2D/3D vector)
 *
 * TextLoader takes a textfile, parses it and puts the parsed data in to the specified variable as a dictionary
 * Static: this reads a textfile and afterwards you don't need it anymore, so doesn't really make sense to create an instance of it
 * @author Stijn Stiefelhagen
 */

// Stijn 13/08/2013 - Updated TextLoader to be able to load both windows as well unix style line endings  (I suspect this will fix our Android savedata issue)
// Stijn 04/10/2013 - Nexus or Android 4.3 fix

// TODO Regarding Windows 8, we should force a proper decimalseparator or the values will be parsed wrong! (might be good anyway)
// TODO make sure that a string like "blabla/bla.txt" can be parsed without errors

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DicEntry  // a class not a struct, I need references (class), not values (structs)
{
	public enum EntryType {BOOL = 1, INT, FLOAT, STRING, DICT, LIST};
	public EntryType type;
	public DicEntry(bool aB) {type = EntryType.BOOL; b = aB;}
	public DicEntry(int aI)  {type = EntryType.INT; i = aI;}
	public DicEntry(float aF) {type = EntryType.FLOAT; f = aF;}
	public DicEntry(string aS) {type = EntryType.STRING; s = aS;}
	public DicEntry(Dictionary<string, DicEntry> aD) {type = EntryType.DICT; d = aD;}
	public DicEntry(List<DicEntry> aL) {type = EntryType.LIST; l = aL;}
	public DicEntry(DicEntry aDicEntry)
	{  // copy constructor, does not deep copy
		type = aDicEntry.type;
		switch(type)
		{
			case EntryType.BOOL: _b = aDicEntry.b; break;
			case EntryType.INT: _i = aDicEntry.i; break;
			case EntryType.FLOAT: _f = aDicEntry.f; break;
			case EntryType.STRING: _s = aDicEntry.s; break;
			case EntryType.DICT: _d = aDicEntry.d; break;
			case EntryType.LIST: _l = aDicEntry.l; break;
			default: throw new UnityException("Copy constructor: unknown type");
		}
	}

	public bool IsComplexProp() {return (type == EntryType.LIST || type == EntryType.DICT);}  // a complex property is one that combines basic ones

	private bool _b;
	private int _i;
	private float _f;
	private string _s;
	private Dictionary<string, DicEntry> _d;
	private List<DicEntry> _l;

	public bool b
	{	get { if (type != EntryType.BOOL) throw new UnityException("Got uninitialized boolean"); return _b; }
		set { _b = value; }
	}
	public int i
	{	get { if (type != EntryType.INT) throw new UnityException("Got uninitialized int"); return _i; }
		set { _i = value; }
	}
	public float f
	{	get { if (type != EntryType.FLOAT) throw new UnityException("Got uninitialized float"); return _f; }
		set { _f = value; }
	}
	public string s
	{	get { if (type != EntryType.STRING) throw new UnityException("Got uninitialized string"); return _s; }
		set { _s = value; }
	}
	public Dictionary<string, DicEntry> d
	{	get { if (type != EntryType.DICT) throw new UnityException("Got uninitialized dictionary"); return _d; }
		set { _d = value; }
	}
	public List<DicEntry> l
	{	get { if (type != EntryType.LIST) throw new UnityException("Got uninitialized list"); return _l; }
		set { _l = value; }
	}
};

public class TextLoader
{
	// public vars
	// private vars
	static private Dictionary<string, DicEntry> pDictionaryVar = null;
	static private string pCurrentParseSetName = "";  // when empty not parsing lines within a set
	static private string pText = "";
	static private int pProgress = 0;  // parsing progress within file
	static private int pLine = 0;  // line progress within file
	static private int pTextLength = 0;  // = _text.length
	static private string pSaveString = "";  // used when saving

	// a private constructor, it should never be called
	private TextLoader()
	{
		throw new UnityException("There's no need to create an instance of this class. Use TextLoader.loadText instead.");
	}

	// Loads text (string, formatted in our usual way) into a dictionary where we can access the data
	static public void LoadText(string aTextFileData, Dictionary<string, DicEntry> aVarDic)
	{
		pDictionaryVar = aVarDic;
		pCurrentParseSetName = "";
		pText = aTextFileData;
		pProgress = 0;  // the first character seems to be some strange character, is this a bug? Changed it back from 1 to 0, seems to work fine
		//char test1 = pText[0];
		pLine = 0;
		pTextLength = pText.Length;

		while (pProgress < pTextLength)
		{
			++pLine;
			pParseLine();
		}
	}

	// Returns text (string, formatted in our usual way) from a dictionary
	static public string SaveText(Dictionary<string, DicEntry> aVarDic) { return SaveText(aVarDic, null); }
	static public string SaveText(Dictionary<string, DicEntry> aVarDic, List<string> aSaveList)
	{
		if (aSaveList == null)
		{  // aSaveList not provided, check for a SaveList property within the given dictionary
			if (!aVarDic.ContainsKey("SaveList")) throw new UnityException("TextLoader::saveText, no save list was given or found");

			// fill save list
			aSaveList = new List<string>();
			foreach (DicEntry tDicEntry in aVarDic["SaveList"].l)
				aSaveList.Add(tDicEntry.s);
		}

		// here we have a valid SaveList
		pSaveString = "";

		foreach (string tSaveStr in aSaveList)
		{
			if (!aVarDic.ContainsKey(tSaveStr))
			{
				Debug.LogError("TextLoader::saveText, property " + tSaveStr + " was not found in given dictionary! Saving of this property is skipped.");
				continue;
			}
			pSaveMainProp(aVarDic, tSaveStr);
			//Debug.Log("SAVETEXT: " + tSaveStr);
			pSaveString += "\r\n";
		}

		return pSaveString;
	}


	// private functions

	////////////////////////////////////////////////////////
	// Loading

	static private void pSkipWhiteSpace()
	{
		string tChar = "";
		do
		{
			tChar = (pText[pProgress]).ToString();
			++pProgress;
		} while ((pProgress < pTextLength) && (tChar == " ") || (tChar == "\t"));  // || (tChar == "\t"))
		--pProgress;  // we went one too far so go back one
	}


	static private string pRemoveTrailingWhiteSpace(string aString)
	{
		int i = aString.Length - 1;
		string tChar;
		do
		{
			tChar = aString[i].ToString();
		}
		while ((i-- >= 1) && ((tChar == " ") || (tChar == "\t")));
		return aString.Substring(0, i+2);
	}

	static private bool pTestForNewLine()
	{
		char tChar = pText[pProgress];
		if (tChar != '\r' && tChar != '\n') return false;
		if (tChar == '\r')
		{  // there must be a line feed after a carriage return, check it
			if ((pText[pProgress + 1]).ToString() != "\n") throw new UnityException("No Line Feed (/n) after a Carriage Return (/r)");  // our files should be saved with \r\n (windows style line ending)
		}
		return true;
	}

	// function does not advance progress: only detects
	static private bool pTestForString(string aString)
	{
		int tStrProgress = 0;
		int tLength = aString.Length;
		while ((tStrProgress != tLength) &&  // not yet fully detected the string
		       (pText[pProgress + tStrProgress] == aString[tStrProgress]))  // chars should be equal
		{
			++tStrProgress;
		}
		return (tStrProgress == tLength);  // if tStrProgress == tLength then we succesfully detected the string else we didn't
	}

	// this function differs from pTestForString in that in searches accross an entire line, not just at _progress
	// function does not advance progress: only detects
	static private bool pTestForStringWithinLine(string aString)
	{
		string tLine = "";  // capture line
		char tChar1 = '0';
		char tChar0 = '0';  // character before tChar1
		int tLineProgress = 0;

		while (true)
		{
			if (pProgress+tLineProgress == pText.Length) break;  // (SS) 04/10/2013 - Nexus or Android 4.3 fix
			tChar0 = tChar1;
			tChar1 = pText[pProgress + tLineProgress];
			if ((tChar1 == '\r') || (tChar1 == '\n') || ((tChar1 == '/') && (tChar0 == '/'))) break;  // out of loop -> newline or comment detected
			tLine = tLine + tChar1;
			++tLineProgress;
		}

		return (tLine.IndexOf(aString) != -1);  // check if given aString occurs in the line
	}

	static private void pReadString(string aString)
	{
		int tStrProgress = 0;
		int tLength = aString.Length;
		while ((tStrProgress != tLength) &&  // not yet fully detected the string
		       (pText[pProgress] == aString[tStrProgress]))  // chars should be equal
		{
			++tStrProgress;
			++pProgress;
		}
		if (tStrProgress != tLength)  // if tStrProgress != tLength then we didn't detect what we were supposed to read
		{
			throw new UnityException("Did not read " + aString + " in the following " + pText.Substring(pProgress - 10, pProgress + 10));  // todo also mention file name
		}
	}

	static private void pSkipUntilNewLine()  // useful when a comment was detected
	{
		while ((pProgress < pTextLength) && pText[pProgress] != '\r' && pText[pProgress] != '\n')
		{
			++pProgress;  // skip until we get to \r or \n
		}
		if (pText[pProgress] == '\r') ++pProgress;  // skip the \r which should always be followed by a \n
		++pProgress;  // skip \n
	}

	static private string pGetIdentifier()  // can't do default parameters :S  use function overloading instead
	{
		return pGetIdentifier("");
	}
	static private string pGetIdentifier(string aStopChar)  // aStopChar can be given to stop identifying further
	{
		string tIdentifier = "";
		string tChar = pText[pProgress].ToString();

		while ((tChar != " ") && (tChar != "\t") && (tChar != ",") && (tChar != aStopChar) && (pProgress < pTextLength))
		{
			tIdentifier = tIdentifier + tChar;
			++pProgress;
			tChar = pText[pProgress].ToString();
		}

		if (tIdentifier.Length == 0) throw new UnityException("Failed to read identifier at character " + tIdentifier);  // todo also mention file name
		return tIdentifier;
	}

	static private string pGetBaseValue()  // reads a base type: string, int, float or boolean (but returns as string)
	{
		string tText = "";
		char tChar = pText[pProgress];
		char tCharPrev = '0';

		// continue until newline, comma (we might be in a list), closing bracket (end of a (prop)list) or '//' has been found
		while ((tChar != '\r') && (tChar != '\n') && (tChar != ',') && (tChar != ']') && ((tCharPrev != '/') || (tChar != '/')) && (pProgress < pTextLength))
		{
			tText = tText + tChar;
			++pProgress;
			tCharPrev = tChar;
			tChar = pText[pProgress];
		}
		if ((tCharPrev == '/') && (tChar == '/')) tText = tText.Substring(0, tText.Length-1);  // if comment was found I think we have to strip off that last '/' which added but shouldn't have
		return pRemoveTrailingWhiteSpace(tText);
	}

	// cannot use default parameters (is it possible in c#?), using function overloading
	static private void pParseValue(Dictionary<string, DicEntry> aDictionary) { pParseValue(aDictionary, "", null); }
	static private void pParseValue(Dictionary<string, DicEntry> aDictionary, string aProperty) { pParseValue(aDictionary, aProperty, null); }
	static private void pParseValue(Dictionary<string, DicEntry> aDictionary, string aProperty, List<DicEntry> aList)  // values will be stored within the aDictionary dictionary under aProperty -> aDictionary[aProperty] = <parsed value>
	{
		string tString;
		if (pText[pProgress].ToString() == "[")
		{  // parsing a list or a proplist
			if (pText[pProgress + 1].ToString() == "#")
			{  // proplist (dictionary)
				++pProgress;  // progress 1 characters: '[', we will read the '#' in the do-while loop
				//Dictionary<string, DicEntry> tDictionary = new Dictionary<string, DicEntry>();  // create new dictionary for this proplist
				// TODO could be we have a dictionary in a list
				//if (aList != null) aList.Add();  // anArray.push(tDictionary);
				//else               aDictionary[aProperty] = tDictionary;

				aDictionary[aProperty] = new DicEntry(new Dictionary<string, DicEntry>());

				do
				{
					pSkipWhiteSpace();
					pReadString("#");
					tString = pGetIdentifier(":");
					pSkipWhiteSpace();
					pReadString(":");
					pSkipWhiteSpace();
					pParseValue(aDictionary[aProperty].d, tString);
					pSkipWhiteSpace();
				} while (pText[pProgress++].ToString() == ",");  // note: also increases the progress
				--pProgress;  // went one too far, we didn't read a ","
			} else
			{  // list (array)
				++pProgress;  // progress 1 character: '['

				// TODO could be we have a list in a list, then aProperty is not applicable! Think we should use: if (aList != null)  ....
				aDictionary[aProperty] = new DicEntry(new List<DicEntry>());  // create array for this list

				pSkipWhiteSpace();
				if (pText[pProgress].ToString() != "]")  // check for the empty list
				{
					do
					{
						pSkipWhiteSpace();
						pParseValue(null, "", aDictionary[aProperty].l); // (aDictionary[aProperty]) as Array);
						pSkipWhiteSpace();
					} while (pText[pProgress++].ToString() == ",");  // note: also increases the progress
					--pProgress;  // went one too far, we didn't read a ","
				}
			}
			// skip white space and read a closing ']'
			pSkipWhiteSpace();
			pReadString("]");
		} else
		{  // parsing a base type: string, int, float or boolean
			tString = pGetBaseValue();
			// find out if tString is a string, int, float or boolean now
			int tIntVal;
			if (int.TryParse(tString, out tIntVal))  // attempt to parse as an int
			{  // it's an int
				if (aList == null) aDictionary[aProperty] = new DicEntry(tIntVal);
				else               aList.Add(new DicEntry(tIntVal));
			} else
			{  // it's not an int
				float tFloatVal;
				if (float.TryParse(tString, out tFloatVal))  // attempt to parse as a float
				{  // it's a float
					if (aList == null) aDictionary[aProperty] = new DicEntry(tFloatVal);
					else               aList.Add(new DicEntry(tFloatVal));
				} else
				{  // it's not a float or an int
					// check if it's a boolean or a string  (both are a string ofcourse, but we say true and false are booleans
					if ((tString == "true") || (tString == "True") || (tString == "false") || (tString == "False"))
					{  // it's a boolean
						if ((tString == "true") || (tString == "True"))
							if (aList == null) aDictionary[aProperty] = new DicEntry(true);
							else               aList.Add(new DicEntry(true));
						else
							if (aList == null) aDictionary[aProperty] = new DicEntry(false);
							else               aList.Add(new DicEntry(false));
					} else
					{  // it's a string
						if (aList == null) aDictionary[aProperty] = new DicEntry(tString);
						else               aList.Add(new DicEntry(tString));
					}
				}
			}
		}
	}

	static private void pParseLine()
	{
		pSkipWhiteSpace();
		if (pTestForNewLine())  // check empty line
		{
			pSkipUntilNewLine();
			return;
		}

		if (pTestForString("//"))  // check for comment
		{
			pSkipUntilNewLine();
			return;
		}

		string tIdentifier;
		if (pCurrentParseSetName == "")
		{  // we could be seeing a xxxx_Begin tag now or just a variable name
			if (pTestForStringWithinLine("_Begin"))
			{  // we see a '_Begin' tag, get identifier up to the underscore
				tIdentifier = pGetIdentifier("_");
				// Won't test with pTestForString("_Begin") cause this is what we should be reading at this point
				pReadString("_Begin");
				pCurrentParseSetName = tIdentifier;
				pDictionaryVar[pCurrentParseSetName] = new DicEntry(new Dictionary<string, DicEntry>());
			} else
			{  // we don't see a begin tag: just a variable name
				pSkipWhiteSpace();  // 04/10/2013 - Nexus fix
				if (pProgress == pText.Length-1)
				{
					pProgress = pText.Length;  // forces end of file
					return;
				}
				tIdentifier = pGetIdentifier();
				pSkipWhiteSpace();
				pParseValue(pDictionaryVar, tIdentifier);
			}
		} else
		{  // pCurrentParseSetName != ""
			// test for _End tag
			if (pTestForString(pCurrentParseSetName + "_End"))
			{
				pReadString(pCurrentParseSetName + "_End");
				pCurrentParseSetName = "";
			} else
			{  // no end tag, read variable
				tIdentifier = pGetIdentifier();
				pSkipWhiteSpace();
				pParseValue(pDictionaryVar[pCurrentParseSetName].d, tIdentifier);
			}
		}
		pSkipUntilNewLine();  // anything after this must be commented, so ignore it, next line
	}

	////////////////////////////////////////////////////////
	// Saving

	static private void pSaveMainProp(Dictionary<string, DicEntry> aVarDic, string aProp)
	{
		pSaveString += aProp;  // write property name
		pSaveString += " ";  //  write empty space separator
		pSaveProp(aVarDic[aProp]);
	}

	static private void pSaveProp(DicEntry aDicEntry)
	{
		if (aDicEntry.IsComplexProp())
		{
			if (aDicEntry.type == DicEntry.EntryType.DICT) pSaveDict(aDicEntry);
			else                                           pSaveList(aDicEntry);  // otherwise it's a DicEntry.EntryType.List
		} else
		{
			pSaveBaseProp(aDicEntry);
		}
	}

	static private void pSaveBaseProp(DicEntry aDicEntry)
	{
		switch(aDicEntry.type)
		{
			case DicEntry.EntryType.BOOL:
				if (aDicEntry.b) pSaveString += "true";
				else             pSaveString += "false";
				break;
			case DicEntry.EntryType.STRING:
				pSaveString += aDicEntry.s;
				break;
			case DicEntry.EntryType.FLOAT:
				//Debug.Log (" WTF!!!" + aDicEntry.f.ToString());
				// FIX TO MAKE SURE WE CAN SAVE FULL INT as FLOAT i.e. we need to save 1 as 1.0
				string tEntry = aDicEntry.f.ToString(); 
				if (!tEntry.Contains(".")) tEntry = tEntry+".0";
				pSaveString += tEntry;
				break;
			case DicEntry.EntryType.INT:
				pSaveString += aDicEntry.i.ToString();
				break;
		}
	}

	static private void pSaveDict(DicEntry aDicEntry)
	{
		pSaveString += "[";  // opening bracket for a dictionary
		int tEntrycount = aDicEntry.d.Count;
		if (tEntrycount == 0) throw new UnityException("Attempted to write an empty dictionary, this is currently not possible.");  // All dictionaries need to have at least one entry
		foreach (KeyValuePair<string, DicEntry> tPair in aDicEntry.d)
		{
			pSaveString += "#";
			pSaveString += tPair.Key;
			pSaveString += ":";
			pSaveProp(tPair.Value);
			--tEntrycount;
			if (tEntrycount > 0) pSaveString += ",";  // avoid writing a comma at the last entry
		}
		pSaveString += "]";  // closing bracket for a dictionary
	}

	static private void pSaveList(DicEntry aDicEntry)
	{
		pSaveString += "[";  // opening bracket for a list
		int tEntrycount = aDicEntry.l.Count;
		foreach (DicEntry tDicEntry in aDicEntry.l)
		{
			pSaveProp(tDicEntry);
			--tEntrycount;
			if (tEntrycount > 0) pSaveString += ",";  // avoid writing a comma at the last entry
		}
		pSaveString += "]";  // closing bracket for a list
	}
}
