/*
 * This file is a part of the WordNet.Net open source project.
 * 
 * Copyright (C) 2005 Malcolm Crowe, Troy Simpson 
 * 
 * Project Home: http://www.ebswift.com
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * 
 * */

using Boo.Lang;
using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using Wnlib;

namespace WordNetClasses
{
    /// <summary>
    /// Summary description for WordNetClasses.
    /// </summary>
    public class WN
	{
		public static bool hasmatch = false; // determines whether morphs are considered

		public WN(string dictpath)
		{
			//Wnlib.WNDB(dictpath);
			WNCommon.path = dictpath;
		}

		public void OverviewFor(string t, string p, ref bool b, ref SearchSet obj, ArrayList list)
		{
			PartOfSpeech pos = PartOfSpeech.of(p);
			SearchSet ss = WNDB.is_defined(t, pos);
			MorphStr ms = new MorphStr(t, pos);
			bool checkmorphs = false;
            hasmatch = false;  //TODO: if this isn't reset then morphs aren't checked on subsequent searches - check for side effects of resetting this manually

			checkmorphs = AddSearchFor(t, pos, list); // do a search
			string m;

			if (checkmorphs)
				hasmatch = true;

			if (!hasmatch)
			{
				// loop through morphs (if there are any)
				while ((m = ms.next()) != null)
					if (m != t)
					{
						ss = ss + WNDB.is_defined(m, pos);
						AddSearchFor(m, pos, list);
					}
			}
			b = ss.NonEmpty;
			obj = ss;
		}

		bool AddSearchFor(string s, PartOfSpeech pos, ArrayList list)
		{
			Search se = new Search(s, false, pos, new SearchType(false, "OVERVIEW"), 0);
			if (se.lexemes.Count > 0)
				list.Add(se);

			if (se.lexemes.Count > 0)
				return true; // results were found
			else
				return false;
		}

		/// <summary>
		/// List<Senses>
		/// </summary>
		/// <param name="searchWord">Das zu suchende Wort</param>
		/// <returns></returns>
		public System.Collections.Generic.List<Sense> SearchForNoun(string searchWord)
        {
			Search search = new Search(searchWord.ToLower(), true, PartOfSpeech.of("noun"), new SearchType(true, "ISPARTPTR"), 0);

			System.Collections.Generic.List<Sense> result = new System.Collections.Generic.List<Sense>();
			foreach (SynSet sense in search.senses)
			{
                List<string> synonyms = new List<string>();
				foreach (Lexeme synonym in sense.words) synonyms.Add(synonym.word);
				List<string> holonyms = new List<string>(); //All holonyms are in one list, no division by parts
				foreach (SynSet sensePart in sense.senses)
				{
					foreach (Lexeme holonym in sensePart.words) holonyms.Add(holonym.word);
				}
				result.Add(new Sense(synonyms.ToArray(), holonyms.ToArray()));
			}
			return result;
        }

	}

	public class Sense
    {
		public System.Collections.Generic.List<string> Synonyms;
		public System.Collections.Generic.List<string> Holonyms;

        public Sense(string[] synonyms, string[] holonyms)
        {
            Synonyms = synonyms.ToList() ?? throw new ArgumentNullException(nameof(synonyms));
            Holonyms = holonyms.ToList() ?? throw new ArgumentNullException(nameof(holonyms));
        }

        public override bool Equals(object obj)
        {
            return obj is Sense sense &&
				   System.Collections.Generic.EqualityComparer<System.Collections.Generic.List<string>>.Default.Equals(Synonyms, sense.Synonyms) &&
				   System.Collections.Generic.EqualityComparer<System.Collections.Generic.List<string>>.Default.Equals(Holonyms, sense.Holonyms);
        }

        public override int GetHashCode()
        {
            int hashCode = -451808101;
            hashCode = hashCode * -1521134295 + System.Collections.Generic.EqualityComparer<System.Collections.Generic.List<string>>.Default.GetHashCode(Synonyms);
            hashCode = hashCode * -1521134295 + System.Collections.Generic.EqualityComparer<System.Collections.Generic.List<string>>.Default.GetHashCode(Holonyms);
            return hashCode;
        }
    }
}
