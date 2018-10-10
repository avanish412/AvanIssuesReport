using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvanWatchMyissues.Model;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace AvanWatchMyissues.DataAccess
{
    [DataContract]
    public class IssuesConfig
    {
        [DataMember(Name = "credential")]
        public Credential credential { get; set; }

        [DataMember(Name = "repositories")]
        public Repository[] repositories { get; set; }

        [DataMember(Name = "selectedrepository")]
        public Repository selectedrepository { get; set; }

        public IssuesConfig()
        {
            credential = new Credential();
        }
    }
    [DataContract]
    public class Credential
    {
        [DataMember(Name = "userid")]
        public string userid { get; set; }
        [DataMember(Name = "password")]
        public string password { get; set; }
    }
    [DataContract]
    public class Repository
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "owner")]
        public string owner { get; set; }
        [DataMember(Name = "githubissues")]
        public GithubIssue [] githubissues { get; set; }
    }
    [DataContract]
    public class GithubIssue
    {
        [DataMember(Name = "number")]
        public int number { get; set; }
        [DataMember(Name = "assignee")]
        public string assignee { get; set; }
    }
    public class ConfigAccess
    {
        
        const string configFile = "\\issues.conf";

        string _configfile;
        FileStream _fileStream = null;
        DataContractJsonSerializer _jsonSerializer = null;
        IssuesConfig _rallyConfig = new IssuesConfig();

        public Credential IssuesCredential
        {
            get { return _rallyConfig.credential; }
            set { _rallyConfig.credential = value; }
        }
        public Repository SelectedRepository
        {
            get { return _rallyConfig.selectedrepository; }
            set { _rallyConfig.selectedrepository = value; }
        }

        public List<Repository> Repositories
        {
            get { return _rallyConfig.repositories.ToList(); }
            set { _rallyConfig.repositories = value.ToArray(); }
        }

        public ConfigAccess()
        {             
            _configfile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + configFile;            
            _jsonSerializer = new DataContractJsonSerializer(typeof(IssuesConfig));
        }
        public bool SavetoConfigData()
        {
            try {
                _fileStream = new FileStream(_configfile, FileMode.Open, FileAccess.Write);
                _fileStream.SetLength(0);
                _jsonSerializer.WriteObject(_fileStream, _rallyConfig);
                _fileStream.Close();
            }
            catch(SerializationException ex)
            {
                return false;
            }

            return true;
        }
        public bool LoadConfigData()
        {
            try {
                _fileStream = new FileStream(_configfile, FileMode.Open, FileAccess.Read);
                object objResponse = _jsonSerializer.ReadObject(_fileStream);

                IssuesConfig jsonResponse = objResponse as IssuesConfig;

                if (jsonResponse == null)
                    return false;
                //////Credential
                _rallyConfig.credential.userid = jsonResponse.credential.userid;
                _rallyConfig.credential.password = jsonResponse.credential.password;

                //////All Repositories
                _rallyConfig.repositories = jsonResponse.repositories;

                //////Selected Repository
                _rallyConfig.selectedrepository = jsonResponse.selectedrepository;

                _fileStream.Close();
                return true;
            }
            catch(Exception ex)
            {
                string error = ex.ToString();
            }
            return false;
        }
    }
}
