﻿using System.Collections.ObjectModel;

namespace FlickrNet
{
    /// <summary>
    /// A list of <see cref="ActivityItem"/> items.
    /// </summary>
    public sealed class ActivityItemCollection : Collection<ActivityItem>, IFlickrParsable
    {
        void IFlickrParsable.Load(System.Xml.XmlReader reader)
        {
            if (reader.LocalName != "items")
                UtilityMethods.CheckParsingException(reader);

            reader.Read();

            while (reader.LocalName == "item")
            {
                var item = new ActivityItem();
                ((IFlickrParsable)item).Load(reader);
                Add(item);
            }

            reader.Skip();
        }
    }
}
