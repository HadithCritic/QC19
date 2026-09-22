using System;
using System.Collections.Generic;

namespace Model
{
    public class BrowseHistoryItem : Selection
    {
        public BrowseHistoryItem(Book book, SelectionScope scope, List<int> indexes)
            : base(book, scope, indexes)
        {
        }
    }
}
