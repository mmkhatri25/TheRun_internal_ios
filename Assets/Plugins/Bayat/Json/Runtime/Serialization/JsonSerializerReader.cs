﻿#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
#if !(NET35 || NET20 || PORTABLE40)
using System.Dynamic;
#endif
using System.Diagnostics;
using System.Globalization;
#if !((UNITY_WSA || UNITY_WINRT) || PORTABLE40 || NET35 || NET20)
using System.Numerics;
#endif
using System.Runtime.Serialization;

using Bayat.Json.Linq;
using Bayat.Json.Shims;
using Bayat.Json.Utilities;

#if NET20
using Bayat.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

using Bayat.Core;
using Bayat.Core.Reflection;

namespace Bayat.Json.Serialization
{

    [Preserve]
    public class JsonSerializerReader : JsonSerializerInternalBase
    {

        public enum PropertyPresence
        {
            None = 0,
            Null = 1,
            Value = 2
        }

        public JsonSerializerReader(JsonSerializer serializer)
            : base(serializer)
        {
        }

        public void Populate(JsonReader reader, object target)
        {
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            Type objectType = target.GetType();

            JsonContract contract = this.Serializer.ContractResolver.ResolveContract(objectType);

            if (!reader.MoveToContent())
            {
                throw JsonSerializationException.Create(reader, "No JSON content found.");
            }

            if (reader.TokenType == JsonToken.StartArray)
            {
                if (contract.ContractType == JsonContractType.Array)
                {
                    JsonArrayContract arrayContract = (JsonArrayContract)contract;

                    PopulateList((arrayContract.ShouldCreateWrapper) ? arrayContract.CreateWrapper(target) : (IList)target, reader, arrayContract, null, null);
                }
                else
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate JSON array onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                reader.ReadAndAssert();

                string id = null;
                if (this.Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore
                    && reader.TokenType == JsonToken.PropertyName
                    && string.Equals(reader.Value.ToString(), JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
                {
                    reader.ReadAndAssert();
                    id = (reader.Value != null) ? reader.Value.ToString() : null;
                    reader.ReadAndAssert();
                }

                if (contract.ContractType == JsonContractType.Dictionary)
                {
                    JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)contract;
                    PopulateDictionary((dictionaryContract.ShouldCreateWrapper) ? dictionaryContract.CreateWrapper(target) : (IDictionary)target, reader, dictionaryContract, null, id);
                }
                else if (contract.ContractType == JsonContractType.Object)
                {
                    PopulateObject(target, reader, (JsonObjectContract)contract, null, id);
                }
                else
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate JSON object onto type '{0}'.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Unexpected initial token '{0}' when populating object. Expected JSON object or array.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        internal JsonContract GetContractSafe(Type type)
        {
            if (type == null)
            {
                return null;
            }

            return this.Serializer.ContractResolver.ResolveContract(type);
        }

        public T DeserializeProperty<T>(JsonReader reader)
        {
            return (T)DeserializeProperty(reader, typeof(T));
        }

        public object DeserializeProperty(JsonReader reader, Type objectType)
        {
            if (reader.TokenType != JsonToken.PropertyName)
            {
                reader.ReadAndMoveToContent();
            }
            reader.ReadAndMoveToContent();
            object result = Deserialize(reader, objectType, false);
            return result;
        }

        public T Deserialize<T>(JsonReader reader)
        {
            return (T)Deserialize(reader, typeof(T), false);
        }

        public object Deserialize(JsonReader reader, Type objectType, bool checkAdditionalContent)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            JsonContract contract = GetContractSafe(objectType);
            try
            {
                JsonConverter converter = GetConverter(contract, null, null, null);

                if (reader.TokenType == JsonToken.None && !ReadForType(reader, contract, converter != null))
                {
                    if (contract != null && !contract.IsNullable)
                    {
                        throw JsonSerializationException.Create(reader, "No JSON content found and type '{0}' is not nullable.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    return null;
                }

                object deserializedValue;

                if (converter != null && converter.CanRead)
                {
                    deserializedValue = DeserializeConvertable(converter, reader, objectType, null);
                }
                else
                {
                    deserializedValue = CreateValueInternal(reader, objectType, contract, null, null, null, null);
                }

                if (checkAdditionalContent)
                {
                    if (reader.Read() && reader.TokenType != JsonToken.Comment)
                    {
                        throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
                    }
                }

                return deserializedValue;
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(null, contract, null, reader as IJsonLineInfo, reader.Path, ex))
                {
                    HandleError(reader, false, 0);
                    return null;
                }
                else
                {
                    // clear context in case serializer is being used inside a converter
                    // if the converter wraps the error then not clearing the context will cause this error:
                    // "Current error context error is different to requested error."
                    ClearErrorContext();
                    throw;
                }
            }
        }


        public object DeserializeIntoProperty(JsonReader reader, object existingValue)
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {

                // Skip property name
                reader.Read();
                //throw new InvalidOperationException();
            }
            return DeserializeInto(reader, existingValue);
        }

        public object DeserializeInto(JsonReader reader, object existingValue)
        {
            return DeserializeInto(reader, existingValue.GetType(), existingValue, false);
        }

        public object DeserializeInto(JsonReader reader, Type objectType, object existingValue, bool checkAdditionalContent)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            JsonContract contract = GetContractSafe(objectType);

            try
            {
                JsonConverter converter = GetConverter(contract, null, null, null);

                if (reader.TokenType == JsonToken.None && !ReadForType(reader, contract, converter != null))
                {
                    if (contract != null && !contract.IsNullable)
                    {
                        throw JsonSerializationException.Create(reader, "No JSON content found and type '{0}' is not nullable.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    return null;
                }

                object deserializedValue;

                if (converter != null && converter.CanRead)
                {
                    deserializedValue = DeserializeConvertable(converter, reader, objectType, existingValue);
                }
                else
                {
                    deserializedValue = CreateValueInternal(reader, objectType, contract, null, null, null, existingValue);
                }

                if (checkAdditionalContent)
                {
                    if (reader.Read() && reader.TokenType != JsonToken.Comment)
                    {
                        throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
                    }
                }

                return deserializedValue;
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(null, contract, null, reader as IJsonLineInfo, reader.Path, ex))
                {
                    HandleError(reader, false, 0);
                    return null;
                }
                else
                {
                    // clear context in case serializer is being used inside a converter
                    // if the converter wraps the error then not clearing the context will cause this error:
                    // "Current error context error is different to requested error."
                    ClearErrorContext();
                    throw;
                }
            }
        }

        private JsonSerializerProxy GetInternalSerializer()
        {
            if (this.InternalSerializer == null)
            {
                this.InternalSerializer = new JsonSerializerProxy(this);
            }

            return this.InternalSerializer;
        }

        private JToken CreateJToken(JsonReader reader, JsonContract contract)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            if (contract != null)
            {
                if (contract.UnderlyingType == typeof(JRaw))
                {
                    return JRaw.Create(reader);
                }
                if (reader.TokenType == JsonToken.Null
                    && !(contract.UnderlyingType == typeof(JValue) || contract.UnderlyingType == typeof(JToken)))
                {
                    return null;
                }
            }

            JToken token;
            using (JTokenWriter writer = new JTokenWriter())
            {
                writer.WriteToken(reader);
                token = writer.Token;
            }

            return token;
        }

        internal JToken CreateJObject(JsonReader reader)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            // this is needed because we've already read inside the object, looking for metadata properties
            using (JTokenWriter writer = new JTokenWriter())
            {
                writer.WriteStartObject();

                do
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        string propertyName = (string)reader.Value;
                        if (!reader.ReadAndMoveToContent())
                        {
                            break;
                        }

                        if (CheckPropertyName(reader, propertyName))
                        {
                            continue;
                        }

                        writer.WritePropertyName(propertyName);
                        writer.WriteToken(reader, true, true, false);
                    }
                    else if (reader.TokenType == JsonToken.Comment)
                    {
                        // eat
                    }
                    else
                    {
                        writer.WriteEndObject();
                        return writer.Token;
                    }
                } while (reader.Read());

                throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
            }
        }

        private object CreateValueInternal(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue)
        {
            if (contract != null && contract.ContractType == JsonContractType.Linq)
            {
                return CreateJToken(reader, contract);
            }

            do
            {
                switch (reader.TokenType)
                {
                    // populate a typed object or generic dictionary/array
                    // depending upon whether an objectType was supplied
                    case JsonToken.StartObject:
                        return CreateObject(reader, objectType, contract, member, containerContract, containerMember, existingValue);
                    case JsonToken.StartArray:
                        return CreateList(reader, objectType, contract, member, existingValue, null);
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Boolean:
                    case JsonToken.Date:
                    case JsonToken.Bytes:
                        return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.String:
                        string s = (string)reader.Value;

                        // convert empty string to null automatically for nullable types
                        if (CoerceEmptyStringToNull(objectType, contract, s))
                        {
                            return null;
                        }

                        // string that needs to be returned as a byte array should be base 64 decoded
                        if (objectType == typeof(byte[]))
                        {
                            return Convert.FromBase64String(s);
                        }

                        return EnsureType(reader, s, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.StartConstructor:
                        string constructorName = reader.Value.ToString();

                        return EnsureType(reader, constructorName, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.Null:
                    case JsonToken.Undefined:
#if !(DOTNET || PORTABLE40 || (UNITY_WSA || UNITY_WINRT))
                        if (objectType == typeof(DBNull))
                        {
                            return DBNull.Value;
                        }
#endif

                        return EnsureType(reader, reader.Value, CultureInfo.InvariantCulture, contract, objectType);
                    case JsonToken.Raw:
                        return new JRaw((string)reader.Value);
                    case JsonToken.Comment:
                        // ignore
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token while deserializing object: " + reader.TokenType);
                }
            } while (reader.Read());

            throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
        }

        private static bool CoerceEmptyStringToNull(Type objectType, JsonContract contract, string s)
        {
            return string.IsNullOrEmpty(s) && objectType != null && objectType != typeof(string) && objectType != typeof(object) && contract != null && contract.IsNullable;
        }

        public string GetExpectedDescription(JsonContract contract)
        {
            switch (contract.ContractType)
            {
                case JsonContractType.Object:
                case JsonContractType.Dictionary:
#if !(DOTNET || (UNITY_WSA || UNITY_WINRT) || PORTABLE40)
                case JsonContractType.Serializable:
#endif
#if !(NET35 || NET20 || PORTABLE40)
                case JsonContractType.Dynamic:
#endif
                    return @"JSON object (e.g. {""name"":""value""})";
                case JsonContractType.Array:
                    return @"JSON array (e.g. [1,2,3])";
                case JsonContractType.Primitive:
                    return @"JSON primitive value (e.g. string, number, boolean, null)";
                case JsonContractType.String:
                    return @"JSON string value";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public JsonConverter GetConverter(JsonContract contract, JsonConverter memberConverter, JsonContainerContract containerContract, JsonProperty containerProperty)
        {
            JsonConverter converter = null;
            if (memberConverter != null)
            {
                // member attribute converter
                converter = memberConverter;
            }
            else if (containerProperty != null && containerProperty.ItemConverter != null)
            {
                converter = containerProperty.ItemConverter;
            }
            else if (containerContract != null && containerContract.ItemConverter != null)
            {
                converter = containerContract.ItemConverter;
            }
            else if (contract != null)
            {
                JsonConverter matchingConverter;
                if (contract.Converter != null)
                {
                    // class attribute converter
                    converter = contract.Converter;
                }
                else if ((matchingConverter = this.Serializer.GetMatchingConverter(contract.UnderlyingType)) != null)
                {
                    // passed in converters
                    converter = matchingConverter;
                }
                else if (contract.InternalConverter != null)
                {
                    // internally specified converter
                    converter = contract.InternalConverter;
                }
            }
            return converter;
        }

        private object CreateObject(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue)
        {
            string id;
            string unityGuid;
            Type resolvedObjectType = objectType;
            UnityEngine.Object unityObject = null;

            if (this.Serializer.MetadataPropertyHandling == MetadataPropertyHandling.Ignore)
            {
                // don't look for metadata properties
                reader.ReadAndAssert();
                id = null;
                unityGuid = null;
            }
            else if (this.Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
            {
                JTokenReader tokenReader = reader as JTokenReader;
                if (tokenReader == null)
                {
                    JToken t = JToken.ReadFrom(reader);
                    tokenReader = (JTokenReader)t.CreateReader();
                    tokenReader.Culture = reader.Culture;
                    tokenReader.DateFormatString = reader.DateFormatString;
                    tokenReader.DateParseHandling = reader.DateParseHandling;
                    tokenReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                    tokenReader.FloatParseHandling = reader.FloatParseHandling;
                    tokenReader.SupportMultipleContent = reader.SupportMultipleContent;

                    // start
                    tokenReader.ReadAndAssert();

                    reader = tokenReader;
                }

                object newValue;
                if (ReadMetadataPropertiesToken(tokenReader, ref resolvedObjectType, ref contract, member, containerContract, containerMember, existingValue, out newValue, out id, out unityGuid, out unityObject))
                {
                    if (SceneReferenceResolver.Current != null && !string.IsNullOrEmpty(unityGuid) && !AssetReferenceResolver.Current.Contains(unityGuid))
                    {
                        SceneReferenceResolver.Current.Add(unityObject, unityGuid);
                        //if (SceneReferenceResolver.Current.Contains(unityGuid))
                        //{
                        //    SceneReferenceResolver.Current.Set(unityGuid, unityObject);
                        //}
                        //else
                        //{
                        //    SceneReferenceResolver.Current.Add(unityGuid, unityObject);
                        //}
                    }
                    if (id != null)
                    {
                        if (newValue != null)
                        {
                            AddReference(reader, id, newValue);
                        }
                        else
                        {
                            AddReference(reader, id, unityObject);
                        }
                    }
                    if (unityObject != null)
                    {
                        return unityObject;
                    }
                    return newValue;
                }
            }
            else
            {
                reader.ReadAndAssert();
                object newValue;
                if (ReadMetadataProperties(reader, ref resolvedObjectType, ref contract, member, containerContract, containerMember, existingValue, out newValue, out id, out unityGuid, out unityObject))
                {
                    if (SceneReferenceResolver.Current != null && !string.IsNullOrEmpty(unityGuid) && !AssetReferenceResolver.Current.Contains(unityGuid))
                    {
                        SceneReferenceResolver.Current.Add(unityObject, unityGuid);
                        //if (SceneReferenceResolver.Current.Contains(unityGuid))
                        //{
                        //    SceneReferenceResolver.Current.Set(unityGuid, unityObject);
                        //}
                        //else
                        //{
                        //    SceneReferenceResolver.Current.Add(unityGuid, unityObject);
                        //}
                    }
                    if (id != null)
                    {
                        if (newValue != null)
                        {
                            AddReference(reader, id, newValue);
                        }
                        else
                        {
                            AddReference(reader, id, unityObject);
                        }
                    }
                    if (unityObject != null)
                    {
                        return unityObject;
                    }
                    return newValue;
                }
            }

            if (HasNoDefinedType(contract))
            {
                return CreateJObject(reader);
            }

            switch (contract.ContractType)
            {
                case JsonContractType.Object:
                    {
                        bool createdFromNonDefaultCreator = false;
                        JsonObjectContract objectContract = (JsonObjectContract)contract;
                        object targetObject;

                        JsonContract newContract = GetContractSafe(resolvedObjectType);
                        var converter = GetConverter(newContract, null, null, null);
                        var objectConverter = converter as Bayat.Json.Converters.ObjectJsonConverter;

                        // check that if type name handling is being used that the existing value is compatible with the specified type
                        if (existingValue != null && (resolvedObjectType == objectType || resolvedObjectType.IsAssignableFrom(existingValue.GetType())))
                        {
                            targetObject = existingValue;
                        }
                        else if (unityObject != null)
                        {
                            targetObject = unityObject;
                        }
                        else
                        {
                            if (objectConverter != null)
                            {
                                targetObject = objectConverter.Create(reader, this, objectContract, id, unityGuid, resolvedObjectType, out createdFromNonDefaultCreator);
                            }
                            else
                            {
                                targetObject = CreateNewObject(reader, objectContract, member, containerMember, id, unityGuid, out createdFromNonDefaultCreator);
                            }
                        }

                        if (SceneReferenceResolver.Current != null && !string.IsNullOrEmpty(unityGuid) && !AssetReferenceResolver.Current.Contains(unityGuid))
                        {
                            SceneReferenceResolver.Current.Add((UnityEngine.Object)targetObject, unityGuid);
                            //if (SceneReferenceResolver.Current.Contains(unityGuid))
                            //{
                            //    SceneReferenceResolver.Current.Set(unityGuid, (UnityEngine.Object)targetObject);
                            //}
                            //else
                            //{
                            //    SceneReferenceResolver.Current.Add(unityGuid, (UnityEngine.Object)targetObject);
                            //}
                        }

                        // don't populate if read from non-default creator because the object has already been read
                        if (createdFromNonDefaultCreator)
                        {
                            return targetObject;
                        }

                        if (objectConverter != null)
                        {
                            return objectConverter.Populate(objectContract, reader, resolvedObjectType, targetObject, this);
                        }

                        return PopulateObject(targetObject, reader, objectContract, member, id);
                    }
                case JsonContractType.Primitive:
                    {
                        JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract)contract;
                        // if the content is inside $value then read past it
                        if (this.Serializer.MetadataPropertyHandling != MetadataPropertyHandling.Ignore
                            && reader.TokenType == JsonToken.PropertyName
                            && string.Equals(reader.Value.ToString(), JsonTypeReflector.ValuePropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();

                            // the token should not be an object because the $type value could have been included in the object
                            // without needing the $value property
                            if (reader.TokenType == JsonToken.StartObject)
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected token when deserializing primitive value: " + reader.TokenType);
                            }

                            object value = CreateValueInternal(reader, resolvedObjectType, primitiveContract, member, null, null, existingValue);

                            reader.ReadAndAssert();
                            return value;
                        }
                        break;
                    }
                case JsonContractType.Dictionary:
                    {
                        JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)contract;
                        object targetDictionary;

                        if (existingValue == null)
                        {
                            bool createdFromNonDefaultCreator;
                            IDictionary dictionary = CreateNewDictionary(reader, dictionaryContract, out createdFromNonDefaultCreator);

                            if (createdFromNonDefaultCreator)
                            {
                                if (id != null)
                                {
                                    throw JsonSerializationException.Create(reader, "Cannot preserve reference to readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                                }

                                if (contract.OnSerializingCallbacks.Count > 0)
                                {
                                    throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on readonly dictionary, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                                }

                                if (contract.OnErrorCallbacks.Count > 0)
                                {
                                    throw JsonSerializationException.Create(reader, "Cannot call OnError on readonly list, or dictionary created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                                }

                                if (!dictionaryContract.HasParameterizedCreatorInternal)
                                {
                                    throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size dictionary: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                                }
                            }

                            PopulateDictionary(dictionary, reader, dictionaryContract, member, id);

                            if (createdFromNonDefaultCreator)
                            {
                                ObjectConstructor<object> creator = dictionaryContract.OverrideCreator ?? dictionaryContract.ParameterizedCreator;

                                return creator(dictionary);
                            }
                            else if (dictionary is IWrappedDictionary)
                            {
                                return ((IWrappedDictionary)dictionary).UnderlyingDictionary;
                            }

                            targetDictionary = dictionary;
                        }
                        else
                        {
                            targetDictionary = PopulateDictionary(dictionaryContract.ShouldCreateWrapper ? dictionaryContract.CreateWrapper(existingValue) : (IDictionary)existingValue, reader, dictionaryContract, member, id);
                        }

                        return targetDictionary;
                    }
#if !(NET35 || NET20 || PORTABLE40)
                case JsonContractType.Dynamic:
                    JsonDynamicContract dynamicContract = (JsonDynamicContract)contract;
                    return CreateDynamic(reader, dynamicContract, member, id);
#endif
#if !(DOTNET || PORTABLE40 || (UNITY_WSA || UNITY_WINRT))
                case JsonContractType.Serializable:
                    JsonISerializableContract serializableContract = (JsonISerializableContract)contract;
                    return CreateISerializable(reader, serializableContract, member, id);
#endif
            }

            string message = @"Cannot deserialize the current JSON object (e.g. {{""name"":""value""}}) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine +
                             @"To fix this error either change the JSON to a {1} or change the deserialized type so that it is a normal .NET type (e.g. not a primitive type like integer, not a collection type like an array or List<T>) that can be deserialized from a JSON object. JsonObjectAttribute can also be added to the type to force it to deserialize from a JSON object." + Environment.NewLine;
            message = message.FormatWith(CultureInfo.InvariantCulture, resolvedObjectType, GetExpectedDescription(contract));

            throw JsonSerializationException.Create(reader, message);
        }

        internal bool ReadMetadataPropertiesToken(JTokenReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, out object newValue, out string id, out string unityGuid, out UnityEngine.Object unityObject)
        {
            id = null;
            newValue = null;
            unityObject = null;
            unityGuid = null;

            if (reader.TokenType == JsonToken.StartObject)
            {
                JObject current = (JObject)reader.CurrentToken;

                JToken unityRefToken = current[JsonTypeReflector.UnityRefPropertyName];
                if (unityRefToken != null)
                {
                    unityGuid = (string)unityRefToken;
                    if (SceneReferenceResolver.Current != null)
                    {
                        unityObject = SceneReferenceResolver.Current.Get(unityGuid);
                        //unityObject = SceneReferenceResolver.Current.ResolveReference(unityGuid);
                    }

                    if (unityObject == null && AssetReferenceResolver.Current != null)
                    {
                        unityObject = AssetReferenceResolver.Current.Get(unityGuid);
                        if (unityObject != null && !(unityObject is UnityEngine.ScriptableObject))
                        {
                            reader.Skip();
                            newValue = unityObject;
                            return true;
                        }
                    }
                }
                JToken refToken = current[JsonTypeReflector.RefPropertyName];
                if (refToken != null)
                {
                    if (refToken.Type != JTokenType.String && refToken.Type != JTokenType.Null)
                    {
                        throw JsonSerializationException.Create(refToken, refToken.Path, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName), null);
                    }

                    JToken property = refToken.Parent;
                    JToken additionalContent = null;
                    if (property.Next != null)
                    {
                        additionalContent = property.Next;
                    }
                    else if (property.Previous != null)
                    {
                        additionalContent = property.Previous;
                    }

                    string reference = (string)refToken;

                    if (reference != null)
                    {
                        if (additionalContent != null)
                        {
                            throw JsonSerializationException.Create(additionalContent, additionalContent.Path, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName), null);
                        }

                        newValue = this.Serializer.ReferenceResolver.ResolveReference(this, reference);
                        if (newValue as UnityEngine.Object != null)
                        {
                            unityObject = newValue as UnityEngine.Object;
                        }

                        if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
                        {
                            this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, reference, newValue.GetType())), null);
                        }

                        reader.Skip();
                        return true;
                    }
                }
                JToken typeToken = current[JsonTypeReflector.TypePropertyName];
                if (typeToken != null)
                {
                    string qualifiedTypeName = (string)typeToken;
                    JsonReader typeTokenReader = typeToken.CreateReader();
                    typeTokenReader.ReadAndAssert();
                    ResolveTypeName(typeTokenReader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);

                    JToken valueToken = current[JsonTypeReflector.ValuePropertyName];
                    if (valueToken != null)
                    {
                        while (true)
                        {
                            reader.ReadAndAssert();
                            if (reader.TokenType == JsonToken.PropertyName)
                            {
                                if ((string)reader.Value == JsonTypeReflector.ValuePropertyName)
                                {
                                    return false;
                                }
                            }

                            reader.ReadAndAssert();
                            reader.Skip();
                        }
                    }
                }
                JToken idToken = current[JsonTypeReflector.IdPropertyName];
                if (idToken != null)
                {
                    id = (string)idToken;
                }
                JToken valuesToken = current[JsonTypeReflector.ArrayValuesPropertyName];
                if (valuesToken != null)
                {
                    JsonReader listReader = valuesToken.CreateReader();
                    listReader.ReadAndAssert();
                    newValue = CreateList(listReader, objectType, contract, member, existingValue, id);

                    reader.Skip();
                    return true;
                }
            }

            reader.ReadAndAssert();
            return false;
        }

        internal bool ReadMetadataProperties(JsonReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, object existingValue, out object newValue, out string id, out string unityGuid, out UnityEngine.Object unityObject)
        {
            id = null;
            newValue = null;
            unityObject = null;
            unityGuid = null;

            if (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value.ToString();

                if (propertyName.Length > 0 && propertyName[0] == '$')
                {
                    // read metadata properties
                    // $type, $id, $ref, etc
                    bool metadataProperty;

                    do
                    {
                        propertyName = reader.Value.ToString();

                        if (string.Equals(propertyName, JsonTypeReflector.RefPropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();
                            if (reader.TokenType != JsonToken.String && reader.TokenType != JsonToken.Null)
                            {
                                throw JsonSerializationException.Create(reader, "JSON reference {0} property must have a string or null value.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));
                            }

                            string reference = (reader.Value != null) ? reader.Value.ToString() : null;

                            reader.ReadAndAssert();

                            if (reference != null)
                            {
                                if (reader.TokenType == JsonToken.PropertyName)
                                {
                                    throw JsonSerializationException.Create(reader, "Additional content found in JSON reference object. A JSON reference object should only have a {0} property.".FormatWith(CultureInfo.InvariantCulture, JsonTypeReflector.RefPropertyName));
                                }

                                newValue = this.Serializer.ReferenceResolver.ResolveReference(this, reference);
                                if (newValue as UnityEngine.Object != null)
                                {
                                    unityObject = newValue as UnityEngine.Object;
                                }

                                if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
                                {
                                    this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved object reference '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, reference, newValue.GetType())), null);
                                }

                                return true;
                            }
                            else
                            {
                                metadataProperty = true;
                            }
                        }
                        else if (string.Equals(propertyName, JsonTypeReflector.TypePropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();
                            string qualifiedTypeName = reader.Value.ToString();

                            ResolveTypeName(reader, ref objectType, ref contract, member, containerContract, containerMember, qualifiedTypeName);

                            reader.ReadAndAssert();

                            metadataProperty = true;
                        }
                        else if (string.Equals(propertyName, JsonTypeReflector.IdPropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();

                            id = (reader.Value != null) ? reader.Value.ToString() : null;

                            reader.ReadAndAssert();
                            metadataProperty = true;
                        }
                        else if (string.Equals(propertyName, JsonTypeReflector.UnityRefPropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();

                            unityGuid = (reader.Value != null) ? reader.Value.ToString() : null;
                            if (SceneReferenceResolver.Current != null)
                            {
                                unityObject = SceneReferenceResolver.Current.Get(unityGuid);
                                //unityObject = SceneReferenceResolver.Current.ResolveReference(unityGuid);
                            }

                            if (unityObject == null && AssetReferenceResolver.Current != null)
                            {
                                unityObject = AssetReferenceResolver.Current.Get(unityGuid);
                                if (unityObject != null && !(unityObject is UnityEngine.ScriptableObject))
                                {
                                    reader.ReadAndAssert();
                                    newValue = unityObject;
                                    return true;
                                }
                            }

                            reader.ReadAndAssert();
                            metadataProperty = true;
                        }
                        else if (string.Equals(propertyName, JsonTypeReflector.ArrayValuesPropertyName, StringComparison.Ordinal))
                        {
                            reader.ReadAndAssert();
                            object list = CreateList(reader, objectType, contract, member, existingValue, id);
                            reader.ReadAndAssert();
                            newValue = list;
                            return true;
                        }
                        else
                        {
                            metadataProperty = false;
                        }
                    } while (metadataProperty && reader.TokenType == JsonToken.PropertyName);
                }
            }
            return false;
        }

        public void ResolveTypeName(JsonReader reader, ref Type objectType, ref JsonContract contract, JsonProperty member, JsonContainerContract containerContract, JsonProperty containerMember, string qualifiedTypeName)
        {
            TypeNameHandling resolvedTypeNameHandling =
                ((member != null) ? member.TypeNameHandling : null)
                ?? ((containerContract != null) ? containerContract.ItemTypeNameHandling : null)
                ?? ((containerMember != null) ? containerMember.ItemTypeNameHandling : null)
                ?? this.Serializer.TypeNameHandling;

            if (resolvedTypeNameHandling != TypeNameHandling.None)
            {
                string typeName;
                string assemblyName;
                ReflectionUtils.SplitFullyQualifiedTypeName(qualifiedTypeName, out typeName, out assemblyName);

                Type specifiedType;
                try
                {
                    specifiedType = this.Serializer.Binder.BindToType(assemblyName, typeName);
                }
                catch (Exception ex)
                {
                    throw JsonSerializationException.Create(reader, "Error resolving type specified in JSON '{0}'.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName), ex);
                }

                if (specifiedType == null)
                {
                    throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' was not resolved.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName));
                }

                if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                {
                    this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Resolved type '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, qualifiedTypeName, specifiedType)), null);
                }

                if (objectType != null
#if !(NET35 || NET20 || PORTABLE40)
                    && objectType != typeof(IDynamicMetaObjectProvider)
#endif
                    && !objectType.IsAssignableFrom(specifiedType))
                {
                    throw JsonSerializationException.Create(reader, "Type specified in JSON '{0}' is not compatible with '{1}'.".FormatWith(CultureInfo.InvariantCulture, specifiedType.AssemblyQualifiedName, objectType.AssemblyQualifiedName));
                }

                objectType = specifiedType;
                contract = GetContractSafe(specifiedType);
            }
        }

        private JsonArrayContract EnsureArrayContract(JsonReader reader, Type objectType, JsonContract contract)
        {
            if (contract == null)
            {
                throw JsonSerializationException.Create(reader, "Could not resolve type '{0}' to a JsonContract.".FormatWith(CultureInfo.InvariantCulture, objectType));
            }

            JsonArrayContract arrayContract = contract as JsonArrayContract;
            if (arrayContract == null)
            {
                string message = @"Cannot deserialize the current JSON array (e.g. [1,2,3]) into type '{0}' because the type requires a {1} to deserialize correctly." + Environment.NewLine +
                                 @"To fix this error either change the JSON to a {1} or change the deserialized type to an array or a type that implements a collection interface (e.g. ICollection, IList) like List<T> that can be deserialized from a JSON array. JsonArrayAttribute can also be added to the type to force it to deserialize from a JSON array." + Environment.NewLine;
                message = message.FormatWith(CultureInfo.InvariantCulture, objectType, GetExpectedDescription(contract));

                throw JsonSerializationException.Create(reader, message);
            }

            return arrayContract;
        }

        private object CreateList(JsonReader reader, Type objectType, JsonContract contract, JsonProperty member, object existingValue, string id)
        {
            object value;

            if (HasNoDefinedType(contract))
            {
                return CreateJToken(reader, contract);
            }

            JsonArrayContract arrayContract = EnsureArrayContract(reader, objectType, contract);

            if (existingValue == null)
            {
                bool createdFromNonDefaultCreator;
                IList list = CreateNewList(reader, arrayContract, out createdFromNonDefaultCreator);

                if (createdFromNonDefaultCreator)
                {
                    if (id != null)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot preserve reference to array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (contract.OnSerializingCallbacks.Count > 0)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot call OnSerializing on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (contract.OnErrorCallbacks.Count > 0)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot call OnError on an array or readonly list, or list created from a non-default constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }

                    if (!arrayContract.HasParameterizedCreatorInternal && !arrayContract.IsArray)
                    {
                        throw JsonSerializationException.Create(reader, "Cannot deserialize readonly or fixed size list: {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                    }
                }

                if (!arrayContract.IsMultidimensionalArray)
                {
                    PopulateList(list, reader, arrayContract, member, id);
                }
                else
                {
                    PopulateMultidimensionalArray(list, reader, arrayContract, member, id);
                }

                if (createdFromNonDefaultCreator)
                {
                    if (arrayContract.IsMultidimensionalArray)
                    {
                        list = CollectionUtils.ToMultidimensionalArray(list, arrayContract.CollectionItemType, contract.CreatedType.GetArrayRank());
                    }
                    else if (arrayContract.IsArray)
                    {
                        Array a = Array.CreateInstance(arrayContract.CollectionItemType, list.Count);
                        list.CopyTo(a, 0);
                        list = a;
                    }
                    else
                    {
                        ObjectConstructor<object> creator = arrayContract.OverrideCreator ?? arrayContract.ParameterizedCreator;

                        return creator(list);
                    }
                }
                else if (list is IWrappedCollection)
                {
                    return ((IWrappedCollection)list).UnderlyingCollection;
                }

                value = list;
            }
            else
            {
                if (!arrayContract.CanDeserialize)
                {
                    throw JsonSerializationException.Create(reader, "Cannot populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.CreatedType));
                }

                value = PopulateList((arrayContract.ShouldCreateWrapper) ? arrayContract.CreateWrapper(existingValue) : (IList)existingValue, reader, arrayContract, member, id);
            }

            return value;
        }

        internal bool HasNoDefinedType(JsonContract contract)
        {
            return (contract == null || contract.UnderlyingType == typeof(object) || contract.ContractType == JsonContractType.Linq
#if !(NET35 || NET20 || PORTABLE40)
                    || contract.UnderlyingType == typeof(IDynamicMetaObjectProvider)
#endif
                );
        }

        private object EnsureType(JsonReader reader, object value, CultureInfo culture, JsonContract contract, Type targetType)
        {
            if (targetType == null)
            {
                return value;
            }

            Type valueType = ReflectionUtils.GetObjectType(value);

            // type of value and type of target don't match
            // attempt to convert value's type to target's type
            if (valueType != targetType)
            {
                if (value == null && contract.IsNullable)
                {
                    return null;
                }

                try
                {
                    if (contract.IsConvertable)
                    {
                        JsonPrimitiveContract primitiveContract = (JsonPrimitiveContract)contract;

                        if (contract.IsEnum)
                        {
                            if (value is string)
                            {
                                return Enum.Parse(contract.NonNullableUnderlyingType, value.ToString(), true);
                            }
                            if (ConvertUtils.IsInteger(primitiveContract.TypeCode))
                            {
                                return Enum.ToObject(contract.NonNullableUnderlyingType, value);
                            }
                        }

#if !((UNITY_WSA || UNITY_WINRT) || PORTABLE40 || NET35 || NET20)
                        if (value is BigInteger)
                        {
                            return ConvertUtils.FromBigInteger((BigInteger)value, contract.NonNullableUnderlyingType);
                        }
#endif

                        // this won't work when converting to a custom IConvertible
                        return Convert.ChangeType(value, contract.NonNullableUnderlyingType, culture);
                    }

                    return ConvertUtils.ConvertOrCast(value, culture, contract.NonNullableUnderlyingType);
                }
                catch (Exception ex)
                {
                    throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.FormatValueForPrint(value), targetType), ex);
                }
            }

            return value;
        }

        public bool SetPropertyValue(JsonProperty property, JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target)
        {
            object currentValue;
            bool useExistingValue;
            JsonContract propertyContract;
            bool gottenCurrentValue;

            if (CalculatePropertyDetails(property, ref propertyConverter, containerContract, containerProperty, reader, target, out useExistingValue, out currentValue, out propertyContract, out gottenCurrentValue))
            {
                return false;
            }

            object value;

            if (propertyConverter != null && propertyConverter.CanRead)
            {
                if (!gottenCurrentValue && target != null && property.Readable)
                {
                    currentValue = property.ValueProvider.GetValue(target);
                }

                value = DeserializeConvertable(propertyConverter, reader, property.PropertyType, currentValue);
            }
            else
            {
                value = CreateValueInternal(reader, property.PropertyType, propertyContract, property, containerContract, containerProperty, (useExistingValue) ? currentValue : null);
            }

            // always set the value if useExistingValue is false,
            // otherwise also set it if CreateValue returns a new value compared to the currentValue
            // this could happen because of a JsonConverter against the type
            if ((!useExistingValue || value != currentValue)
                && ShouldSetPropertyValue(property, value))
            {
                property.ValueProvider.SetValue(target, value);

                if (property.SetIsSpecified != null)
                {
                    if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                    {
                        this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "IsSpecified for property '{0}' on {1} set to true.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType)), null);
                    }

                    property.SetIsSpecified(target, true);
                }

                return true;
            }

            // the value wasn't set be JSON was populated onto the existing value
            return useExistingValue;
        }

        private bool CalculatePropertyDetails(JsonProperty property, ref JsonConverter propertyConverter, JsonContainerContract containerContract, JsonProperty containerProperty, JsonReader reader, object target, out bool useExistingValue, out object currentValue, out JsonContract propertyContract, out bool gottenCurrentValue)
        {
            currentValue = null;
            useExistingValue = false;
            propertyContract = null;
            gottenCurrentValue = false;

            if (property.Ignored)
            {
                return true;
            }

            JsonToken tokenType = reader.TokenType;

            if (property.PropertyContract == null)
            {
                property.PropertyContract = GetContractSafe(property.PropertyType);
            }

            ObjectCreationHandling objectCreationHandling =
                property.ObjectCreationHandling.GetValueOrDefault(this.Serializer.ObjectCreationHandling);

            if ((objectCreationHandling != ObjectCreationHandling.Replace)
                && (tokenType == JsonToken.StartArray || tokenType == JsonToken.StartObject)
                && property.Readable)
            {
                currentValue = property.ValueProvider.GetValue(target);
                gottenCurrentValue = true;

                if (currentValue != null)
                {
                    propertyContract = GetContractSafe(currentValue.GetType());

                    useExistingValue = (!propertyContract.IsReadOnlyOrFixedSize && !propertyContract.UnderlyingType.IsValueType());
                }
            }

            if (!property.Writable && !useExistingValue)
            {
                return true;
            }

            // test tokentype here because null might not be convertable to some types, e.g. ignoring null when applied to DateTime
            if (property.NullValueHandling.GetValueOrDefault(this.Serializer.NullValueHandling) == NullValueHandling.Ignore && tokenType == JsonToken.Null)
            {
                return true;
            }

            // test tokentype here because default value might not be convertable to actual type, e.g. default of "" for DateTime
            if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
                && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer.DefaultValueHandling), DefaultValueHandling.Populate)
                && JsonTokenUtils.IsPrimitiveToken(tokenType)
                && MiscellaneousUtils.ValueEquals(reader.Value, property.GetResolvedDefaultValue()))
            {
                return true;
            }

            if (currentValue == null)
            {
                propertyContract = property.PropertyContract;
            }
            else
            {
                propertyContract = GetContractSafe(currentValue.GetType());

                if (propertyContract != property.PropertyContract)
                {
                    propertyConverter = GetConverter(propertyContract, property.MemberConverter, containerContract, containerProperty);
                }
            }

            return false;
        }

        internal void AddReference(JsonReader reader, string id, object value)
        {
            try
            {
                if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                {
                    this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Read object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, id, value.GetType())), null);
                }
                this.Serializer.ReferenceResolver.AddReference(this, id, value);
            }
            catch (Exception ex)
            {
                throw JsonSerializationException.Create(reader, "Error reading object reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, id), ex);
            }
        }

        public bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
        {
            return ((value & flag) == flag);
        }

        private bool ShouldSetPropertyValue(JsonProperty property, object value)
        {
            if (property.NullValueHandling.GetValueOrDefault(this.Serializer.NullValueHandling) == NullValueHandling.Ignore && value == null)
            {
                return false;
            }

            if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer.DefaultValueHandling), DefaultValueHandling.Ignore)
                && !HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer.DefaultValueHandling), DefaultValueHandling.Populate)
                && MiscellaneousUtils.ValueEquals(value, property.GetResolvedDefaultValue()))
            {
                return false;
            }

            if (!property.Writable)
            {
                return false;
            }

            return true;
        }

        private IList CreateNewList(JsonReader reader, JsonArrayContract contract, out bool createdFromNonDefaultCreator)
        {
            // some types like non-generic IEnumerable can be serialized but not deserialized
            if (!contract.CanDeserialize)
            {
                throw JsonSerializationException.Create(reader, "Cannot create and populate list type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.CreatedType));
            }

            if (contract.OverrideCreator != null)
            {
                if (contract.HasParameterizedCreator)
                {
                    createdFromNonDefaultCreator = true;
                    return contract.CreateTemporaryCollection();
                }
                else
                {
                    createdFromNonDefaultCreator = false;
                    return (IList)contract.OverrideCreator();
                }
            }
            else if (contract.IsReadOnlyOrFixedSize)
            {
                createdFromNonDefaultCreator = true;
                IList list = contract.CreateTemporaryCollection();

                if (contract.ShouldCreateWrapper)
                {
                    list = contract.CreateWrapper(list);
                }

                return list;
            }
            else if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || this.Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
            {
                object list = contract.DefaultCreator();

                if (contract.ShouldCreateWrapper)
                {
                    list = contract.CreateWrapper(list);
                }

                createdFromNonDefaultCreator = false;
                return (IList)list;
            }
            else if (contract.HasParameterizedCreatorInternal)
            {
                createdFromNonDefaultCreator = true;
                return contract.CreateTemporaryCollection();
            }
            else
            {
                if (!contract.IsInstantiable)
                {
                    throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                }

                throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }
        }

        private IDictionary CreateNewDictionary(JsonReader reader, JsonDictionaryContract contract, out bool createdFromNonDefaultCreator)
        {
            if (contract.OverrideCreator != null)
            {
                if (contract.HasParameterizedCreator)
                {
                    createdFromNonDefaultCreator = true;
                    return contract.CreateTemporaryDictionary();
                }
                else
                {
                    createdFromNonDefaultCreator = false;
                    return (IDictionary)contract.OverrideCreator();
                }
            }
            else if (contract.IsReadOnlyOrFixedSize)
            {
                createdFromNonDefaultCreator = true;
                return contract.CreateTemporaryDictionary();
            }
            else if (contract.DefaultCreator != null && (!contract.DefaultCreatorNonPublic || this.Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
            {
                object dictionary = contract.DefaultCreator();

                if (contract.ShouldCreateWrapper)
                {
                    dictionary = contract.CreateWrapper(dictionary);
                }

                createdFromNonDefaultCreator = false;
                return (IDictionary)dictionary;
            }
            else if (contract.HasParameterizedCreatorInternal)
            {
                createdFromNonDefaultCreator = true;
                return contract.CreateTemporaryDictionary();
            }
            else
            {
                if (!contract.IsInstantiable)
                {
                    throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
                }

                throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }
        }

        internal void OnDeserializing(JsonReader reader, JsonContract contract, object value)
        {
            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            contract.InvokeOnDeserializing(value, this.Serializer.Context);
        }

        internal void OnDeserialized(JsonReader reader, JsonContract contract, object value)
        {
            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            contract.InvokeOnDeserialized(value, this.Serializer.Context);
        }

        private object PopulateDictionary(IDictionary dictionary, JsonReader reader, JsonDictionaryContract contract, JsonProperty containerProperty, string id)
        {
            IWrappedDictionary wrappedDictionary = dictionary as IWrappedDictionary;
            object underlyingDictionary = wrappedDictionary != null ? wrappedDictionary.UnderlyingDictionary : dictionary;

            if (id != null)
            {
                AddReference(reader, id, underlyingDictionary);
            }

            OnDeserializing(reader, contract, underlyingDictionary);

            int initialDepth = reader.Depth;

            if (contract.KeyContract == null)
            {
                contract.KeyContract = GetContractSafe(contract.DictionaryKeyType);
            }

            if (contract.ItemContract == null)
            {
                contract.ItemContract = GetContractSafe(contract.DictionaryValueType);
            }

            JsonConverter dictionaryValueConverter = contract.ItemConverter ?? GetConverter(contract.ItemContract, null, contract, containerProperty);
            PrimitiveTypeCode keyTypeCode = (contract.KeyContract is JsonPrimitiveContract) ? ((JsonPrimitiveContract)contract.KeyContract).TypeCode : PrimitiveTypeCode.Empty;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        object keyValue = reader.Value;
                        if (CheckPropertyName(reader, keyValue.ToString()))
                        {
                            continue;
                        }

                        try
                        {
                            try
                            {
                                // this is for correctly reading ISO and MS formatted dictionary keys
                                switch (keyTypeCode)
                                {
                                    case PrimitiveTypeCode.DateTime:
                                    case PrimitiveTypeCode.DateTimeNullable:
                                        {
                                            DateTime dt;
                                            if (DateTimeUtils.TryParseDateTime(keyValue.ToString(), reader.DateTimeZoneHandling, reader.DateFormatString, reader.Culture, out dt))
                                            {
                                                keyValue = dt;
                                            }
                                            else
                                            {
                                                keyValue = EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                            }
                                            break;
                                        }
#if !NET20
                                    case PrimitiveTypeCode.DateTimeOffset:
                                    case PrimitiveTypeCode.DateTimeOffsetNullable:
                                        {
                                            DateTimeOffset dt;
                                            if (DateTimeUtils.TryParseDateTimeOffset(keyValue.ToString(), reader.DateFormatString, reader.Culture, out dt))
                                            {
                                                keyValue = dt;
                                            }
                                            else
                                            {
                                                keyValue = EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                            }
                                            break;
                                        }
#endif
                                    default:
                                        keyValue = EnsureType(reader, keyValue, CultureInfo.InvariantCulture, contract.KeyContract, contract.DictionaryKeyType);
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                throw JsonSerializationException.Create(reader, "Could not convert string '{0}' to dictionary key type '{1}'. Create a TypeConverter to convert from the string to the key type object.".FormatWith(CultureInfo.InvariantCulture, reader.Value, contract.DictionaryKeyType), ex);
                            }

                            if (!ReadForType(reader, contract.ItemContract, dictionaryValueConverter != null))
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when deserializing object.");
                            }

                            object itemValue;
                            if (dictionaryValueConverter != null && dictionaryValueConverter.CanRead)
                            {
                                itemValue = DeserializeConvertable(dictionaryValueConverter, reader, contract.DictionaryValueType, null);
                            }
                            else
                            {
                                itemValue = CreateValueInternal(reader, contract.DictionaryValueType, contract.ItemContract, null, contract, containerProperty, null);
                            }

                            dictionary[keyValue] = itemValue;
                        }
                        catch (Exception ex)
                        {
                            if (IsErrorHandled(underlyingDictionary, contract, keyValue, reader as IJsonLineInfo, reader.Path, ex))
                            {
                                HandleError(reader, true, initialDepth);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        finished = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && reader.Read());

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, underlyingDictionary, "Unexpected end when deserializing object.");
            }

            OnDeserialized(reader, contract, underlyingDictionary);
            return underlyingDictionary;
        }

        private object PopulateMultidimensionalArray(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id)
        {
            int rank = contract.UnderlyingType.GetArrayRank();

            if (id != null)
            {
                AddReference(reader, id, list);
            }

            OnDeserializing(reader, contract, list);

            JsonContract collectionItemContract = GetContractSafe(contract.CollectionItemType);
            JsonConverter collectionItemConverter = GetConverter(collectionItemContract, null, contract, containerProperty);

            int? previousErrorIndex = null;
            Stack<IList> listStack = new Stack<IList>();
            listStack.Push(list);
            IList currentList = list;

            bool finished = false;
            do
            {
                int initialDepth = reader.Depth;

                if (listStack.Count == rank)
                {
                    try
                    {
                        if (ReadForType(reader, collectionItemContract, collectionItemConverter != null))
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.EndArray:
                                    listStack.Pop();
                                    currentList = listStack.Peek();
                                    previousErrorIndex = null;
                                    break;
                                default:
                                    object value;

                                    if (collectionItemConverter != null && collectionItemConverter.CanRead)
                                    {
                                        value = DeserializeConvertable(collectionItemConverter, reader, contract.CollectionItemType, null);
                                    }
                                    else
                                    {
                                        value = CreateValueInternal(reader, contract.CollectionItemType, collectionItemContract, null, contract, containerProperty, null);
                                    }

                                    currentList.Add(value);
                                    break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        JsonPosition errorPosition = reader.GetPosition(initialDepth);

                        if (IsErrorHandled(list, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
                        {
                            HandleError(reader, true, initialDepth);

                            if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
                            {
                                // reader index has not moved since previous error handling
                                // break out of reading array to prevent infinite loop
                                throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
                            }
                            else
                            {
                                previousErrorIndex = errorPosition.Position;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    if (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.StartArray:
                                IList newList = new List<object>();
                                currentList.Add(newList);
                                listStack.Push(newList);
                                currentList = newList;
                                break;
                            case JsonToken.EndArray:
                                listStack.Pop();

                                if (listStack.Count > 0)
                                {
                                    currentList = listStack.Peek();
                                }
                                else
                                {
                                    finished = true;
                                }
                                break;
                            case JsonToken.Comment:
                                break;
                            default:
                                throw JsonSerializationException.Create(reader, "Unexpected token when deserializing multidimensional array: " + reader.TokenType);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            } while (!finished);

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, list, "Unexpected end when deserializing array.");
            }

            OnDeserialized(reader, contract, list);
            return list;
        }

        public void ThrowUnexpectedEndException(JsonReader reader, JsonContract contract, object currentObject, string message)
        {
            try
            {
                throw JsonSerializationException.Create(reader, message);
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(currentObject, contract, null, reader as IJsonLineInfo, reader.Path, ex))
                {
                    HandleError(reader, false, 0);
                }
                else
                {
                    throw;
                }
            }
        }

        private object PopulateList(IList list, JsonReader reader, JsonArrayContract contract, JsonProperty containerProperty, string id)
        {
            IWrappedCollection wrappedCollection = list as IWrappedCollection;
            object underlyingList = wrappedCollection != null ? wrappedCollection.UnderlyingCollection : list;

            if (id != null)
            {
                AddReference(reader, id, underlyingList);
            }

            // can't populate an existing array
            if (list.IsFixedSize)
            {
                reader.Skip();
                return underlyingList;
            }

            OnDeserializing(reader, contract, underlyingList);

            int initialDepth = reader.Depth;

            if (contract.ItemContract == null)
            {
                contract.ItemContract = GetContractSafe(contract.CollectionItemType);
            }

            JsonConverter collectionItemConverter = GetConverter(contract.ItemContract, null, contract, containerProperty);

            //if (collectionItemConverter == null)
            //{
            //    var serializableContract = contract.ItemContract as JsonISerializableContract;
            //    if (serializableContract != null && serializableContract.ISerializableCreator == null)
            //    {
            //        reader.Skip();
            //        return underlyingList;
            //    }
            //}

            int? previousErrorIndex = null;

            bool finished = false;
            do
            {
                try
                {
                    if (ReadForType(reader, contract.ItemContract, collectionItemConverter != null))
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.EndArray:
                                finished = true;
                                break;
                            default:
                                object value;

                                if (collectionItemConverter != null && collectionItemConverter.CanRead)
                                {
                                    value = DeserializeConvertable(collectionItemConverter, reader, contract.CollectionItemType, null);
                                }
                                else
                                {
                                    value = CreateValueInternal(reader, contract.CollectionItemType, contract.ItemContract, null, contract, containerProperty, null);
                                }

                                list.Add(value);
                                break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    JsonPosition errorPosition = reader.GetPosition(initialDepth);

                    if (IsErrorHandled(underlyingList, contract, errorPosition.Position, reader as IJsonLineInfo, reader.Path, ex))
                    {
                        HandleError(reader, true, initialDepth);

                        if (previousErrorIndex != null && previousErrorIndex == errorPosition.Position)
                        {
                            // reader index has not moved since previous error handling
                            // break out of reading array to prevent infinite loop
                            throw JsonSerializationException.Create(reader, "Infinite loop detected from error handling.", ex);
                        }
                        else
                        {
                            previousErrorIndex = errorPosition.Position;
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            } while (!finished);

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, underlyingList, "Unexpected end when deserializing array.");
            }

            OnDeserialized(reader, contract, underlyingList);
            return underlyingList;
        }

#if !(DOTNET || PORTABLE40 || (UNITY_WSA || UNITY_WINRT))
        private object CreateISerializable(JsonReader reader, JsonISerializableContract contract, JsonProperty member, string id)
        {
            Type objectType = contract.UnderlyingType;

            if (!JsonTypeReflector.FullyTrusted)
            {
                string message = @"Type '{0}' implements ISerializable but cannot be deserialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine +
                                 @"To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
                message = message.FormatWith(CultureInfo.InvariantCulture, objectType);

                throw JsonSerializationException.Create(reader, message);
            }

            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using ISerializable constructor.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
            }

            SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new JsonFormatterConverter(this, contract, member));

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();
                        if (!reader.Read())
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                        }
                        serializationInfo.AddValue(memberName, JToken.ReadFrom(reader));
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        finished = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && reader.Read());

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, serializationInfo, "Unexpected end when deserializing object.");
            }

            if (contract.ISerializableCreator == null)
            {
                throw JsonSerializationException.Create(reader, "ISerializable type '{0}' does not have a valid constructor. To correctly implement ISerializable a constructor that takes SerializationInfo and StreamingContext parameters should be present.".FormatWith(CultureInfo.InvariantCulture, objectType));
            }

            object createdObject = contract.ISerializableCreator(serializationInfo, this.Serializer.Context);

            if (id != null)
            {
                AddReference(reader, id, createdObject);
            }

            // these are together because OnDeserializing takes an object but for an ISerializable the object is fully created in the constructor
            OnDeserializing(reader, contract, createdObject);
            OnDeserialized(reader, contract, createdObject);

            return createdObject;
        }

        public object CreateISerializableItem(JToken token, Type type, JsonISerializableContract contract, JsonProperty member)
        {
            JsonContract itemContract = GetContractSafe(type);
            JsonConverter itemConverter = GetConverter(itemContract, null, contract, member);

            JsonReader tokenReader = token.CreateReader();
            tokenReader.ReadAndAssert(); // Move to first token

            object result;
            if (itemConverter != null && itemConverter.CanRead)
            {
                result = DeserializeConvertable(itemConverter, tokenReader, type, null);
            }
            else
            {
                result = CreateValueInternal(tokenReader, type, itemContract, null, contract, member, null);
            }

            return result;
        }
#endif

#if !(NET35 || NET20 || PORTABLE40)
        private object CreateDynamic(JsonReader reader, JsonDynamicContract contract, JsonProperty member, string id)
        {
            IDynamicMetaObjectProvider newObject;

            if (!contract.IsInstantiable)
            {
                throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }

            if (contract.DefaultCreator != null &&
                (!contract.DefaultCreatorNonPublic || this.Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor))
            {
                newObject = (IDynamicMetaObjectProvider)contract.DefaultCreator();
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Unable to find a default constructor to use for type {0}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType));
            }

            if (id != null)
            {
                AddReference(reader, id, newObject);
            }

            OnDeserializing(reader, contract, newObject);

            int initialDepth = reader.Depth;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();

                        try
                        {
                            if (!reader.Read())
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            // first attempt to find a settable property, otherwise fall back to a dynamic set without type
                            JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

                            if (property != null && property.Writable && !property.Ignored)
                            {
                                if (property.PropertyContract == null)
                                {
                                    property.PropertyContract = GetContractSafe(property.PropertyType);
                                }

                                JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, null, null);

                                if (!SetPropertyValue(property, propertyConverter, null, member, reader, newObject))
                                {
                                    reader.Skip();
                                }
                            }
                            else
                            {
                                Type t = (JsonTokenUtils.IsPrimitiveToken(reader.TokenType)) ? reader.ValueType : typeof(IDynamicMetaObjectProvider);

                                JsonContract dynamicMemberContract = GetContractSafe(t);
                                JsonConverter dynamicMemberConverter = GetConverter(dynamicMemberContract, null, null, member);

                                object value;
                                if (dynamicMemberConverter != null && dynamicMemberConverter.CanRead)
                                {
                                    value = DeserializeConvertable(dynamicMemberConverter, reader, t, null);
                                }
                                else
                                {
                                    value = CreateValueInternal(reader, t, dynamicMemberContract, null, null, member, null);
                                }

                                contract.TrySetMember(newObject, memberName, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                            {
                                HandleError(reader, true, initialDepth);
                            }
                            else
                            {
                                throw;
                            }
                        }
                        break;
                    case JsonToken.EndObject:
                        finished = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && reader.Read());

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
            }

            OnDeserialized(reader, contract, newObject);

            return newObject;
        }
