﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Munq.Search.Documents.FieldSources
{
    public class StringSource: IFieldSource
	{
		List<string> _source = new List<string>();

		public StringSource(string value)
		{
			_source.Add(value);
		}

		public StringSource(IEnumerable<string> values)
		{
			_source.AddRange(values);
		}

		public IEnumerable<Token> GetTokenIterator()
		{
			foreach (var str in _source)
				yield return new Token { Value = str };
		}
	}
}
