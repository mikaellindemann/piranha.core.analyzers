/*
 * Copyright (c) 2020 Mikael Lindemann
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 *
 * https://github.com/piranhacms/piranha.core.analyzers
 *
 */

namespace Piranha.Analyzers
{
    internal static class Constants
    {
        internal static class Namespaces
        {
            internal const string PiranhaExtendFields = "Piranha.Extend.Fields";
        }

        internal static class Types
        {
            internal static class Piranha
            {
                internal static class AttributeBuilder
                {
                    internal const string PageTypeAttribute = "Piranha.AttributeBuilder.PageTypeAttribute";
                    internal const string PostTypeAttribute = "Piranha.AttributeBuilder.PostTypeAttribute";
                    internal const string SiteTypeAttribute = "Piranha.AttributeBuilder.SiteTypeAttribute";
                }

                internal static class Extend
                {
                    internal const string FieldAttribute = "Piranha.Extend.FieldAttribute";
                    internal const string FieldSettingsAttribute = "Piranha.Extend.FieldSettingsAttribute";
                    internal const string RegionAttribute = "Piranha.Extend.RegionAttribute";
                    internal const string StringFieldSettingsAttribute = "Piranha.Extend.StringFieldSettingsAttribute";

                    internal static class Fields
                    {
                        internal const string AudioField = "Piranha.Extend.Fields.AudioField";
                        internal const string CheckBoxField = "Piranha.Extend.Fields.CheckBoxField";
                        internal const string DateField = "Piranha.Extend.Fields.DateField";
                        internal const string DocumentField = "Piranha.Extend.Fields.DocumentField";
                        internal const string ImageField = "Piranha.Extend.Fields.ImageField";
                        internal const string MediaField = "Piranha.Extend.Fields.MediaField";
                        internal const string NumberField = "Piranha.Extend.Fields.NumberField";
                        internal const string PageField = "Piranha.Extend.Fields.PageField";
                        internal const string PostField = "Piranha.Extend.Fields.PostField";
                        internal const string StringField = "Piranha.Extend.Fields.StringField";
                        internal const string VideoField = "Piranha.Extend.Fields.VideoField";
                    }
                }

                internal static class Models
                {
                    internal const string Page = "Piranha.Models.Page";
                    internal const string Post = "Piranha.Models.Post";
                    internal const string SiteContent = "Piranha.Models.SiteContent";
                }
            }
        }
    }
}
