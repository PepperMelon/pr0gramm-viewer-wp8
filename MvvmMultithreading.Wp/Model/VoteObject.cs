namespace Pr0gramm.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using ViewModel;

    public class VoteObject
    {
        public int InboxCount;
        public IEnumerable<int> Log { get; set; }
        public int LastId { get; set; }
        public int Ts;
        public object Cache;
        public int Rt;
        public int Qc;

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static VoteObject FromJson(string json)
        {
            var itemsList = ViewModelLocator.VoteItemViewModel.Items;
            var voteObject = JsonConvert.DeserializeObject<VoteObject>(json);

            if (voteObject.LastId != 0)
            {
                ViewModelLocator.VoteItemViewModel.LastId = voteObject.LastId;
            }

            if (itemsList == null)
            {
                itemsList = new List<VoteObjectItem>();
            }

            for (int i = 0; i < voteObject.Log.Count(); i = i + 2)
            {
                var id = voteObject.Log.ElementAt(i);
                var typeId = voteObject.Log.ElementAt(i + 1);

                var voteItem = new VoteObjectItem();
                voteItem.Id = id;
                CheckVoteItem(typeId, voteItem);

                var itemAlreadyExists = itemsList.FirstOrDefault(x => x.Id == id);
                if (itemAlreadyExists != null)
                {
                    itemAlreadyExists.Vote = voteItem.Vote;
                    continue;
                }

                itemsList.Add(voteItem);
            }

            return voteObject;
        }

        private static void CheckVoteItem(int typeId, VoteObjectItem voteItem)
        {
            switch (typeId)
            {
                case 1:
                    voteItem.Vote = Vote.Down;
                    voteItem.VoteType = VoteType.Post;
                    break;
                case 2:
                    voteItem.Vote = Vote.Neutral;
                    voteItem.VoteType = VoteType.Post;
                    break;
                case 3:
                    voteItem.Vote = Vote.Up;
                    voteItem.VoteType = VoteType.Post;
                    break;
                case 4:
                    voteItem.Vote = Vote.Down;
                    voteItem.VoteType = VoteType.Comment;
                    break;
                case 5:
                    voteItem.Vote = Vote.Neutral;
                    voteItem.VoteType = VoteType.Comment;
                    break;
                case 6:
                    voteItem.Vote = Vote.Up;
                    voteItem.VoteType = VoteType.Comment;
                    break;
                case 7:
                    voteItem.Vote = Vote.Down;
                    voteItem.VoteType = VoteType.Tag;
                    break;
                case 8:
                    voteItem.Vote = Vote.Neutral;
                    voteItem.VoteType = VoteType.Tag;
                    break;
                case 9:
                    voteItem.Vote = Vote.Up;
                    voteItem.VoteType = VoteType.Tag;
                    break;
                case 10:
                    voteItem.Vote = Vote.Favorite;
                    voteItem.VoteType = VoteType.Post;
                    break;
                default:
                    voteItem.Vote = Vote.Neutral;
                    break;
            }
        }
    }

    public class VoteObjectItem
    {
        public int Id { get; set; }

        public Vote Vote { get; set; }

        public VoteType VoteType { get; set; }
    }

    public enum VoteType
    {
        Post,
        Comment,
        Tag
    }

    public enum Vote
    {
        Down,
        Neutral,
        Up,
        Favorite
    }
}