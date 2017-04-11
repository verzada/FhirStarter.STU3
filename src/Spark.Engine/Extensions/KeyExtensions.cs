/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Spark.Engine.Core;

// mh: KeyExtensions terugverplaatst naar Spark.Engine.Core omdat ze in dezelfde namespace moeten zitten als Key.
namespace Spark.Engine.Extensions
{

    public static class KeyExtensions
    {
        public static Key ExtractKey(this Resource resource)
        {
            var _base = resource.ResourceBase != null ? resource.ResourceBase.ToString() : null;
            var key = new Key(_base, resource.TypeName, resource.Id, resource.VersionId);
            return key;
        }

        public static void ApplyTo(this IKey key, Resource resource)
        {
            resource.ResourceBase = key.HasBase() ?  new Uri(key.Base) : null;
            resource.Id = key.ResourceId;
            resource.VersionId = key.VersionId; 
        }

        private static Key Clone(this IKey self)
        {
            var key = new Key(self.Base, self.TypeName, self.ResourceId, self.VersionId);
            return key;
        }

        private static bool HasBase(this IKey key)
        {
            return !string.IsNullOrEmpty(key.Base);
        }

        public static Key WithoutBase(this IKey self)
        {
            var key = self.Clone();
            key.Base = null;
            return key;
        }

        private static IEnumerable<string> GetSegments(this IKey key)
        {
            if (key.Base != null) yield return key.Base;
            if (key.TypeName != null) yield return key.TypeName;
            if (key.ResourceId != null) yield return key.ResourceId;
            if (key.VersionId != null)
            {
                yield return "_history";
                yield return key.VersionId;
            }
        }

        public static string ToUriString(this IKey key)
        {
            var segments = key.GetSegments();
            return string.Join("/", segments);
        }
    }
}
