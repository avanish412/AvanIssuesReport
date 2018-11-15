using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace AvanWatchMyissues.Model
{
    public enum ReportType
    {
        LAST_2_DAYS=2,
        LAST_7_DAYS=7,
        LAST_15_DAYS=15,
        LAST_30_DAYS=30,
        LAST_100_DAYS=100,
        LAST_180_DAYS=180,
        LAST_365_DAYS=365,
        ALL_ISSUES=1000
    }

    public class IssueReport
    {
        public string Repository { get; set; }
        public int TotalOpenIssues { get; set; }
        public int TotalPullRequest { get; set; }
    }
    public class Issue
    {
        public string Repository { get; set; }
        public int Number { get; set; }

        public string Labels { get; set; }
        public bool IsPullRequest { get; set; }
        public Uri IDLink { get; set; }
        public string Title{ get; set; }
        public string Description { get; set; }
        public string User { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreationAge { get; set; }
        public DateTime LastCommentedAt { get; set; }
        public int LastCommentedAge { get; set; }
        public string Assignee{ get; set; }
        public int Comments { get; set; }
        public string AllComments { get; set; }
        public string CommentedBy { get; set; }

    }
    [DataContract]
    public class Comment
    {
        [DataMember(Name = "user")]
        public User user { get; set; }

        [DataMember(Name = "body")]
        public string body { get; set; }
        [DataMember(Name = "created_at")]
        public string created_at { get; set; }        
    }
    [DataContract]
    public class User
    {
        [DataMember(Name = "login")]
        public string login { get; set; }
    }
}
