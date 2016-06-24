using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Pr0gramm.View
{
    using System.Collections.ObjectModel;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using Extensions;
    using Logic;
    using Microsoft.Phone.Tasks;
    using Model;
    using Resources;
    using ViewModel;

    public partial class CommentNavControl : UserControl
    {
        private VoteObjectItem voteObjectItem;

        public static readonly DependencyProperty CurrentPostViewModelProperty =
            DependencyProperty.Register("CurrentPostViewModel", typeof(CurrentPostViewModel), typeof(CommentNavControl), new PropertyMetadata(default(CurrentPostViewModel)));

        public CurrentPostViewModel CurrentPostViewModel
        {
            get { return (CurrentPostViewModel)GetValue(CurrentPostViewModelProperty); }
            set { SetValue(CurrentPostViewModelProperty, value); }
        }
        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(Comment), typeof(CommentNavControl), new PropertyMetadata(default(Comment), PropertyChangedCallback));

        private async static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var commentNavControl = dependencyObject as CommentNavControl;
            commentNavControl.MenuContainer.Visibility = Visibility.Visible;
            commentNavControl.CommentContainer.Visibility = Visibility.Collapsed;

            if (commentNavControl.Comment == null)
            {
                return;
            }

            if (ViewModelLocator.VoteObject == null)
            {
                return;
            }

            var richtTextBox = commentNavControl.CommentTextBox;
            SetLinkedText(richtTextBox, commentNavControl.Comment.Content);

            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                return;
            }
            
            var votingPost =
                ViewModelLocator.VoteItemViewModel.Items.Where(x => x.VoteType == VoteType.Comment)
                    .FirstOrDefault(x => x.Id == commentNavControl.Comment.Id);

            if (votingPost == null)
            {
                NeutralizeUi(commentNavControl);
                return;
            }

            if (votingPost.Vote == Vote.Neutral)
            {
                NeutralizeUi(commentNavControl);
            }
            else if (votingPost.Vote == Vote.Up)
            {
                commentNavControl.voteUpButton.Background = new SolidColorBrush(Colors.Gray);
                commentNavControl.VotetAlreadyUp = true;
                commentNavControl.VotetAlreadyDown = false;
            }
            else if (votingPost.Vote == Vote.Down)
            {
                commentNavControl.voteDownButton.Background = new SolidColorBrush(Colors.Gray);
                commentNavControl.VotetAlreadyUp = false;
                commentNavControl.VotetAlreadyDown = true;
            }

            commentNavControl.PointsText.Text = new UpDownVotePointCoverter().Convert(commentNavControl.Comment, null, null, null).ToString();

        }

        private static void SetVoteForComment(Comment comment, Vote vote)
        {
            var votingPost =
                ViewModelLocator.VoteItemViewModel.Items.Where(x => x.VoteType == VoteType.Comment)
                    .FirstOrDefault(x => x.Id == comment.Id);
            if (votingPost != null)
            {
                votingPost.Vote = vote;
            }
            else
            {
                ViewModelLocator.VoteItemViewModel.Items.Add(new VoteObjectItem(){ Id = comment.Id, Vote = vote, VoteType = VoteType.Comment});
            }
        }

        private static void NeutralizeUi(CommentNavControl commentNavControl)
        {
            commentNavControl.PointsText.Text =
                new UpDownVotePointCoverter().Convert(commentNavControl.Comment, null, null, null).ToString();
            commentNavControl.VotetAlreadyUp = false;
            commentNavControl.VotetAlreadyDown = false;
            commentNavControl.voteDownButton.Background =
                new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
            commentNavControl.voteUpButton.Background =
                new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
        }

        public bool VotetAlreadyUp { get; set; }

        public bool VotetAlreadyDown { get; set; }

        public Comment Comment
        {
            get { return (Comment)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        public CommentNavControl()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;

            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                voteDownButton.Visibility = Visibility.Collapsed;
                voteUpButton.Visibility = Visibility.Collapsed;
                CommentButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void VoteTap(object sender, GestureEventArgs e)
        {
            if (VotetAlreadyUp)
            {
                await NeutralizeCommentVote();
                return;
            }

            var success = await Pr0grammService.VoteComment(this.Comment.Id.ToString(), Vote.Up);
            if (success)
            {
                if (VotetAlreadyDown)
                {
                    Comment.Down--;
                }

                Comment.Up++;
                SetVoteForComment(Comment, Vote.Up);
                PointsText.Text = new UpDownVotePointCoverter().Convert(Comment, null, null, null).ToString();
                voteUpButton.Background = new SolidColorBrush(Colors.Gray);
                voteDownButton.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
                VotetAlreadyUp = true;
                VotetAlreadyDown = false;
            }
        }

        private async void DownTap(object sender, GestureEventArgs e)
        {
            if (VotetAlreadyDown)
            {
                await NeutralizeCommentVote();
                return;
            }

            var success = await Pr0grammService.VoteComment(this.Comment.Id.ToString(), Vote.Down);
            if (success)
            {
                if (VotetAlreadyUp)
                {
                    Comment.Up--;
                }

                Comment.Down++;
                SetVoteForComment(Comment, Vote.Down);
                PointsText.Text = new UpDownVotePointCoverter().Convert(Comment, null, null, null).ToString();
                voteDownButton.Background = new SolidColorBrush(Colors.Gray);
                voteUpButton.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
                VotetAlreadyDown = true;
                VotetAlreadyUp = false;
            }
        }

        private void CommentTap(object sender, GestureEventArgs e)
        {
            this.MenuContainer.Visibility = Visibility.Collapsed;
            this.CommentContainer.Visibility = Visibility.Visible;
            this.CommentTextbox.Focus();
        }

        private void CopyTap(object sender, GestureEventArgs e)
        {
            Clipboard.SetText(this.Comment.Content);
        }

        private async Task NeutralizeCommentVote()
        {
            var success = await Pr0grammService.VoteComment(this.Comment.Id.ToString(), Vote.Neutral);
            if (success)
            {
                if (VotetAlreadyUp)
                {
                    Comment.Up--;
                }
                if (VotetAlreadyDown)
                {
                    Comment.Down--;
                }

                SetVoteForComment(Comment, Vote.Neutral);
                PointsText.Text = new UpDownVotePointCoverter().Convert(Comment, null, null, null).ToString();
                voteUpButton.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
                voteDownButton.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
                VotetAlreadyUp = false;
                VotetAlreadyDown = false;
            }
        }

        private async void SendTap(object sender, GestureEventArgs e)
        {
            var successfully = await Pr0grammService.Comment(this.CommentTextbox.Text, CurrentPostViewModel.Post.Id.ToString(), this.Comment.Id.ToString());
            if (successfully)
            {
                var mark = Convert.ToInt32(ViewModelLocator.Authentication.Mark);
                var comment = new Comment()
                {
                    Content = this.CommentTextbox.Text,
                    Down = 0,
                    Up = 1,
                    Level = Comment.Level + 1,
                    Mark = mark,
                    ReadableCreatedTime = DateTime.Now,
                    Name = ViewModelLocator.Authentication.UserName
                };

                var comments = CurrentPostViewModel.PostInfo.Comments;
                var indexOfParenComment = comments.IndexOf(Comment);
                CurrentPostViewModel.PostInfo.Comments.Insert(indexOfParenComment + 1, comment);
            }
            else
            {
                ViewModelLocator.ShowNotification(AppResources.ErrorOnSending, string.Empty);
            }

            this.CommentTextbox.Text = string.Empty;
            this.MenuContainer.Visibility = Visibility.Visible;
            this.CommentContainer.Visibility = Visibility.Collapsed;
        }

        private void CancelSendTap(object sender, GestureEventArgs e)
        {
            this.CommentTextbox.Text = string.Empty;
            this.MenuContainer.Visibility = Visibility.Visible;
            this.CommentContainer.Visibility = Visibility.Collapsed;
        }

        public static void SetLinkedText(RichTextBox richTextBlock, string htmlFragment)
        {
            var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            richTextBlock.Blocks.Clear();

            int nextOffset = 0;

            foreach (Match match in linkParser.Matches(htmlFragment))
            {
                if (match.Index >= nextOffset)
                {
                    AppendText(richTextBlock, htmlFragment.Substring(nextOffset, match.Index - nextOffset));

                    try
                    {
                        AppendLink(richTextBlock, match.Value, new Uri(match.Value));
                        nextOffset = match.Index + match.Length;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            AppendLink(richTextBlock, match.Value, new Uri(match.Value.Replace("www.", "https://")));
                            nextOffset = match.Index + match.Length;
                        }
                        catch
                        {
                        }
                    }
                }
            }

            AppendText(richTextBlock, htmlFragment.Substring(nextOffset));
        }

        public static void AppendText(RichTextBox richTextBlock, string text)
        {
            Paragraph paragraph;

            if (richTextBlock.Blocks.Count == 0 ||
                (paragraph = richTextBlock.Blocks[richTextBlock.Blocks.Count - 1] as Paragraph) == null)
            {
                paragraph = new Paragraph();
                richTextBlock.Blocks.Add(paragraph);
            }

            paragraph.Inlines.Add(new Run { Text = text });
        }

        public static void AppendLink(RichTextBox richTextBlock, string text, Uri uri)
        {
            Paragraph paragraph;

            if (richTextBlock.Blocks.Count == 0 ||
                (paragraph = richTextBlock.Blocks[richTextBlock.Blocks.Count - 1] as Paragraph) == null)
            {
                paragraph = new Paragraph();
                richTextBlock.Blocks.Add(paragraph);
            }

            var run = new Run { Text = text };
            var link = new Hyperlink { NavigateUri = uri, TargetName = "_blank" };
            link.Command = new DummyCommand(uri);

            link.Inlines.Add(run);
            paragraph.Inlines.Add(link);
        }

        public class DummyCommand : ICommand
        {
            private readonly Uri link;

            public DummyCommand(Uri link)
            {
                this.link = link;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                var webBrowserTask = new WebBrowserTask();
                webBrowserTask.Uri = link;
                webBrowserTask.Show();
            }

            public event EventHandler CanExecuteChanged;
        }
    }
}
