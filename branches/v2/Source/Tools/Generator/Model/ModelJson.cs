﻿// Copyright (c) 2007-2011 SlimDX Group
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Json;

namespace SlimDX.Generator
{
	static class ModelJson
	{
		public static ApiModel Parse(JsonObject root, IEnumerable<string> searchPaths)
		{
			var result = new ApiModel();
			foreach (var type in ParseTypes(root, searchPaths))
			{
				if (type.Value is EnumerationModel)
					result.AddEnumeration((EnumerationModel)type.Value);
				if (type.Value is StructureModel)
					result.AddStructure((StructureModel)type.Value);
				else if (type.Value is InterfaceModel)
					result.AddInterface((InterfaceModel)type.Value);
			}

			return result;
		}

		static Dictionary<string, TypeModel> ParseTypes(JsonObject root, IEnumerable<string> searchPaths)
		{
			var types = new Dictionary<string, TypeModel>();

			JsonObject items;
			if (root.TryGetValue("dependencies", out items))
			{
				foreach (var item in items)
				{
					var dependencyFile = (string)item;
					foreach (var searchPath in searchPaths)
					{
						var dependencyPath = Path.Combine(searchPath, dependencyFile);
						if (File.Exists(dependencyPath))
						{
							var dependency = JsonObject.FromJson(File.ReadAllText(dependencyPath));
							foreach (var type in ParseTypes(dependency, searchPaths))
								types[type.Key] = type.Value;
							break;
						}
					}
				}
			}

			if (root.TryGetValue("references", out items))
			{
				foreach (var item in items)
				{
					var key = (string)item["key"];
					var type = Type.GetType((string)item["target"]);
					var name = item.ContainsKey("name") ? (string)item["name"] : type.FullName;
					var model = new TypeModel(key, name, type);
					types[model.Key] = model;
				}
			}

			if (root.TryGetValue("enumerations", out items))
			{
				foreach (var item in items)
				{
					var name = (string)item["key"];
					var model = new EnumerationModel(name);
					types[model.Key] = model;

					foreach (var value in ParseValues(item))
						model.AddValue(value);
				}
			}

			// do an initial parse of the structures and interfaces so that we can get them
			// all into the type map before we start doing members and methods
			if (root.TryGetValue("structures", out items))
			{
				foreach (var item in items)
				{
					var name = (string)item["key"];
					var model = new StructureModel(name);
					types[model.Key] = model;
				}
			}

			if (root.TryGetValue("interfaces", out items))
			{
				foreach (var item in items)
				{
					var name = (string)item["key"];
					var guid = new Guid((string)item["guid"]);
					var model = new InterfaceModel(name, guid);
					types[model.Key] = model;
				}
			}

			// now add the members and methods for the structures and interfaces
			if (root.TryGetValue("structures", out items))
			{
				foreach (var item in items)
				{
					var name = (string)item["key"];
					var model = (StructureModel)types[name];

					foreach (var value in ParseFields(item, types))
						model.AddMember(value);
				}
			}

			if (root.TryGetValue("interfaces", out items))
			{
				foreach (var item in items)
				{
					var name = (string)item["key"];
					var model = (InterfaceModel)types[name];

					model.Parent = types[(string)item["type"]].Key == "IUnknown" ? types["SlimDX.ComObject"] : types[(string)item["type"]];
					foreach (var method in ParseMethods(item, types))
						model.AddMethod(method);
				}
			}
			return types;
		}

		static int FixupMethodOffset(InterfaceModel model, Dictionary<string, TypeModel> types)
		{
			if (model.MethodOffset != 0)
				return model.MethodOffset;

			if (model.Parent == types["SlimDX.ComObject"])
			{
				model.MethodOffset = 3;
				return 3;
			}

			var parent = model.Parent as InterfaceModel;
			model.MethodOffset = parent.Methods.Count + FixupMethodOffset(parent, types);

			return model.MethodOffset;
		}

		static IEnumerable<MethodModel> ParseMethods(JsonObject root, Dictionary<string, TypeModel> types)
		{
			var results = new List<MethodModel>();

			JsonObject methods;
			if (root.TryGetValue("methods", out methods))
			{
				foreach (var method in methods)
				{
					var name = (string)method["key"];
					var type = types[(string)method["type"]];
					var index = (int)method["index"];
					var model = new MethodModel(name, type, index);
					foreach (var parameter in ParseParameters(method, types))
						model.AddParameter(parameter);

					results.Add(model);
				}
			}

			return results;
		}

		static IEnumerable<ParameterModel> ParseParameters(JsonObject root, Dictionary<string, TypeModel> types)
		{
			var results = new List<ParameterModel>();

			JsonObject parameters;
			if (root.TryGetValue("parameters", out parameters))
			{
				foreach (var parameter in parameters)
				{
					var name = (string)parameter["key"];
					var type = types[(string)parameter["type"]];
					var flags = ParseParameterFlags(parameter);
					var model = new ParameterModel(name, type, flags);

					results.Add(model);
				}
			}

			return results;
		}

		static ParameterModelFlags ParseParameterFlags(JsonObject root)
		{
			var results = ParameterModelFlags.None;

			JsonObject flags;
			if (root.TryGetValue("flags", out flags))
			{
				foreach (var flag in flags)
				{
					switch ((string)flag)
					{
						case "out":
							results |= ParameterModelFlags.IsOutput;
							break;
					}
				}
			}

			return results;
		}

		static IEnumerable<StructureMemberModel> ParseFields(JsonObject root, Dictionary<string, TypeModel> types)
		{
			var results = new List<StructureMemberModel>();

			JsonObject parameters;
			if (root.TryGetValue("members", out parameters))
			{
				foreach (var parameter in parameters)
				{
					var name = (string)parameter["key"];
					var type = types[(string)parameter["type"]];
					var model = new StructureMemberModel(name, type);

					results.Add(model);
				}
			}

			return results;
		}

		static IEnumerable<EnumerationValueModel> ParseValues(JsonObject root)
		{
			var results = new List<EnumerationValueModel>();

			JsonObject values;
			if (root.TryGetValue("values", out values))
			{
				foreach (var value in values)
					results.Add(new EnumerationValueModel((string)value["key"], (string)value["value"]));
			}

			return results;
		}
	}
}