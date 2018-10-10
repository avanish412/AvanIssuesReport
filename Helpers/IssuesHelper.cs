
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvanWatchMyissues.Model;
using AvanWatchMyissues.DataAccess;
using System.IO;
using System.Security.Cryptography;
using Octokit;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace AvanWatchMyissues.Helpers
{
    public class IssueReportHelper
    {
        public static GitHubClient GithubClient { get; set; }
        public static string WorksspaceRef { get; set; }
        public static string ProjectRef { get; set; }
        public static List<DataAccess.Repository> _repolist = null;
        public static bool Login(string user, string password)
        {
            GithubClient = new GitHubClient(new ProductHeaderValue("WatchMyIssues"));
            if (GithubClient != null)
            {
                //var basicAuth = new Credentials("85a9572e0549281e0231e59dc2851ae8c8bc447a");
                var basicAuth = new Credentials(user,password);
                GithubClient.Credentials = basicAuth;

                if(GithubClient.User != null)
                    return true;
            }
            return false;
        }
        public static void LogOut()
        {
            GithubClient = null;
        }
        public static async Task<List<DataAccess.Repository>> GetRepositories()
        {
            List<DataAccess.Repository> repos = new List<DataAccess.Repository>();

            var repositoriesForAnet = await GithubClient.Repository.GetAllForOrg("AuthorizeNet");

            foreach (Octokit.Repository rep in repositoriesForAnet)
            {
                DataAccess.Repository repo = new DataAccess.Repository();
                repo.Name = rep.Name;
                repo.owner = rep.Owner.Login;

                repos.Add(repo);
            }
            _repolist = new List<DataAccess.Repository>(repos);

            return await Task.Run(() => new List<DataAccess.Repository>(repos));
        }
        public static async Task<List<Model.Issue>> GetIssues(DataAccess.Repository selectedrepo,ReportType reportype)
        {
            List<Model.Issue> issues = new List<Model.Issue>();

            var recently = new RepositoryIssueRequest
            {
                Filter = IssueFilter.All,
                Since = DateTimeOffset.Now.Subtract(TimeSpan.FromDays((int)reportype))
            };

            if (selectedrepo.Name.CompareTo("ALL REPOSITORIES") == 0)
            {
                foreach (DataAccess.Repository repo in _repolist)
                {
                    var issuesForAnet = await GithubClient.Issue.GetAllForRepository("AuthorizeNet", repo.Name, recently);
                    //var issuesForAnet = await GithubClient.PullRequest.GetAllForRepository("AuthorizeNet", repo.Name, recently);

                    foreach (Octokit.Issue issue in issuesForAnet)
                    {
                        Model.Issue iss = new Model.Issue();

                        iss.Assignee = issue.Assignee != null ? issue.Assignee.Name : "";
                        iss.Comments = issue.Comments;
                        iss.IsPullRequest = (issue.PullRequest != null);

                        if (iss.Comments > 0)
                        {
                            var commentsforissue = await GithubClient.Issue.Comment.GetAllForIssue(repo.owner, repo.Name, issue.Number);
                            DateTime lasttime = DateTime.Now;
                            foreach (IssueComment cmnt in commentsforissue)
                            {
                                iss.CommentedBy += cmnt.User.Login + ",";
                                lasttime = cmnt.UpdatedAt.Value.DateTime;

                                iss.AllComments += cmnt.User.Login + "\t\t\t\t" + lasttime.ToString()+"\n\n"+cmnt.Body +"\n\n";
                            }
                            iss.LastCommentedAt = lasttime;
                            iss.LastCommentedAge = (int)(DateTime.Now - iss.LastCommentedAt).TotalDays;
                        }
                        iss.Number = issue.Number;
                        iss.IDLink = issue.HtmlUrl;
                        iss.Repository = repo.Name;// issue.Repository!=null? issue.Repository.Name:"";
                        iss.Title = issue.Title;
                        iss.Description = issue.Body;
                        iss.CreatedAt = issue.CreatedAt.DateTime;
                        iss.User = issue.User.Login;
                        iss.CreationAge = (int)(DateTime.Now - issue.CreatedAt).TotalDays;
                        issues.Add(iss);
                    }
                }
            }
            else
            {
                var issuesForAnet = await GithubClient.Issue.GetAllForRepository("AuthorizeNet", selectedrepo.Name, recently);

                foreach (Octokit.Issue issue in issuesForAnet)
                {
                    Model.Issue iss = new Model.Issue();

                    iss.Assignee = issue.Assignee != null ? issue.Assignee.Name : "";
                    iss.Comments = issue.Comments;
                    iss.IsPullRequest = (issue.PullRequest != null);
                    if (iss.Comments>0)
                    {
                        var commentsforissue = await GithubClient.Issue.Comment.GetAllForIssue(selectedrepo.owner,selectedrepo.Name,issue.Number);
                        DateTime lasttime = DateTime.Now;
                        foreach ( IssueComment cmnt in commentsforissue)
                        {
                            iss.CommentedBy += cmnt.User.Login + ",";
                            lasttime = cmnt.UpdatedAt.Value.DateTime;

                            iss.AllComments += cmnt.User.Login + "\t\t\t\t" + lasttime.ToString() + "\n\n" + cmnt.Body+"\n\n";
                        }
                        iss.LastCommentedAt = lasttime;
                        iss.LastCommentedAge = (int)(DateTime.Now - iss.LastCommentedAt).TotalDays;                        
                    }
                    
                    iss.Number = issue.Number;
                    iss.IDLink = issue.HtmlUrl;
                    iss.Repository = selectedrepo.Name;// issue.Repository!=null? issue.Repository.Name:"";
                    iss.Title = issue.Title;
                    iss.Description = issue.Body;
                    iss.CreatedAt = issue.CreatedAt.DateTime;
                    iss.User = issue.User.Login;
                    iss.CreationAge = (int)(DateTime.Now - issue.CreatedAt).TotalDays;
                    issues.Add(iss);
                }
            }        

            return issues;
        }
        public static string EncryptString(string ClearText)
        {

            byte[] clearTextBytes = Encoding.UTF8.GetBytes(ClearText);

            System.Security.Cryptography.SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();

            MemoryStream ms = new MemoryStream();
            byte[] rgbIV = Encoding.ASCII.GetBytes("ryojvlzmdalyglrj");
            byte[] key = Encoding.ASCII.GetBytes("hcxilkqbbhczfeultgbskdmaunivmfuo");
            CryptoStream cs = new CryptoStream(ms, rijn.CreateEncryptor(key, rgbIV),
       CryptoStreamMode.Write);

            cs.Write(clearTextBytes, 0, clearTextBytes.Length);

            cs.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        public static string DecryptString(string EncryptedText)
        {
            byte[] encryptedTextBytes = Convert.FromBase64String(EncryptedText);

            MemoryStream ms = new MemoryStream();

            System.Security.Cryptography.SymmetricAlgorithm rijn = SymmetricAlgorithm.Create();


            byte[] rgbIV = Encoding.ASCII.GetBytes("ryojvlzmdalyglrj");
            byte[] key = Encoding.ASCII.GetBytes("hcxilkqbbhczfeultgbskdmaunivmfuo");

            CryptoStream cs = new CryptoStream(ms, rijn.CreateDecryptor(key, rgbIV),
            CryptoStreamMode.Write);

            cs.Write(encryptedTextBytes, 0, encryptedTextBytes.Length);

            cs.Close();

            return Encoding.UTF8.GetString(ms.ToArray());

        }
    }
}
