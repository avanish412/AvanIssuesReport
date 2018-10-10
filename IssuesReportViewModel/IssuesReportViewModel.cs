using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvanWatchMyissues.Model;
using AvanWatchMyissues.DataAccess;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AvanWatchMyissues.Helpers;

namespace AvanWatchMyissues.ViewModel
{
    public partial class IssuesReportViewModel : INotifyPropertyChanged
    {
        private ConfigAccess _cfgAcccess = new ConfigAccess();
        private string _userid="";
        private string _password="";
        private bool _loggedin = false;
        List<Repository> _repolist = null;
        List<Issue> _griditemlist = null;
        List<IssueReport> _gridissuereportitemlist = null;
        private Repository _repository = null;
        private ReportType _reporttype = ReportType.ALL_ISSUES;
        private bool _inProgress = false;
        private int _totalIssues = 0;
        private int _totalPullRequests = 0;
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand ResetCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand PopulateCommand { get; set; }
        public RelayCommand SelectedProjectChangedCommand { get; set; }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public string txtUserID
        {
            get { return _userid; }
            set
            {
                _userid = value;
                NotifyPropertyChanged("txtUserID");
            }
        }
        public string txtPassword
        {
            get { return _password; }
            set
            {
                    _password = value;
                    NotifyPropertyChanged("txtPassword");
            }
        }
        public List<Repository> cmbRepositories
        {
            get { return _repolist; }
            set
            {
                _repolist = value;
                NotifyPropertyChanged("cmbRepositories");
            }
        }
        
        public List<Issue> gridItems
        {
            get { return _griditemlist; }
            set
            {
                _griditemlist = value;
                NotifyPropertyChanged("gridItems");
            }
        }

