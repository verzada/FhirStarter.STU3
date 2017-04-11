/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Hl7.Fhir.Rest;

namespace Spark.Engine.Extensions
{
    public static class HttpRequestExtensions
    {
        public static void Replace(this HttpHeaders headers, string header, string value)
        {
            //if (headers.Exists(header)) 
            headers.Remove(header);
            headers.Add(header, value);
        }

        public static string GetParameter(this HttpRequestMessage request, string key)
        {
            foreach (var param in request.GetQueryNameValuePairs())
            {
                if (param.Key == key) return param.Value;
            }
            return null;
        }

        private static List<Tuple<string, string>> TupledParameters(this HttpRequestMessage request)
        {
            var list = new List<Tuple<string, string>>();

            var query = request.GetQueryNameValuePairs();
            foreach (var pair in query)
            {
                list.Add(new Tuple<string, string>(pair.Key, pair.Value));
            }
            return list;
        }

        public static SearchParams GetSearchParams(this HttpRequestMessage request)
        {
            var parameters = request.TupledParameters().Where(tp => tp.Item1 != "_format");
            var searchCommand = SearchParams.FromUriParamList(parameters);
            return searchCommand;
        }
    }
}