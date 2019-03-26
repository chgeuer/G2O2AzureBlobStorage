namespace G2OAzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using Microsoft.WindowsAzure.Storage;
    using Newtonsoft.Json;

    [Export(typeof(G2OProxyConfiguration))]
    public class G2OProxyConfiguration
    {
        [Import("GetConfiguration")]
        public Func<string, string> GetConfiguration { get; set; }

        #region Keys

        public string GetKey(string nonce)
        {
            return this.Keys[nonce];
        }

        Dictionary<string, string> _keys;
        public Dictionary<string, string> Keys
        {
            get
            {
                if (_keys == null)
                {
                    var g2o_nonces = GetConfiguration("g2o_nonces");
                    _keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(g2o_nonces);
                }

                return _keys;
            }
        }

        #endregion

        #region Storage

        List<Account> _storage;
        public List<Account> Storage
        {
            get
            {
                if (_storage == null)
                {
                    var g2o_storage = GetConfiguration("g2o_storage");
                    _storage = JsonConvert.DeserializeObject<List<Account>>(g2o_storage);
                }

                return _storage;
            }
        }

        #endregion

        public class Account
        {
            public CloudStorageAccount CloudStorageAccount { get { return CloudStorageAccount.Parse(connectionString: this.ConnectionString); } }

            public string Alias { get; set; }
            public string ConnectionString { get; set; }
            public List<string> Containers { get; set; }
        }
    }
}