#endif

        public class CreatorPropertyContext
        {
            public string Name;
            public JsonProperty Property;
            public JsonProperty ConstructorProperty;
            public PropertyPresence? Presence;
            public object Value;
            public bool Used;
        }

        private object CreateObjectUsingCreatorWithParameters(JsonReader reader, JsonObjectContract contract, JsonProperty containerProperty, ObjectConstructor<object> creator, string id)
        {
            ValidationUtils.ArgumentNotNull(creator, nameof(creator));

            // only need to keep a track of properies presence if they are required or a value should be defaulted if missing
            bool trackPresence = (contract.HasRequiredOrDefaultValueProperties || HasFlag(this.Serializer.DefaultValueHandling, DefaultValueHandling.Populate));

            Type objectType = contract.UnderlyingType;

            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                string parameters = string.Join(", ", contract.CreatorParameters.Select(p => p.PropertyName).ToArray());
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Deserializing {0} using creator with parameters: {1}.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType, parameters)), null);
            }

            List<CreatorPropertyContext> propertyContexts = ResolvePropertyAndCreatorValues(contract, containerProperty, reader, objectType);
            if (trackPresence)
            {
                foreach (JsonProperty property in contract.Properties)
                {
                    if (propertyContexts.All(p => p.Property != property))
                    {
                        propertyContexts.Add(new CreatorPropertyContext
                        {
                            Property = property,
                            Name = property.PropertyName,
                            Presence = PropertyPresence.None
                        });
                    }
                }
            }

            object[] creatorParameterValues = new object[contract.CreatorParameters.Count];

            foreach (CreatorPropertyContext context in propertyContexts)
            {
                // set presence of read values
                if (trackPresence)
                {
                    if (context.Property != null && context.Presence == null)
                    {
                        object v = context.Value;
                        PropertyPresence propertyPresence;
                        if (v == null)
                        {
                            propertyPresence = PropertyPresence.Null;
                        }
                        else if (v is string)
                        {
                            propertyPresence = CoerceEmptyStringToNull(context.Property.PropertyType, context.Property.PropertyContract, (string)v)
                                ? PropertyPresence.Null
                                : PropertyPresence.Value;
                        }
                        else
                        {
                            propertyPresence = PropertyPresence.Value;
                        }

                        context.Presence = propertyPresence;
                    }
                }

                JsonProperty constructorProperty = context.ConstructorProperty;
                if (constructorProperty == null && context.Property != null)
                {
                    constructorProperty = contract.CreatorParameters.ForgivingCaseSensitiveFind(p => p.PropertyName, context.Property.UnderlyingName);
                }

                if (constructorProperty != null && !constructorProperty.Ignored)
                {
                    // handle giving default values to creator parameters
                    // this needs to happen before the call to creator
                    if (trackPresence)
                    {
                        if (context.Presence == PropertyPresence.None || context.Presence == PropertyPresence.Null)
                        {
                            if (constructorProperty.PropertyContract == null)
                            {
                                constructorProperty.PropertyContract = GetContractSafe(constructorProperty.PropertyType);
                            }

                            if (HasFlag(constructorProperty.DefaultValueHandling.GetValueOrDefault(this.Serializer.DefaultValueHandling), DefaultValueHandling.Populate))
                            {
                                context.Value = EnsureType(
                                    reader,
                                    constructorProperty.GetResolvedDefaultValue(),
                                    CultureInfo.InvariantCulture,
                                    constructorProperty.PropertyContract,
                                    constructorProperty.PropertyType);
                            }
                        }
                    }

                    int i = contract.CreatorParameters.IndexOf(constructorProperty);
                    creatorParameterValues[i] = context.Value;

                    context.Used = true;
                }
            }

            object createdObject = creator(creatorParameterValues);

            if (id != null)
            {
                AddReference(reader, id, createdObject);
            }

            OnDeserializing(reader, contract, createdObject);

            // go through unused values and set the newly created object's properties
            foreach (CreatorPropertyContext context in propertyContexts)
            {
                if (context.Used ||
                    context.Property == null ||
                    context.Property.Ignored ||
                    context.Presence == PropertyPresence.None)
                {
                    continue;
                }

                JsonProperty property = context.Property;
                object value = context.Value;

                if (ShouldSetPropertyValue(property, value))
                {
                    property.ValueProvider.SetValue(createdObject, value);
                    context.Used = true;
                }
                else if (!property.Writable && value != null)
                {
                    // handle readonly collection/dictionary properties
                    JsonContract propertyContract = this.Serializer.ContractResolver.ResolveContract(property.PropertyType);

                    if (propertyContract.ContractType == JsonContractType.Array)
                    {
                        JsonArrayContract propertyArrayContract = (JsonArrayContract)propertyContract;

                        object createdObjectCollection = property.ValueProvider.GetValue(createdObject);
                        if (createdObjectCollection != null)
                        {
                            IWrappedCollection createdObjectCollectionWrapper = propertyArrayContract.CreateWrapper(createdObjectCollection);
                            IWrappedCollection newValues = propertyArrayContract.CreateWrapper(value);

                            foreach (object newValue in newValues)
                            {
                                createdObjectCollectionWrapper.Add(newValue);
                            }
                        }
                    }
                    else if (propertyContract.ContractType == JsonContractType.Dictionary)
                    {
                        JsonDictionaryContract dictionaryContract = (JsonDictionaryContract)propertyContract;

                        object createdObjectDictionary = property.ValueProvider.GetValue(createdObject);
                        if (createdObjectDictionary != null)
                        {
                            IDictionary targetDictionary = (dictionaryContract.ShouldCreateWrapper) ? dictionaryContract.CreateWrapper(createdObjectDictionary) : (IDictionary)createdObjectDictionary;
                            IDictionary newValues = (dictionaryContract.ShouldCreateWrapper) ? dictionaryContract.CreateWrapper(value) : (IDictionary)value;

                            // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
                            IDictionaryEnumerator e = newValues.GetEnumerator();
                            try
                            {
                                while (e.MoveNext())
                                {
                                    DictionaryEntry entry = e.Entry;
                                    targetDictionary.Add(entry.Key, entry.Value);
                                }
                            }
                            finally
                            {
                                (e as IDisposable)?.Dispose();
                            }
                        }
                    }

                    context.Used = true;
                }
            }

            if (contract.ExtensionDataSetter != null)
            {
                foreach (CreatorPropertyContext propertyValue in propertyContexts)
                {
                    if (!propertyValue.Used)
                    {
                        contract.ExtensionDataSetter(createdObject, propertyValue.Name, propertyValue.Value);
                    }
                }
            }

            if (trackPresence)
            {
                foreach (CreatorPropertyContext context in propertyContexts)
                {
                    if (context.Property == null)
                    {
                        continue;
                    }

                    EndProcessProperty(
                        createdObject,
                        reader,
                        contract,
                        reader.Depth,
                        context.Property,
                        context.Presence.GetValueOrDefault(),
                        !context.Used);
                }
            }

            OnDeserialized(reader, contract, createdObject);
            return createdObject;
        }

        private object DeserializeConvertable(JsonConverter converter, JsonReader reader, Type objectType, object existingValue)
        {
            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Started deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
            }

            object value = converter.ReadJson(reader, objectType, existingValue, this);

            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Info)
            {
                this.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Finished deserializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, objectType, converter.GetType())), null);
            }

            return value;
        }

        private List<CreatorPropertyContext> ResolvePropertyAndCreatorValues(JsonObjectContract contract, JsonProperty containerProperty, JsonReader reader, Type objectType)
        {
            List<CreatorPropertyContext> propertyValues = new List<CreatorPropertyContext>();
            bool exit = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string memberName = reader.Value.ToString();

                        CreatorPropertyContext creatorPropertyContext = new CreatorPropertyContext
                        {
                            Name = reader.Value.ToString(),
                            ConstructorProperty = contract.CreatorParameters.GetClosestMatchProperty(memberName),
                            Property = contract.Properties.GetClosestMatchProperty(memberName)
                        };
                        propertyValues.Add(creatorPropertyContext);

                        JsonProperty property = creatorPropertyContext.ConstructorProperty ?? creatorPropertyContext.Property;
                        if (property != null && !property.Ignored)
                        {
                            if (property.PropertyContract == null)
                            {
                                property.PropertyContract = GetContractSafe(property.PropertyType);
                            }

                            JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, contract, containerProperty);

                            if (!ReadForType(reader, property.PropertyContract, propertyConverter != null))
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            if (propertyConverter != null && propertyConverter.CanRead)
                            {
                                creatorPropertyContext.Value = DeserializeConvertable(propertyConverter, reader, property.PropertyType, null);
                            }
                            else
                            {
                                creatorPropertyContext.Value = CreateValueInternal(reader, property.PropertyType, property.PropertyContract, property, contract, containerProperty, null);
                            }

                            continue;
                        }
                        else
                        {
                            if (!reader.Read())
                            {
                                throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                            }

                            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                            {
                                this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}.".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);
                            }

                            if (this.Serializer.MissingMemberHandling == MissingMemberHandling.Error)
                            {
                                throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, objectType.Name));
                            }
                        }

                        if (contract.ExtensionDataSetter != null)
                        {
                            creatorPropertyContext.Value = ReadExtensionDataValue(contract, containerProperty, reader);
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        exit = true;
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!exit && reader.Read());

            return propertyValues;
        }

        public bool ReadForType(JsonReader reader, JsonContract contract, bool hasConverter)
        {
            // don't read properties with converters as a specific value
            // the value might be a string which will then get converted which will error if read as date for example
            if (hasConverter)
            {
                return reader.Read();
            }

            ReadType t = (contract != null) ? contract.InternalReadType : ReadType.Read;

            switch (t)
            {
                case ReadType.Read:
                    return reader.ReadAndMoveToContent();
                case ReadType.ReadAsInt32:
                    reader.ReadAsInt32();
                    break;
                case ReadType.ReadAsDecimal:
                    reader.ReadAsDecimal();
                    break;
                case ReadType.ReadAsDouble:
                    reader.ReadAsDouble();
                    break;
                case ReadType.ReadAsBytes:
                    reader.ReadAsBytes();
                    break;
                case ReadType.ReadAsBoolean:
                    reader.ReadAsBoolean();
                    break;
                case ReadType.ReadAsString:
                    reader.ReadAsString();
                    break;
                case ReadType.ReadAsDateTime:
                    reader.ReadAsDateTime();
                    break;
#if !NET20
                case ReadType.ReadAsDateTimeOffset:
                    reader.ReadAsDateTimeOffset();
                    break;
#endif
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return (reader.TokenType != JsonToken.None);
        }

        public object CreateNewObject(JsonReader reader, JsonObjectContract objectContract, JsonProperty containerMember, JsonProperty containerProperty, string id, string unityGuid, out bool createdFromNonDefaultCreator)
        {
            object newObject = null;

            if (typeof(UnityEngine.Material).IsAssignableFrom(objectContract.UnderlyingType))
            {
                UnityEngine.Material material = new UnityEngine.Material(UnityEngine.Shader.Find("Diffuse"));
                newObject = material;
            }
            else if (typeof(UnityEngine.Shader).IsAssignableFrom(objectContract.UnderlyingType))
            {
                UnityEngine.Shader shader = UnityEngine.Shader.Find("Diffuse");
                newObject = shader;
            }
            else if (typeof(UnityEngine.Texture2D).IsAssignableFrom(objectContract.UnderlyingType))
            {
                UnityEngine.Texture2D texture = UnityEngine.Texture2D.whiteTexture;
                newObject = texture;
            }
            else if (typeof(UnityEngine.GameObject).IsAssignableFrom(objectContract.UnderlyingType))
            {
                UnityEngine.GameObject go = null;
                go = new UnityEngine.GameObject("JSON Loaded GameObject");
                if (SceneReferenceResolver.Current != null)
                {
                    SceneReferenceResolver.Current.Add(go, unityGuid);
                    //SceneReferenceResolver.Current.Add(unityGuid, go);
                }
                newObject = go;
            }
            else if (typeof(UnityEngine.Component).IsAssignableFrom(objectContract.UnderlyingType))
            {
                UnityEngine.GameObject go = null;
                go = new UnityEngine.GameObject(string.Format("JSON Loaded {0} Component", objectContract.UnderlyingType.Name));
                if (SceneReferenceResolver.Current != null && string.IsNullOrEmpty(SceneReferenceResolver.Current.Get(go)))
                {
                    SceneReferenceResolver.Current.Add(go);
                }
                UnityEngine.Component component = go.GetComponent(objectContract.UnderlyingType);
                if (component == null)
                {
                    component = go.AddComponent(objectContract.UnderlyingType);
                }
                if (SceneReferenceResolver.Current != null)
                {
                    SceneReferenceResolver.Current.Add(component, unityGuid);
                    //SceneReferenceResolver.Current.Add(unityGuid, component);
                }
                newObject = component;
            }
            else if (typeof(UnityEngine.ScriptableObject).IsAssignableFrom(objectContract.UnderlyingType))
            {
                newObject = UnityEngine.ScriptableObject.CreateInstance(objectContract.UnderlyingType);
            }
            else if (objectContract.OverrideCreator != null)
            {
                if (objectContract.CreatorParameters.Count > 0)
                {
                    createdFromNonDefaultCreator = true;
                    return CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.OverrideCreator, id);
                }

                newObject = objectContract.OverrideCreator(new object[0]);
            }
            else if (objectContract.DefaultCreator != null &&
                     (!objectContract.DefaultCreatorNonPublic || this.Serializer.ConstructorHandling == ConstructorHandling.AllowNonPublicDefaultConstructor || objectContract.ParameterizedCreator == null))
            {
                // use the default constructor if it is...
                // public
                // non-public and the user has change constructor handling settings
                // non-public and there is no other creator
                newObject = objectContract.DefaultCreator();
            }
            else if (objectContract.ParameterizedCreator != null)
            {
                createdFromNonDefaultCreator = true;
                return CreateObjectUsingCreatorWithParameters(reader, objectContract, containerMember, objectContract.ParameterizedCreator, id);
            }

            if (newObject == null)
            {
                if (!objectContract.IsInstantiable)
                {
                    throw JsonSerializationException.Create(reader, "Could not create an instance of type {0}. Type is an interface or abstract class and cannot be instantiated.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
                }

                throw JsonSerializationException.Create(reader, "Unable to find a constructor to use for type {0}. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.".FormatWith(CultureInfo.InvariantCulture, objectContract.UnderlyingType));
            }

            createdFromNonDefaultCreator = false;
            return newObject;
        }

        public object PopulateObject(object newObject, JsonReader reader, JsonObjectContract contract, JsonProperty member, string id)
        {
            OnDeserializing(reader, contract, newObject);

            // only need to keep a track of properies presence if they are required or a value should be defaulted if missing
            Dictionary<JsonProperty, PropertyPresence> propertiesPresence = (contract.HasRequiredOrDefaultValueProperties || HasFlag(this.Serializer.DefaultValueHandling, DefaultValueHandling.Populate))
                ? contract.Properties.ToDictionary(m => m, m => PropertyPresence.None)
                : null;

            if (id != null)
            {
                AddReference(reader, id, newObject);
            }

            int initialDepth = reader.Depth;

            bool finished = false;
            do
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        {
                            string memberName = reader.Value.ToString();
                            if (CheckPropertyName(reader, memberName))
                            {
                                continue;
                            }

                            try
                            {
                                // attempt exact case match first
                                // then try match ignoring case
                                JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

                                if (property == null)
                                {
                                    if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                                    {
                                        this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);
                                    }

                                    if (this.Serializer.MissingMemberHandling == MissingMemberHandling.Error)
                                    {
                                        throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType.Name));
                                    }

                                    if (!reader.Read())
                                    {
                                        break;
                                    }

                                    SetExtensionData(contract, member, reader, memberName, newObject);
                                    continue;
                                }

                                if (property.Ignored || !ShouldDeserialize(reader, property, newObject))
                                {
                                    if (!reader.Read())
                                    {
                                        break;
                                    }

                                    SetPropertyPresence(reader, property, propertiesPresence);
                                    SetExtensionData(contract, member, reader, memberName, newObject);
                                }
                                else
                                {
                                    if (property.PropertyContract == null)
                                    {
                                        property.PropertyContract = GetContractSafe(property.PropertyType);
                                    }

                                    JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, contract, member);

                                    if (!ReadForType(reader, property.PropertyContract, propertyConverter != null))
                                    {
                                        throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                                    }

                                    SetPropertyPresence(reader, property, propertiesPresence);

                                    // set extension data if property is ignored or readonly
                                    if (!SetPropertyValue(property, propertyConverter, contract, member, reader, newObject))
                                    {
                                        SetExtensionData(contract, member, reader, memberName, newObject);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                                {
                                    HandleError(reader, true, initialDepth);
                                }
                                else
                                {
                                    throw;
                                }
                            }
                            break;
                        }
                    case JsonToken.EndObject:
                        finished = true;
                        break;
                    case JsonToken.Comment:
                        // ignore
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when deserializing object: " + reader.TokenType);
                }
            } while (!finished && reader.Read());

            if (!finished)
            {
                ThrowUnexpectedEndException(reader, contract, newObject, "Unexpected end when deserializing object.");
            }

            if (propertiesPresence != null)
            {
                foreach (KeyValuePair<JsonProperty, PropertyPresence> propertyPresence in propertiesPresence)
                {
                    JsonProperty property = propertyPresence.Key;
                    PropertyPresence presence = propertyPresence.Value;

                    EndProcessProperty(newObject, reader, contract, initialDepth, property, presence, true);
                }
            }

            OnDeserialized(reader, contract, newObject);
            return newObject;
        }

        public object PopulateObjectProperty(object newObject, JsonReader reader, JsonProperty member, string memberName, JsonObjectContract contract)
        {
            if (CheckPropertyName(reader, memberName))
            {
                return null;
            }

            try
            {
                // attempt exact case match first
                // then try match ignoring case
                JsonProperty property = contract.Properties.GetClosestMatchProperty(memberName);

                if (property == null)
                {
                    if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
                    {
                        this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(reader as IJsonLineInfo, reader.Path, "Could not find member '{0}' on {1}".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType)), null);
                    }

                    if (this.Serializer.MissingMemberHandling == MissingMemberHandling.Error)
                    {
                        throw JsonSerializationException.Create(reader, "Could not find member '{0}' on object of type '{1}'".FormatWith(CultureInfo.InvariantCulture, memberName, contract.UnderlyingType.Name));
                    }

                    if (!reader.Read())
                    {
                        return null;
                    }

                    SetExtensionData(contract, member, reader, memberName, newObject);
                    return null;
                }

                if (property.Ignored || !ShouldDeserialize(reader, property, newObject))
                {
                    if (!reader.Read())
                    {
                        return null;
                    }

                    SetExtensionData(contract, member, reader, memberName, newObject);
                }
                else
                {
                    if (property.PropertyContract == null)
                    {
                        property.PropertyContract = GetContractSafe(property.PropertyType);
                    }

                    JsonConverter propertyConverter = GetConverter(property.PropertyContract, property.MemberConverter, contract, member);

                    if (!ReadForType(reader, property.PropertyContract, propertyConverter != null))
                    {
                        throw JsonSerializationException.Create(reader, "Unexpected end when setting {0}'s value.".FormatWith(CultureInfo.InvariantCulture, memberName));
                    }

                    // set extension data if property is ignored or readonly
                    if (!SetPropertyValue(property, propertyConverter, contract, member, reader, newObject))
                    {
                        SetExtensionData(contract, member, reader, memberName, newObject);
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsErrorHandled(newObject, contract, memberName, reader as IJsonLineInfo, reader.Path, ex))
                {
                    HandleError(reader, true, reader.Depth);
                }
                else
                {
                    throw;
                }
            }
            return newObject;
        }

        public bool ShouldDeserialize(JsonReader reader, JsonProperty property, object target)
        {
            if (property.ShouldDeserialize == null)
            {
                return true;
            }

            bool shouldDeserialize = property.ShouldDeserialize(target);

            if (this.TraceWriter != null && this.TraceWriter.LevelFilter >= TraceLevel.Verbose)
            {
                this.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, reader.Path, "ShouldDeserialize result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, shouldDeserialize)), null);
            }

            return shouldDeserialize;
        }

        public bool CheckPropertyName(JsonReader reader, string memberName)
        {
            if (this.Serializer.MetadataPropertyHandling == MetadataPropertyHandling.ReadAhead)
            {
                switch (memberName)
                {
                    case JsonTypeReflector.IdPropertyName:
                    case JsonTypeReflector.UnityRefPropertyName:
                    case JsonTypeReflector.RefPropertyName:
                    case JsonTypeReflector.TypePropertyName:
                    case JsonTypeReflector.ArrayValuesPropertyName:
                        reader.Skip();
                        return true;
                }
            }
            return false;
        }

        public void SetExtensionData(JsonObjectContract contract, JsonProperty member, JsonReader reader, string memberName, object o)
        {
            if (contract.ExtensionDataSetter != null)
            {
                try
                {
                    object value = ReadExtensionDataValue(contract, member, reader);

                    contract.ExtensionDataSetter(o, memberName, value);
                }
                catch (Exception ex)
                {
                    throw JsonSerializationException.Create(reader, "Error setting value in extension data for type '{0}'.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType), ex);
                }
            }
            else
            {
                reader.Skip();
            }
        }

        private object ReadExtensionDataValue(JsonObjectContract contract, JsonProperty member, JsonReader reader)
        {
            object value;
            if (contract.ExtensionDataIsJToken)
            {
                value = JToken.ReadFrom(reader);
            }
            else
            {
                value = CreateValueInternal(reader, null, null, null, contract, member, null);
            }
            return value;
        }

        public void EndProcessProperty(object newObject, JsonReader reader, JsonObjectContract contract, int initialDepth, JsonProperty property, PropertyPresence presence, bool setDefaultValue)
        {
            if (presence == PropertyPresence.None || presence == PropertyPresence.Null)
            {
                try
                {
                    Required resolvedRequired = property._required ?? contract.ItemRequired ?? Required.Default;

                    switch (presence)
                    {
                        case PropertyPresence.None:
                            if (resolvedRequired == Required.AllowNull || resolvedRequired == Required.Always)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' not found in JSON.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }

                            if (setDefaultValue && !property.Ignored)
                            {
                                if (property.PropertyContract == null)
                                {
                                    property.PropertyContract = GetContractSafe(property.PropertyType);
                                }

                                if (HasFlag(property.DefaultValueHandling.GetValueOrDefault(this.Serializer.DefaultValueHandling), DefaultValueHandling.Populate) && property.Writable)
                                {
                                    property.ValueProvider.SetValue(newObject, EnsureType(reader, property.GetResolvedDefaultValue(), CultureInfo.InvariantCulture, property.PropertyContract, property.PropertyType));
                                }
                            }
                            break;
                        case PropertyPresence.Null:
                            if (resolvedRequired == Required.Always)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' expects a value but got null.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }
                            if (resolvedRequired == Required.DisallowNull)
                            {
                                throw JsonSerializationException.Create(reader, "Required property '{0}' expects a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName));
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    if (IsErrorHandled(newObject, contract, property.PropertyName, reader as IJsonLineInfo, reader.Path, ex))
                    {
                        HandleError(reader, true, initialDepth);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public void SetPropertyPresence(JsonReader reader, JsonProperty property, Dictionary<JsonProperty, PropertyPresence> requiredProperties)
        {
            if (property != null && requiredProperties != null)
            {
                PropertyPresence propertyPresence;
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                        propertyPresence = (CoerceEmptyStringToNull(property.PropertyType, property.PropertyContract, (string)reader.Value))
                            ? PropertyPresence.Null
                            : PropertyPresence.Value;
                        break;
                    case JsonToken.Null:
                    case JsonToken.Undefined:
                        propertyPresence = PropertyPresence.Null;
                        break;
                    default:
                        propertyPresence = PropertyPresence.Value;
                        break;
                }

                requiredProperties[property] = propertyPresence;
            }
        }

        public void HandleError(JsonReader reader, bool readPastError, int initialDepth)
        {
            ClearErrorContext();

            if (readPastError)
            {
                reader.Skip();

                while (reader.Depth > (initialDepth + 1))
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                }
            }
        }
    }
}
