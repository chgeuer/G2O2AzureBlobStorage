namespace G2O
{
    using System;
    using System.Diagnostics;
    using System.Linq;

    [DebuggerDisplay("X-Akamai-G2O-Auth-Data: {Value}")]
    public class G2OData
    {
        public class Signature
        {
            public G2OData Fields { get; private set; }
            public string Data { get; private set; }
            public string Value { get; private set; }

            public Signature(string data, string value)
            {
                this.Data = data;
                this.Value = value;

                try { this.Fields = new G2OData(this.Data); }
                catch (Exception) { this.Fields = null; }
            }

            public bool IsValidDataStructure { get { return this.Fields != null; } }

            public bool Validate(string path, Func<string, string> keyResolver)
            {
                var nonceValue = keyResolver(this.Fields.Nonce);

                var recreatedSignature = G2OData.CreateSignature(
                    version: this.Fields.Version,
                    edgeIP: this.Fields.EdgeIP,
                    clientIP: this.Fields.ClientIP,
                    time: this.Fields.Time,
                    uniqueId: this.Fields.UniqueID,
                    nonce: this.Fields.Nonce,
                    nonceValue: nonceValue,
                    path: path);

                return this.Value.Equals(recreatedSignature.Value);
            }
        }

        public static Signature CreateSignature(int version, string edgeIP, string clientIP, DateTimeOffset time, string uniqueId, 
            string nonce, string nonceValue, string path)
        {
            var data = new G2OData(version, edgeIP, clientIP, time, uniqueId, nonce);

            var sign = G2OAlgorithms.ComputeSignatureValue(data.Version, nonceValue, data.Value, path);

            return new Signature(data: data.Value, value: sign);
        }

        #region Properties

        /// <summary>
        /// Indicates the encryption format for the authentication.
        /// </summary>
        public readonly int Version; 
        
        /// <summary>
        /// The edge server’s IP address.
        /// </summary>
        public readonly string EdgeIP;

        /// <summary>
        /// The IP address of the client. This value will be the first public IP address in the X-Forwarded-For header. If there is no X-F-F header, or the values in the header are not valid public IP addresses, the connecting client IP addresss is used.
        /// </summary>
        public readonly string ClientIP;

        /// <summary>
        /// The current epoch time (for example 1008979391, for Fri Dec 21 16:03:11 2001)
        /// </summary>
        public readonly DateTimeOffset Time; 

        /// <summary>
        /// A unique ID with some randomness that will guarantee uniqueness for header generated at the same time for multiple requests
        /// </summary>
        public readonly string UniqueID; 

        /// <summary>
        /// A simple string or number that tells the origin which key to use to authenticate the request. This makes it possible to transition from one key to another; during the transition the origin server will need to support more than one key.
        /// </summary>
        public readonly string Nonce;

        public string Value
        {
            get
            {
                return string.Join(", ", this.Version, this.EdgeIP, this.ClientIP, ((long)this.Time.Subtract(epoch).TotalSeconds).ToString(), this.UniqueID, this.Nonce);
            }
        }

        #endregion 

        private static readonly DateTimeOffset epoch = new DateTime(year: 1970, month: 1, day: 1, hour: 0, minute: 0, second: 0, kind: DateTimeKind.Utc);

        public G2OData(int version, string edgeIP, string clientIP, DateTimeOffset time, string uniqueId, string nonce) 
        {
            this.Version = version;
            this.EdgeIP = edgeIP;
            this.ClientIP = clientIP;
            this.Time = time;
            this.UniqueID = uniqueId;
            this.Nonce = nonce;
        }
            
        public G2OData(string value)
        {
            if (string.IsNullOrEmpty(value)) { throw new ArgumentNullException("value"); }

            // version, edge-ip, client-ip, time, unique-id, nonce
            string[] segments = value.Replace(" ", "").Split(new[] { "," }, StringSplitOptions.None).ToArray();
            if (segments.Length != 6) { throw new ArgumentException("must contain 6 segments", "value"); }

            this.Version = int.Parse(segments[0]);
            this.EdgeIP = segments[1]; // IPAddress.Parse(
            this.ClientIP = segments[2]; // IPAddress.Parse(
            this.Time = epoch.AddSeconds(long.Parse(segments[3]));
            this.UniqueID = segments[4];
            this.Nonce = segments[5];
        }
    }
}