        public List<IssueReport> gridIssueReportItems
        {
            get { return _gridissuereportitemlist; }
            set
            {
                _gridissuereportitemlist = value;
                NotifyPropertyChanged("gridIssueReportItems");
            }
        }
        public Repository SelectedRepository
        {
            get { return _repository; }
            set
            {
                _repository = value;
                NotifyPropertyChanged("SelectedRepository");
            }
        }
        public ReportType SelectedReportType
        {
            get { return _reporttype; }
            set
            {
                _reporttype = value;
                NotifyPropertyChanged("SelectedReportType");
            }
        }
        public bool LoggedIn
        {
            get { return _loggedin; }
            set
            {
                _loggedin = value;
                NotifyPropertyChanged("LoggedIn");
            }
        }
        public int TotalIssues
        {
            get { return _totalIssues; }
            set
            {
                _totalIssues = value;
                NotifyPropertyChanged("TotalIssues");
            }
        }
        public int TotalPullRequests
        {
            get { return _totalPullRequests; }
            set
            {
                _totalPullRequests = value;
                NotifyPropertyChanged("TotalPullRequests");
            }
        }
        public bool InProgress
        {
            get { return _inProgress; }
            set
            {
                _inProgress = value;
                NotifyPropertyChanged("InProgress");
            }
        }
        public void Initialize()
        {
            if (_cfgAcccess.LoadConfigData())
            {
                txtUserID = IssueReportHelper.DecryptString(_cfgAcccess.IssuesCredential.userid);
                txtPassword = IssueReportHelper.DecryptString(_cfgAcccess.IssuesCredential.password);
                cmbRepositories = _cfgAcccess.Repositories;

                SelectedRepository = _cfgAcccess.SelectedRepository;
            }
        }
        public IssuesReportViewModel()
        {
            Initialize();

            LoginCommand = new RelayCommand(Login);
            ResetCommand = new RelayCommand(Reset);
            CloseCommand = new RelayCommand(Close);
            PopulateCommand = new RelayCommand(Populate);
            SelectedProjectChangedCommand = new RelayCommand(SelectedProjectChanged);
            _repolist = new List<Repository>();
            ///Login Thread
            _bwLogin.DoWork += bw_LoginDoWork;
            _bwLogin.RunWorkerCompleted += bw_LoginRunWorkerCompleted;

            ///Generate Report Thread
            _bwReport.DoWork += bw_ReportDoWork;
            _bwReport.RunWorkerCompleted += bw_ReportRunWorkerCompleted;
        }
        void Login(object parameter)
        {
            InProgress = true;
            _bwLogin.RunWorkerAsync();
        }
        void Reset(object parameter)
        {
            IssueReportHelper.LogOut(); 
            LoggedIn = false;
            cmbRepositories = new List<Repository>();
            gridItems = null;
            _totalIssues = 0;
        }
        void SelectedProjectChanged(object parameter)
        {
            /*InProgress = true;
            _bwReleaseStory.RunWorkerAsync();*/
        }
        void Close(object parameter)
        {
            SaveWindowInfo();
        }
        void Populate(object parameter)
        {
            InProgress = true;
            _bwReport.RunWorkerAsync();
        }
        public List<Repository> GetRepositories()
        {
            return IssueReportHelper.GetRepositories().Result;
        }
        public List<Issue> GetIssues()
        {
            return IssueReportHelper.GetIssues(SelectedRepository,SelectedReportType).Result;
        }
        public void PopulateGrid()
        {
            gridItems = null;
            gridIssueReportItems = null;
            TotalIssues = 0;
            TotalPullRequests = 0;
            List<Issue> strlist = new List<Issue>();
            List<IssueReport> strReportlist = new List<IssueReport>();

            strlist = GetIssues();
            int nIndex = 0;
            IssueReport issueReport = new IssueReport();
            foreach(Issue iss in strlist)
            {
                //Issue report ////////////////////////////////////
                if (iss.Repository.Equals(issueReport.Repository) != true)
                {
                    if(nIndex != 0)
                        strReportlist.Add(issueReport);
                    issueReport = null;
                }
                if (issueReport == null)
                    issueReport = new IssueReport();
                
                
                issueReport.Repository = iss.Repository;
                if (iss.IsPullRequest == true)
                    issueReport.TotalPullRequest++;
                else
                    issueReport.TotalOpenIssues++;

                ///////////////////////////////////////////////////

                int nCount = 0;

                if (iss.IsPullRequest == true)
                    TotalPullRequests++;
                else
                    TotalIssues++;

                
                foreach (Repository rep in _cfgAcccess.Repositories)
                {
                    if (iss.Repository.CompareTo(rep.Name) == 0 && _cfgAcccess.Repositories[nCount].githubissues !=null)
                    {
                        foreach(GithubIssue ghissue in _cfgAcccess.Repositories[nCount].githubissues)
                        {
                            if (strlist[nIndex].Number == ghissue.number)
                            {
                                strlist[nIndex].Assignee = ghissue.assignee;
                                break;
                            }
                        }
                    }
                    nCount++;
                }
                nIndex++;
            }
            gridItems = strlist;
            gridIssueReportItems = strReportlist;
        }
        public void SaveWindowInfo()
        {
            _cfgAcccess.IssuesCredential.userid = IssueReportHelper.EncryptString(txtUserID);
            _cfgAcccess.IssuesCredential.password = IssueReportHelper.EncryptString(txtPassword);

            List<Repository> repositories = new List<Repository>(_cfgAcccess.Repositories);
            
            foreach (Issue issue in gridItems)
            {
                int nIndex = 0;
                foreach (Repository rep in _cfgAcccess.Repositories)
                {
                    if (issue.Repository.CompareTo(rep.Name) == 0)
                    {
                        int nCount = 0;
                        bool bExist = false;
                        if (rep.githubissues != null)
                        {
                            foreach (GithubIssue gitissue in rep.githubissues)
                            {
                                if (gitissue.number == issue.Number)
                                {
                                    _cfgAcccess.Repositories[nIndex].githubissues[nCount].assignee = issue.Assignee;
                                    bExist = true;
                                    break;
                                }
                                nCount++;
                            }
                        }
                        if (!bExist)
                        {
                            GithubIssue []array = _cfgAcccess.Repositories[nIndex].githubissues;
                            int newsize = _cfgAcccess.Repositories[nIndex].githubissues != null? _cfgAcccess.Repositories[nIndex].githubissues.Length+1:1;
                            Array.Resize(ref array, newsize);
                            GithubIssue ghIssue = new GithubIssue();
                            ghIssue.assignee = issue.Assignee;
                            ghIssue.number = issue.Number;
                            array[newsize - 1] = ghIssue;
                            _cfgAcccess.Repositories[nIndex].githubissues = array;
                        }
                        else
                            continue;
                    }
                    nIndex++;
                }                
            }
            //_cfgAcccess.Repositories = cmbRepositories;

            _cfgAcccess.SelectedRepository = SelectedRepository;

            _cfgAcccess.SavetoConfigData();
        }
    }
}
