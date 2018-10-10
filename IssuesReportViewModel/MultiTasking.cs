using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvanWatchMyissues.Helpers;
using System.Globalization;

namespace AvanWatchMyissues.ViewModel
{
   
    public partial class IssuesReportViewModel : INotifyPropertyChanged
    {
        
        BackgroundWorker _bwLogin = new BackgroundWorker
        {
            WorkerSupportsCancellation = true
        };
        BackgroundWorker _bwReleaseStory = new BackgroundWorker
        {
            WorkerSupportsCancellation = true
        };
        BackgroundWorker _bwReport = new BackgroundWorker
        {
            WorkerSupportsCancellation = true
        };
        private void bw_LoginDoWork(object sender, DoWorkEventArgs e)
        {
            LoggedIn = IssueReportHelper.Login(txtUserID, txtPassword);

            DataAccess.Repository allrep = new DataAccess.Repository();
            allrep.Name = "ALL REPOSITORIES";
            cmbRepositories.Add(allrep);

            cmbRepositories.AddRange(GetRepositories());
        }
        private void bw_LoginRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            InProgress = false;
        }
        private void bw_ReportDoWork(object sender, DoWorkEventArgs e)
        {
            PopulateGrid();
        }
        private void bw_ReportRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            InProgress = false;
        }
    }
}
