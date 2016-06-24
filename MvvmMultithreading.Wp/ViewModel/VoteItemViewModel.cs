namespace Pr0gramm.ViewModel
{
    using System.Collections.Generic;
    using Model;

    public class VoteItemViewModel
    {
        private IList<VoteObjectItem> items = new List<VoteObjectItem>();
        public int LastId { get; set; }

        public IList<VoteObjectItem> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
            }
        }
    }
}