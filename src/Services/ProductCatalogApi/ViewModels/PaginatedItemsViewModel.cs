using System.Collections.Generic;

namespace ProductCatalogApi.ViewModels
{
    public class PaginatedItemsViewModel<TEnt> where TEnt: class
    {
        public int PageSize { get; private set; }
        public int PageIndex { get; private set; }

        public int Count { get; private set; }
        public IEnumerable<TEnt> Data { get; set; }

        public PaginatedItemsViewModel(int pageSize, int pageIndex, int count, IEnumerable<TEnt> data)
        {
            PageSize = pageSize;
            PageIndex = pageIndex;
            Count = count;
            Data = data;
        }
    }
}
