using System;

namespace Network.Models
{
    [Serializable]
    public class FirebaseConfigData
    {
        public string databaseURL;
        public ServiceAccountData serviceAccount;
    }

    [Serializable]
    public class ServiceAccountData
    {
        public string type;
        public string project_id;
        public string private_key_id;
        public string private_key;
        public string client_email;
        public string client_id;
        public string auth_uri;
        public string token_uri;
        public string auth_provider_x509_cert_url;
        public string client_x509_cert_url;
        public string universe_domain;
    }
}
