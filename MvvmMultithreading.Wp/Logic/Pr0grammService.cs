using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Logic
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Windows.Threading;
    using Constants;
    using Extensions;
    using Microsoft.Phone.Net.NetworkInformation;
    using Model;
    using Newtonsoft.Json;
    using Resources;
    using ViewModel;
    using Newtonsoft.Json.Linq;

    public static class Pr0grammService
    {
        public static event EventHandler<bool> AuthenticationCompleted;
        public static event EventHandler CurrentPostReloaded;

        private static Post lastLoadedNewPost;
        private static Post lastLoadedHotPost;
        private static int LoadCounter;
        private static bool syncTimeSubscribed;
        private static DispatcherTimer syncTimer;

        public static void CheckInternetConnection()
        {
            bool isConnected = NetworkInterface.GetIsNetworkAvailable();
            if (!isConnected)
            {
                ViewModelLocator.ShowNotification(AppResources.NoNetworkConnection, string.Empty);
            }
        }

        public static void LoadTopPosts()
        {
            if (!ViewModelLocator.Main.IsLoading)
            {
                ViewModelLocator.Main.IsLoading = true;
            }

            var tag = ViewModelLocator.Main.SearchTag;
            var flag = Helper.GetFlagFilter();
            var jsonUrl = ApiCalls.HotPosts(flag, tag);

            if (ViewModelLocator.Main.Category == Category.Hot)
            {
                if (ViewModelLocator.Main.Posts.Any())
                {
                    jsonUrl = ApiCalls.HotPostsWithOlderParameter(lastLoadedHotPost.Promoted, flag, tag);
                }
            }
            else if (ViewModelLocator.Main.Category == Category.New)
            {
                if (ViewModelLocator.Main.NewPosts.Any())
                {
                    jsonUrl = ApiCalls.HotPostsWithOlderParameter(lastLoadedNewPost.Id, flag, tag);
                }
            }

            var client = new CookieAwareClient();
            client.DownloadStringCompleted += PostsDownloadCompleted;
            client.DownloadStringAsync(new Uri(jsonUrl));
        }

        public static void LoadUserPosts(string userName, ObservableCollection<Post> listTofill, string apiCall, int listCountToCheck)
        {
            if (listCountToCheck == listTofill.Count)
            {
                return;
            }

            var flag = Helper.GetFlagFilter();

            var jsonUrl = string.Format(apiCall, userName, flag);
            if (listTofill.Any())
            {
                jsonUrl = string.Format(string.Format("{0}&older={1}", apiCall, listTofill.Last().Id), userName, flag);
            }

            var client = new CookieAwareClient();
            client.DownloadStringCompleted += (sender, e) => PostsDownloadCompletedForUser(sender, e, listTofill);
            client.DownloadStringAsync(new Uri(jsonUrl));
        }

        private static void PostsDownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ViewModelLocator.Main.IsLoading = false;
                ViewModelLocator.ShowNotification(AppResources.ErrorOnLoadingPosts, string.Empty);
                return;
            }

            try
            {
                var fagFilter = Helper.GetFagFilter();
                var response = e.Result;
                dynamic jsonObject = JsonConvert.DeserializeObject(response);
                foreach (var post in jsonObject.items)
                {
                    Post postObject = JsonConvert.DeserializeObject<Post>(post.ToString());
                    LastLoadedPost(postObject);

                    if (FilterPost(fagFilter, postObject))
                    {
                        continue;
                    }

                    if (ViewModelLocator.Main.Category == Category.Hot)
                    {
                        if (ViewModelLocator.Main.Posts.FirstOrDefault(x => x.Id == postObject.Id) == null)
                        {
                            ViewModelLocator.Main.Posts.Add(postObject);
                        }
                    }
                    else if (ViewModelLocator.Main.Category == Category.New)
                    {
                        if (ViewModelLocator.Main.NewPosts.FirstOrDefault(x => x.Id == postObject.Id) == null)
                        {
                            ViewModelLocator.Main.NewPosts.Add(postObject);
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (ViewModelLocator.Main.IsStart)
                {
                    ViewModelLocator.Main.LoadPostsCommand.Execute(null);
                }

                return;
            }


            if (ViewModelLocator.Main.Category == Category.Hot)
            {
                ViewModelLocator.Main.ListToUse = ViewModelLocator.Main.Posts;
            }
            else if (ViewModelLocator.Main.Category == Category.New)
            {
                ViewModelLocator.Main.ListToUse = ViewModelLocator.Main.NewPosts;
            }

            ViewModelLocator.Main.IsStart = false;
            ViewModelLocator.Main.IsLoading = false;

            //LoadMorePosts();
        }

        private static void PostsDownloadCompletedForUser(object sender, DownloadStringCompletedEventArgs e, ObservableCollection<Post> listTofill)
        {
            if (e.Error != null)
            {
                ViewModelLocator.ShowNotification(AppResources.ErrorOnLoadingPosts, string.Empty);
                return;
            }
                   

            var fagFilter = Helper.GetFagFilter();
            var response = e.Result;
            dynamic jsonObject = JsonConvert.DeserializeObject(response);

            var userName = string.Empty;

            foreach (var post in jsonObject.items)
            {
                Post postObject = JsonConvert.DeserializeObject<Post>(post.ToString());

                if (FilterPost(fagFilter, postObject))
                {
                    continue;
                }

                if (listTofill.FirstOrDefault(x => x.Id == postObject.Id) == null)
                {
                    listTofill.Add(postObject);
                }
            }
        }

        private static void LoadMorePosts()
        {
            if (LoadCounter >= 15)
            {
                LoadCounter = 0;
                ViewModelLocator.Main.IsLoading = false;
                ViewModelLocator.ShowNotification(AppResources.NoOtherPostsFound, string.Empty);
                return;
            }

            if (!LoadMorePostsIfNotEnough())
            {
                LoadCounter = 0;
                ViewModelLocator.Main.IsLoading = false;
            }
        }

        private static bool FilterPost(FagFilter fagFilter, Post postObject)
        {
            if (fagFilter == FagFilter.Schwuchtel)
            {
                if (postObject.Mark != 0)
                {
                    return true;
                }
            }
            else if (fagFilter == FagFilter.Neuschwuchtel)
            {
                if (postObject.Mark != 1)
                {
                    return true;
                }
            }

            if (!Helper.StringToBoolean(ViewModelLocator.Settings.CanLoadPictures)
                && !postObject.Image.EndsWith(".webm")
                && !postObject.Image.EndsWith(".gif"))
            {
                return true;
            }

            if (!Helper.StringToBoolean(ViewModelLocator.Settings.CanLoadGifs)
                && postObject.Image.EndsWith(".gif"))
            {
                return true;
            }

            if (!Helper.StringToBoolean(ViewModelLocator.Settings.CanLoadWebms)
                && postObject.Image.EndsWith(".webm"))
            {
                return true;
            }

            var points = postObject.Up - postObject.Down;
            var targetPoints = 0;
            if (!string.IsNullOrEmpty(ViewModelLocator.Settings.PostPoints))
            {
                targetPoints = Convert.ToInt32(ViewModelLocator.Settings.PostPoints);
            }

            if (Helper.StringToBoolean(ViewModelLocator.Settings.OnlyPositivePosts) && points <= targetPoints)
            {
                return true;
            }

            postObject.Thumb = string.Format("{0}{1}", ApiCalls.ThumbUrl, postObject.Thumb);
            postObject.Image = string.Format("{0}{1}", ApiCalls.ImageUrl, postObject.Image);
            postObject.ReadableCreatedTime = Helper.ConvertJsonDateToDateTime(postObject.Created).AddHours(2);
            return false;
        }

        public static async Task<bool> ReloadCurrentPost(CurrentPostViewModel currentPostViewModel)
        {
            var currentPost = currentPostViewModel.Post;
            if (currentPost == null)
            {
                return false;
            }

            var jsonUrl = string.Format(ApiCalls.CommonPostInfoUrl, currentPost.Id);
            var client = new CookieAwareClient();

            client.DownloadProgressChanged += (sender, args) =>
            {
                if (currentPost.CancellationTokenSource != null && currentPost.CancellationTokenSource.IsCancellationRequested)
                {
                    client.CancelAsync();
                }
            };

            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    return;
                }

                try
                {
                    var response = e.Result;
                    dynamic jsonObject = JsonConvert.DeserializeObject(response);
                    var post = jsonObject.items[0];
                    Post postObject = JsonConvert.DeserializeObject<Post>(post.ToString());
                    currentPostViewModel.Post = postObject;
                    postObject.Thumb = string.Format("{0}{1}", ApiCalls.ThumbUrl, postObject.Thumb);
                    postObject.Image = string.Format("{0}{1}", ApiCalls.ImageUrl, postObject.Image);
                    postObject.ReadableCreatedTime = Helper.ConvertJsonDateToDateTime(postObject.Created).AddHours(2);

                    CurrentPostReloaded.Invoke(null, null);
                }
                catch
                {
                }
            };

            client.DownloadStringAsync(new Uri(jsonUrl));
            return true;
        }

        public static void LoadPost(CurrentPostViewModel currentPostViewModel)
        {
            try
            {
                var post = currentPostViewModel.Post;
                if (post == null)
                {
                    return;
                }

                post.CancellationTokenSource = new CancellationTokenSource();
                var jsonUrl = string.Format("{0}", ApiCalls.PostInfo(post.Id));
                var client = new CookieAwareClient();
                client.DownloadStringCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        return;
                    }

                    var response = args.Result;
                    currentPostViewModel.PostInfo = PostInfo.FromJson(response);
                    currentPostViewModel.PostInfo.Tags = new ObservableCollection<TagInfo>(currentPostViewModel.PostInfo.Tags.OrderByDescending(x => x.Confidence));

                    foreach (var comment in currentPostViewModel.PostInfo.Comments)
                    {
                        comment.IsUserOriginalPoster = comment.Name.Equals(post.User);
                    }

                    try
                    {
                        RecursiveLoadComments(currentPostViewModel.PostInfo.Comments);
                    }
                    catch
                    {
                    }

                    if (currentPostViewModel.Post != null && currentPostViewModel.Post.CancellationTokenSource != null)
                    {
                        currentPostViewModel.Post.CancellationTokenSource.Dispose();
                        currentPostViewModel.Post.CancellationTokenSource = null;
                    }
                };

                client.DownloadProgressChanged += (sender, args) =>
                {
                    if (post.CancellationTokenSource != null && post.CancellationTokenSource.IsCancellationRequested)
                    {
                        client.CancelAsync();
                    }
                };

                try
                {
                    client.DownloadStringAsync(new Uri(jsonUrl), post.CancellationTokenSource);
                }
                catch
                {
                    return;
                }
            }
            catch
            {
            }
        }

        public static Task<UserProfile> LoadUserProfileAsync(string userName)
        {
            try
            {
                var jsonUrl = string.Format(ApiCalls.UserUrl, userName);
                var client = new CookieAwareClient(true);
                var tcs = new TaskCompletionSource<UserProfile>();

                client.DownloadStringCompleted += (s, e) =>
                {
                    if (e.Error != null)
                    {
                        tcs.SetException(e.Error);
                        return;
                    }

                    var response = e.Result;
                    var user = UserProfile.FromJson(response);

                    tcs.SetResult(user);
                };

                client.DownloadStringAsync(new Uri(jsonUrl));
                return tcs.Task;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<bool> LoadVotes(string id)
        {
            try
            {
                var jsonUrl = string.Format(ApiCalls.SyncVotesUrl, id);
                var client = new CookieAwareClient(true);
                client.DownloadStringCompleted += (sender, args) =>
                {
                    if (args.Error != null)
                    {
                        return;
                    }

                    var response = args.Result;

                    ViewModelLocator.VoteObject = VoteObject.FromJson(response);
                    if (!ViewModelLocator.Authentication.IsAuthenticated)
                    {
                        dynamic jsonString = JsonConvert.DeserializeObject(HttpUtility.UrlDecode(ViewModelLocator.Authentication.Me));
                        var name = jsonString.n;
                        ViewModelLocator.ShowNotification(string.Format("{0}, {1}", AppResources.Hello, name), string.Empty);
                        ViewModelLocator.Authentication.IsAuthenticated = true;

                        ViewModelLocator.Main.InboxCount = ViewModelLocator.VoteObject.InboxCount;

                        InitializeSyncTimer();
                        AuthenticationCompleted.Invoke(null, true);
                    }
                };

                client.DownloadStringAsync(new Uri(jsonUrl));

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void ClearPosts()
        {
            ViewModelLocator.Main.Posts.Clear();
            ViewModelLocator.Main.NewPosts.Clear();
            lastLoadedHotPost = null;
            lastLoadedNewPost = null;
        }

        public static async Task<bool> AuthenticateAsync(string userName, string pw)
        {
            var cookies = new CookieContainer();
            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("name", userName),
                new KeyValuePair<string, string>("password", pw)
            };

            Debug.WriteLine("AuthenticateAsync called: {0}", DateTime.Now);

            try
            {
                var httpClient = new HttpClient(new HttpClientHandler { CookieContainer = cookies });
                var response = await httpClient.PostAsync(ApiCalls.AuthUrl, new FormUrlEncodedContent(values));
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                ViewModelLocator.ShowNotification(AppResources.ErrorOnLogIn, string.Empty);
                return false;
            }
            
            var uri = new Uri(ApiCalls.AuthUrl);
            IEnumerable<Cookie> responseCookies = cookies.GetCookies(uri).Cast<Cookie>();

            try
            {
                var firstCookieJson = responseCookies.First().Value;
                ViewModelLocator.Authentication.Me = firstCookieJson;

                var secondCookieValue = responseCookies.ElementAt(1).Value;
                ViewModelLocator.Authentication.PP = secondCookieValue;
                ViewModelLocator.Authentication.IsAuthenticated = true;

                dynamic jsonString = JsonConvert.DeserializeObject(Uri.UnescapeDataString(firstCookieJson));

                var id = jsonString.id;
                var nonce = id.Value.Substring(0, 16);
                ViewModelLocator.Authentication.Id = nonce;

                var name = jsonString.n;
                ViewModelLocator.Authentication.UserName = name;
                ViewModelLocator.ShowNotification(string.Format("{0}, {1}", AppResources.Hello, name), string.Empty);

                InitializeSyncTimer();

                var client = new CookieAwareClient();
                client.DownloadStringCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                    {
                        ViewModelLocator.Authentication.Mark = "1";
                        return;
                    }

                    dynamic jsonResult = JsonConvert.DeserializeObject(e.Result);
                    ViewModelLocator.Authentication.Mark = jsonResult.user.mark;
                };

                client.DownloadStringAsync(new Uri(string.Format(ApiCalls.UserUrl, name)));

                return true;
            }
            catch
            {
                ViewModelLocator.Authentication.IsAuthenticated = false;
                ViewModelLocator.ShowNotification(AppResources.ErrorOnLogIn, string.Empty);
                return false;
            }
        }

        public static void DeSubscribeSyncTimer()
        {
            if (syncTimeSubscribed && syncTimer != null)
            {
                syncTimer.Stop();
                syncTimer = null;
                syncTimeSubscribed = false;
            }
        }

        private static void InitializeSyncTimer()
        {
            if (!syncTimeSubscribed)
            {
                syncTimer = new DispatcherTimer();
                syncTimer.Interval = TimeSpan.FromSeconds(180);
                syncTimer.Tick += SyncTimerOnTick;
                syncTimeSubscribed = true;
                syncTimer.Start();
            }
        }

        private static async void SyncTimerOnTick(object sender, EventArgs eventArgs)
        {
            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                return;
            }

            var successfull = await LoadVotes(ViewModelLocator.VoteItemViewModel.LastId.ToString());
            if (successfull)
            {
                if (ViewModelLocator.VoteObject.InboxCount > ViewModelLocator.Main.InboxCount)
                {
                    var dif = ViewModelLocator.VoteObject.InboxCount - ViewModelLocator.Main.InboxCount;
                    ViewModelLocator.ShowNotification(string.Format(AppResources.NewMessagesCount, dif), null);
                }

                ViewModelLocator.Main.InboxCount = ViewModelLocator.VoteObject.InboxCount;
                AuthenticationCompleted.Invoke(null, false);
            }
        }

        public static async Task<bool> Comment(string comment, string id, string parentId)
        {
            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                return false;
            }

            try
            {
                var cookies = new CookieContainer();
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("comment", comment),
                    new KeyValuePair<string, string>("itemId", id),
                    new KeyValuePair<string, string>("parentId", parentId)
                };

                await AuthPost(cookies, values, ApiCalls.CommentUrl);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> VoteComment(string commentId, Vote vote)
        {
            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                return false;
            }

            var likeUnlikeString = string.Empty;
            if (vote == Vote.Up)
            {
                likeUnlikeString = "1";
            }
            else if (vote == Vote.Down)
            {
                likeUnlikeString = "-1";
            }
            else if (vote == Vote.Neutral)
            {
                likeUnlikeString = "0";
            }

            try
            {
                var cookies = new CookieContainer();
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("id", commentId),
                    new KeyValuePair<string, string>("vote", likeUnlikeString)
                };

                await AuthPost(cookies, values, ApiCalls.CommentVoteUrl);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> VotePost(string postId, Vote vote)
        {
            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                return false;
            }

            var likeUnlikeString = string.Empty;
            if (vote == Vote.Up)
            {
                likeUnlikeString = "1";
            }
            else if (vote == Vote.Down)
            {
                likeUnlikeString = "-1";
            }
            else if (vote == Vote.Neutral)
            {
                likeUnlikeString = "0";
            }
            else if (vote == Vote.Favorite)
            {
                likeUnlikeString = "2";
            }

            try
            {
                var cookies = new CookieContainer();
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("id", postId),
                    new KeyValuePair<string, string>("vote", likeUnlikeString)
                };

                await AuthPost(cookies, values, ApiCalls.PostVoteUrl);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> AddTags(CurrentPostViewModel currentPostViewModel, string tags)
        {
            try
            {
                var cookies = new CookieContainer();
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("itemId", currentPostViewModel.Post.Id.ToString()),
                    new KeyValuePair<string, string>("tags", tags)
                };

                await AuthPost(cookies, values, ApiCalls.AddTagsUrl);

                var splittedTags = tags.Split(',');
                foreach (var splittedTag in splittedTags)
                {
                    currentPostViewModel.PostInfo.Tags.Add(new TagInfo()
                    {
                        Confidence = 0,
                        Id = 0,
                        Tag = splittedTag
                    });
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> VoteTag(TagInfo tagInfo, Vote vote)
        {
            try
            {
                var voteNumber = "0";
                if (vote == Vote.Down)
                {
                    voteNumber = "-1";
                }
                if (vote == Vote.Up)
                {
                    voteNumber = "1";
                }
                if (vote == Vote.Neutral)
                {
                    voteNumber = "0";
                }

                var cookies = new CookieContainer();
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("id", tagInfo.Id.ToString()),
                    new KeyValuePair<string, string>("vote", voteNumber)
                };

                await AuthPost(cookies, values, ApiCalls.VoteTagUrl);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> UploadPicFromUrl(string imageUrl, string key, string sfwstatus, string tags)
        {
            imageUrl = string.Empty;
            sfwstatus = "sfw";
            tags = "zug, unfall, knapp, sfw"; //sfwstatus hier auch rein <-
            //Tags: Komma separiert
            //sfwstatus: nsfw, nsfl oder sfw

            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                return false;
            }

            try
            {
                var cookies = new CookieContainer();
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("imageUrl", imageUrl),
                    new KeyValuePair<string, string>("key", key),
                    new KeyValuePair<string, string>("sfwstatus", sfwstatus),
                    new KeyValuePair<string, string>("tags", tags)
                };

                cookies.Add(new Uri(ApiCalls.CommentUrl), new Cookie("pp", ViewModelLocator.Authentication.PP));
                cookies.Add(new Uri(ApiCalls.CommentUrl), new Cookie("me", ViewModelLocator.Authentication.Me));
                var httpClient = new HttpClient(new HttpClientHandler { CookieContainer = cookies });
                var response = await httpClient.PostAsync(ApiCalls.UploadPicFromUrlUrl, new FormUrlEncodedContent(values));
                response.EnsureSuccessStatusCode();
                var a = response.Content.ReadAsStringAsync();

                dynamic json = JsonConvert.DeserializeObject(a.Result);
                var selfPosted = json.selfPosted;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static async Task<HttpContent> AuthPost(CookieContainer cookies, IEnumerable<KeyValuePair<string, string>> values, string url)
        {
            Debug.WriteLine("Authpost called: {0}", DateTime.Now);

            var postValues = values as List<KeyValuePair<string, string>>;
            postValues.Add(new KeyValuePair<string, string>("_nonce", ViewModelLocator.Authentication.Id));

            cookies.Add(new Uri(url), new Cookie("pp", ViewModelLocator.Authentication.PP));
            cookies.Add(new Uri(url), new Cookie("me", ViewModelLocator.Authentication.Me));
            
            //cookies.Add(new Uri(url), new Cookie("_nonce", ViewModelLocator.Authentication.Id));
            var httpClient = new HttpClient(new HttpClientHandler { CookieContainer = cookies });
            //httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:32.0) Gecko/20100101 Firefox/32.0");
            var response = await httpClient.PostAsync(url, new FormUrlEncodedContent(postValues));
            response.EnsureSuccessStatusCode();
            return response.Content;
        }

        private static void RecursiveLoadComments(ObservableCollection<Comment> comments)
        {
            foreach (var itemcomment in comments.Where(x => x.Parent == 0).OrderBy(x => x.Up - x.Down).ToList())
            {
                comments.Remove(itemcomment);
                comments.Insert(0, itemcomment);

                LoadSubComments(comments, itemcomment, 1);
            }
        }

        private static void LoadSubComments(ObservableCollection<Comment> comments, Comment itemcomment, int level)
        {
            var itemLevel = level;
            var orderedCommentsList = comments.Where(x => x.Parent == itemcomment.Id).OrderBy(x => x.Up - x.Down).ToList();
            foreach (var subComment in orderedCommentsList)
            {
                comments.Remove(subComment);
                var indexOfParent = comments.IndexOf(comments.FirstOrDefault(x => x.Id == subComment.Parent)) + 1;

                comments.Insert(indexOfParent, subComment);
                subComment.Level = itemLevel;

                var newItemLevel = itemLevel + 1;
                LoadSubComments(comments, subComment, newItemLevel);
            }
        }

        private static void LastLoadedPost(Post postObject)
        {
            if (ViewModelLocator.Main.Category == Category.Hot)
            {
                lastLoadedHotPost = postObject;
            }
            else if (ViewModelLocator.Main.Category == Category.New)
            {
                lastLoadedNewPost = postObject;
            }
        }

        private static bool LoadMorePostsIfNotEnough()
        {
            LoadCounter++;
            if (ViewModelLocator.Main.Category == Category.Hot)
            {
                if (ViewModelLocator.Main.Posts.Count < 25)
                {
                    LoadTopPosts();
                    return true;
                }

                return false;
            }
            else if (ViewModelLocator.Main.Category == Category.New)
            {
                if (ViewModelLocator.Main.NewPosts.Count < 25)
                {
                    LoadTopPosts();
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}
